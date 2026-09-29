using Diciela.Data;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La pose d'une pièce sur un personnage habillé (SAVOIR, 2.11), dans les axes du personnage : son pivot au point du
    /// repère de son emplacement sur la tête choisie, plus son décalage et son réglage sur cette tête (ou ce corps) ;
    /// sur l'axe de contact, là où sa face touche la tête ou le corps. Elle prend sa rotation, puis sa taille. Chaque
    /// moitié d'une paire s'écarte (ou se rapproche) d'autant que les oreilles de la tête choisie sont plus écartées que
    /// celles de la tête où la paire a été réglée (plus le x de son décalage et de son réglage) ; une pièce d'un seul côté
    /// (une boucle d'oreille) suit l'oreille de son côté. Des oreilles, des boucles, un museau se posent par leur base :
    /// là où ils touchaient leur tête de référence, ils touchent la tête choisie, le reste s'enfonce ; une paire glisse
    /// vers le milieu jusqu'à ce que sa base porte à plat.
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
            float spread = SpreadOf(appearance.Head, attachment);
            bool pair = IsPair(piece);
            Vector3 offset = attachment.Offset + attachment.AdjustmentOn(appearance.Head, appearance.Body);
            // Le x d'une paire l'écarte, chaque moitié de son côté ; une pièce d'un seul côté (hors du milieu) suit
            // l'oreille de son côté.
            if (pair)
            {
                spread += offset.x;
                offset.x = 0f;
            }
            else if (attachment.Offset.x != 0f)
                offset.x += Mathf.Sign(attachment.Offset.x) * spread;
            // Dans un emplacement à paires, seule une paire se pose par sa base ; une boucle seule suit son point.
            bool seated = attachment.Slot.Seated && (pair || !attachment.Slot.Pairs);
            piece.localPosition = seated &&
                                  TryRest(appearance.Head, attachment, offset, ref spread, pair, out Vector3 rest)
                ? rest
                : PlaceOf(appearance, attachment, offset, pair ? spread : 0f);
            piece.localRotation = Quaternion.Euler(attachment.Rotation) * piece.localRotation;
            piece.localScale = attachment.Scale;
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

        // Une pièce posée par sa base sur une autre tête que sa tête de référence : là où elle touchait sa tête de
        // référence, elle touche la tête choisie (Contact.TryRest) ; une paire glisse vers le milieu jusqu'à porter à plat
        // (spread reçoit son écart). Faux sur sa tête de référence, ou sans surfaces.
        private static bool TryRest(HeadDefinition head, AttachmentDefinition attachment, Vector3 offset, ref float spread,
            bool pair, out Vector3 rest)
        {
            AttachmentSlotDefinition slot = attachment.Slot;
            HeadDefinition reference = attachment.Reference;
            rest = default;
            ContactAxis axis = slot.Contact;
            if (reference == null || reference == head || (axis != ContactAxis.Down && axis != ContactAxis.Back))
                return false;
            // Sa place sur sa tête de référence, où une paire n'est écartée que du x de son décalage.
            float referenceSpread = pair ? attachment.Offset.x : 0f;
            Vector3 onReference = reference.PointOf(slot.Frame) + attachment.Offset;
            if (pair)
                onReference.x = reference.PointOf(slot.Frame).x;
            if (!Contact.TryTouch(axis, reference.SurfaceOf(axis), attachment.Shape, onReference, attachment.Scale,
                    out float touch, referenceSpread))
                return false;
            onReference = WithAlong(axis, onReference, touch + Along(axis, attachment.Offset));
            rest = head.PointOf(slot.Frame) + offset;
            float bestAlong = 0f, bestUneven = float.MaxValue, bestSpread = spread;
            for (float inward = 0f; inward <= (pair ? MaxInward : 0f) + InwardStep * 0.5f; inward += InwardStep)
            {
                if (!Contact.TryRest(axis, head.SurfaceOf(axis), reference.SurfaceOf(axis), attachment.Shape, rest,
                        onReference, attachment.Scale, spread - inward, referenceSpread, out float along, out float uneven))
                    continue;
                if (uneven < bestUneven)
                    (bestAlong, bestUneven, bestSpread) = (along, uneven, spread - inward);
                if (uneven <= Flush)
                    break;
            }
            if (bestUneven == float.MaxValue)
                return false;
            spread = bestSpread;
            rest = WithAlong(axis, rest, bestAlong + Along(axis, offset) - Along(axis, attachment.Offset));
            return true;
        }

        // La coordonnée de ce point sur l'axe du contact (y pour descendre, z pour reculer), et ce point avec une autre.
        private static float Along(ContactAxis axis, Vector3 point)
        {
            return axis == ContactAxis.Down ? point.y : point.z;
        }

        private static Vector3 WithAlong(ContactAxis axis, Vector3 point, float along)
        {
            if (axis == ContactAxis.Down)
                point.y = along;
            else
                point.z = along;
            return point;
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
        private static Vector3 PlaceOf(Appearance appearance, AttachmentDefinition attachment, Vector3 offset,
            float spread)
        {
            AttachmentSlotDefinition slot = attachment.Slot;
            Vector3 place = appearance.Head.PointOf(slot.Frame) + offset;
            ContactAxis axis = slot.Contact;
            if (axis == ContactAxis.None)
                return place;
            bool onBody = slot.TouchesBody;
            SurfaceMap surface = onBody ? appearance.Body.SurfaceOf(axis) : appearance.Head.SurfaceOf(axis);
            if (!Contact.TryTouch(axis, surface, attachment.Shape, place, attachment.Scale, out float along, spread))
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
