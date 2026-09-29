using Diciela.Data;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La pose d'une pièce sur un personnage habillé (SAVOIR, 2.11), dans les axes du personnage : son pivot au point du
    /// repère de son emplacement sur la tête choisie, plus son décalage et son réglage sur cette tête (ou ce corps) ;
    /// sur l'axe de contact, là où sa face touche la tête ou le corps. Elle prend sa rotation, puis sa taille. Des oreilles suivent les
    /// oreilles de la tête choisie : chaque moitié d'une paire s'écarte (ou se rapproche) d'autant que ses oreilles sont
    /// plus écartées que celles de la tête où la paire a été réglée (plus le x de son décalage et de son réglage sur cette
    /// tête), puis la paire se pose par sa base : là où elle touchait sa tête de référence, elle touche la tête choisie ;
    /// une pièce d'un seul côté (une boucle d'oreille) suit l'oreille de son côté.
    /// </summary>
    internal static class PiecePlacement
    {
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
            piece.localPosition = pair && TryRest(appearance.Head, attachment, offset, spread, out Vector3 rest)
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

        // Une paire posée par sa base sur une autre tête que sa tête de référence : là où la paire touchait sa tête de
        // référence, elle touche la tête choisie (Contact.TryRest). Faux sur sa tête de référence, ou sans surfaces.
        private static bool TryRest(HeadDefinition head, AttachmentDefinition attachment, Vector3 offset, float spread,
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
            if (!Contact.TryTouch(ContactAxis.Down, reference.SurfaceOf(ContactAxis.Down), attachment.Shape, onReference,
                    attachment.Scale, out float touch, referenceSpread))
                return false;
            onReference.y = touch + attachment.Offset.y;
            rest = head.PointOf(slot.Frame) + offset;
            if (!Contact.TryRest(head.SurfaceOf(ContactAxis.Down), reference.SurfaceOf(ContactAxis.Down), attachment.Shape,
                    rest, onReference, attachment.Scale, spread, referenceSpread, out float along))
                return false;
            rest.y = along + (offset.y - attachment.Offset.y);
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
