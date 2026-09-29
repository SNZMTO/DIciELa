using Diciela.Data;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La pose d'une pièce sur un personnage habillé (SAVOIR, 2.11), dans les axes du personnage : son pivot au point du
    /// repère de son emplacement sur la tête choisie, plus son décalage et son réglage sur cette tête (ou ce corps) ;
    /// sur l'axe de contact, là où sa face touche la tête ou le corps. Elle prend sa rotation, puis sa taille. Chaque
    /// moitié d'une paire d'oreilles s'écarte (ou se rapproche) d'autant que les oreilles de la tête choisie sont plus
    /// écartées que celles de la tête où la paire a été réglée (plus le x de son décalage et de son réglage), puis la paire
    /// se pose par sa base : là où elle touchait sa tête de référence, elle touche la tête choisie, le reste s'enfonce ;
    /// elle glisse vers le milieu jusqu'à ce que sa base porte à plat.
    /// </summary>
    internal static class PiecePlacement
    {
        // Une paire posée par sa base glisse vers le milieu, par pas de tant de mètres, au plus de tant, jusqu'à ce que sa
        // base porte à plat sur la tête choisie (aucun point ne s'y enfonce de plus que la tolérance) : sur une tête plus
        // ronde que la sienne, elle déborderait du crâne (des oreilles décollées).
        private const float InwardStep = 0.01f;
        private const float MaxInward = 0.08f;
        private const float Flush = 0.03f;

        /// <summary>Pose cette pièce, enfant du personnage, pour cette apparence.</summary>
        public static void Place(Transform piece, Appearance appearance, AttachmentDefinition attachment)
        {
            bool pair = IsPair(piece);
            Vector3 scale = attachment.ScaleOn(appearance.Head, appearance.Body);
            Vector3 offset = attachment.Offset + attachment.AdjustmentOn(appearance.Head, appearance.Body);
            float spread = 0f;
            // Le x d'une paire l'écarte, chaque moitié de son côté.
            if (pair)
            {
                spread = SpreadOf(appearance.Head, attachment) + offset.x;
                offset.x = 0f;
            }
            piece.localPosition = pair && TryRest(appearance.Head, attachment, offset, ref spread, out Vector3 rest)
                ? rest
                : PlaceOf(appearance, attachment, offset, scale, spread);
            piece.localRotation = Quaternion.Euler(attachment.RotationOn(appearance.Head, appearance.Body)) *
                                  piece.localRotation;
            piece.localScale = scale;
            if (pair)
                SpreadHalves(piece, spread);
        }

        // De combien les oreilles de la tête choisie sont plus écartées que celles de la tête de référence, de chaque
        // côté ; zéro hors des oreilles.
        private static float SpreadOf(HeadDefinition head, AttachmentDefinition attachment)
        {
            HeadDefinition reference = attachment.Reference;
            if (attachment.Slot.Frame != AttachmentFrame.Ears || reference == null)
                return 0f;
            return head.EarRight.x - reference.EarRight.x;
        }

        // Une paire posée par sa base sur une autre tête que sa tête de référence : là où elle touchait sa tête de
        // référence, elle touche la tête choisie (Contact.TryRest), et elle glisse vers le milieu jusqu'à porter à plat
        // (spread reçoit son écart). Faux sur sa tête de référence, ou sans surfaces.
        private static bool TryRest(HeadDefinition head, AttachmentDefinition attachment, Vector3 offset, ref float spread,
            out Vector3 rest)
        {
            AttachmentSlotDefinition slot = attachment.Slot;
            HeadDefinition reference = attachment.Reference;
            rest = default;
            if (reference == null || reference == head || slot.Contact != ContactAxis.Down)
                return false;
            // Sa place sur sa tête de référence, où elle n'est écartée que du x de son décalage.
            float referenceSpread = attachment.Offset.x;
            Vector3 onReference = reference.PointOf(slot.Frame) + new Vector3(0f, attachment.Offset.y, attachment.Offset.z);
            SurfaceMap top = head.SurfaceOf(ContactAxis.Down), referenceTop = reference.SurfaceOf(ContactAxis.Down);
            if (!Contact.TryTouch(ContactAxis.Down, referenceTop, attachment.Shape, onReference, attachment.Scale,
                    out float touch, referenceSpread))
                return false;
            onReference.y = touch + attachment.Offset.y;
            rest = head.PointOf(slot.Frame) + offset;
            float bestAlong = 0f, bestUneven = float.MaxValue, bestSpread = spread;
            for (float inward = 0f; inward <= MaxInward + InwardStep * 0.5f; inward += InwardStep)
            {
                if (!Contact.TryRest(top, referenceTop, attachment.Shape, rest, onReference, attachment.Scale,
                        spread - inward, referenceSpread, out float along, out float uneven))
                    continue;
                if (uneven < bestUneven)
                    (bestAlong, bestUneven, bestSpread) = (along, uneven, spread - inward);
                if (uneven <= Flush)
                    break;
            }
            if (bestUneven == float.MaxValue)
                return false;
            spread = bestSpread;
            rest.y = bestAlong + offset.y - attachment.Offset.y;
            return true;
        }

        // Une paire : deux moitiés au moins, enfants à maillage de son modèle.
        private static bool IsPair(Transform piece)
        {
            int halves = 0;
            foreach (Transform child in piece)
            {
                if (child.GetComponent<MeshFilter>() != null)
                    halves++;
            }
            return halves >= 2;
        }

        // Écarte chaque moitié d'une paire (ses enfants, chacun d'un côté) de spread mètres, de son côté.
        private static void SpreadHalves(Transform piece, float spread)
        {
            if (spread == 0f)
                return;
            foreach (Transform half in piece)
            {
                MeshFilter filter = half.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null)
                    continue;
                float side = Mathf.Sign(half.localPosition.x + filter.sharedMesh.bounds.center.x);
                half.localPosition += Vector3.right * (side * spread / piece.localScale.x);
            }
        }

        // Le point du repère plus le décalage ; sur l'axe de contact, la place où la pièce touche, plus le décalage sur
        // cet axe.
        private static Vector3 PlaceOf(Appearance appearance, AttachmentDefinition attachment, Vector3 offset, Vector3 scale,
            float spread)
        {
            AttachmentSlotDefinition slot = attachment.Slot;
            Vector3 place = appearance.Head.PointOf(slot.Frame) + offset;
            ContactAxis axis = slot.Contact;
            if (axis == ContactAxis.None)
                return place;
            bool onBody = slot.TouchesBody;
            SurfaceMap surface = onBody ? appearance.Body.SurfaceOf(axis) : appearance.Head.SurfaceOf(axis);
            if (!Contact.TryTouch(axis, surface, attachment.Shape, place, scale, out float along, spread))
            {
                Debug.LogWarning("DICIELA : la pièce « " + attachment.Id + " » ne touche pas « " +
                                 (onBody ? appearance.Body.Id : appearance.Head.Id) + " ».");
                return place;
            }
            return axis == ContactAxis.Down
                ? new Vector3(place.x, along + offset.y, place.z)
                : new Vector3(place.x, place.y, along + offset.z);
        }
    }
}
