using System.Collections.Generic;
using Diciela.Data;
using Diciela.View;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le réglage de départ d'une pièce (SAVOIR, 2.11) : son décalage, sa taille et ce qu'elle cache, écrits une fois, quand
    /// sa taille vaut encore zéro ; ensuite ils sont au développeur. Dans l'ordre : la pose que le pack lui donne sur un
    /// animal (la huppe sur l'oiseau, les queues), reproduite ; sinon celle qu'il lui donne sur un humain, ramenée de la
    /// tête de ce porteur à celle de référence (le chat) et agrandie du rapport de leurs crânes ; sinon la règle de son
    /// emplacement (SlotRule), mesurée sur la tête de référence. Sur l'axe de contact, la pièce touche, un peu enfoncée
    /// selon la règle.
    /// </summary>
    internal static class AttachmentFitter
    {
        // Le haut de ce qui se porte au cou passe sous la tête de ce nombre de mètres, pour que la tête le couvre.
        private const float NeckTuck = 0.06f;
        // Un chapeau low poly a un fond fermé que la tête traverse : il descend sous le contact de cette part de sa hauteur
        // (sur les chapeaux humains du pack, de 0,2 à 0,8, 0,4 au milieu).
        private const float ClosedHatSinkPerHeight = 0.35f;

        /// <summary>Reproduit une pose : son pivot à cette place, sur ce personnage (cette tête, ce corps), taille 1.</summary>
        public static void FitToPose(AttachmentDefinition piece, Vector3 pivot, HeadDefinition head, BodyDefinition body)
        {
            Reference(piece, head);
            if (piece.HasScale)
                return;
            AttachmentSlotDefinition slot = piece.Slot;
            if (slot == null)
                throw new System.InvalidOperationException("DICIELA : la pièce « " + piece.name + " » n'a pas d'emplacement.");
            Vector3 offset = pivot - head.PointOf(slot.Frame);
            SurfaceMap surface = slot.TouchesBody ? body.SurfaceOf(slot.Contact) : head.SurfaceOf(slot.Contact);
            if (Contact.TryTouch(slot.Contact, surface, piece.Shape, pivot, Vector3.one, out float along))
                offset = WithAlong(slot.Contact, offset, Along(slot.Contact, pivot) - along);
            Write(piece, offset, Vector3.one, null);
        }

        /// <summary>
        /// Ramène une pièce humaine, posée par le pack à l'os Head de son porteur, sur la tête de référence : même place
        /// par rapport au point de son repère, agrandie du rapport des crânes ; sur l'axe de contact, le même enfoncement
        /// que sur son porteur, agrandi aussi (la tête traverse le fond fermé d'un chapeau low poly). Une pièce qui
        /// enveloppe le crâne (wraps : cheveux, barbe) prend le rapport des crânes axe par axe, pour que son creux épouse une
        /// tête d'animal, large et plate, et se cale sur la tête de référence (referenceHead) : des cheveux, leur frange (le
        /// plus bas de leur moitié avant, geometry) aux sourcils ; une barbe, au point de son repère ; le contact n'y suit
        /// ensuite que les écarts d'une tête à l'autre. Les autres prennent le rapport des largeurs. hides : ce qu'elle
        /// cache (ou null).
        /// </summary>
        public static void FitHuman(AttachmentDefinition piece, SlotRule rule, bool wraps, HumanHead wearer,
            HeadMeasures reference, HeadDefinition referenceHead, PackMeasures.Geometry geometry,
            AttachmentSlotDefinition[] hides)
        {
            Reference(piece, referenceHead);
            if (piece.HasScale)
                return;
            AttachmentSlotDefinition slot = piece.Slot;
            HeadMeasures human = wearer.Measures;
            float width = reference.BrowWidth / human.BrowWidth;
            Vector3 scale = wraps
                ? new Vector3(width, (reference.Crown.y - reference.Face.y) / (human.Crown.y - human.Face.y),
                    reference.BrowDepth / human.BrowDepth)
                : Vector3.one * width;
            Vector3 design = wearer.HeadBone;
            Vector3 offset = Vector3.Scale(design - PointOf(human, slot.Frame), scale);
            float along = -rule.Sink;
            Vector3 mapped = referenceHead.PointOf(slot.Frame) + offset;
            if (wraps && slot.Contact == ContactAxis.Down)
                mapped.y = reference.BrowHeight - PieceShape.Fringe(geometry) * scale.y;
            if (wraps && Contact.TryTouch(slot.Contact, referenceHead.SurfaceOf(slot.Contact), piece.Shape, mapped, scale,
                    out float onReference))
                along = Along(slot.Contact, mapped) - onReference;
            else if (!wraps && Contact.TryTouch(slot.Contact, wearer.SurfaceOf(slot.Contact), piece.Shape, design,
                         Vector3.one, out float touch))
                along = (Along(slot.Contact, design) - touch) * Along(slot.Contact, scale);
            Write(piece, WithAlong(slot.Contact, offset, along), scale, hides);
        }

        /// <summary>
        /// Une pièce que le pack ne pose nulle part, posée par la règle de son emplacement sur la tête de référence ;
        /// agrandie du rapport des crânes si elle est dessinée pour une tête humaine (humanReference : la tête humaine de
        /// référence) ; hides : ce qu'elle cache (ou null).
        /// </summary>
        public static void FitByRule(AttachmentDefinition piece, SlotRule rule, bool human, PackMeasures.Geometry geometry,
            HeadMeasures reference, HeadDefinition referenceHead, HeadMeasures humanReference,
            Dictionary<string, Vector3> bones, Vector3 tailRoot, AttachmentSlotDefinition[] hides)
        {
            Reference(piece, referenceHead);
            if (piece.HasScale)
                return;
            AttachmentSlotDefinition slot = piece.Slot;
            float scale = human ? reference.BrowWidth / humanReference.BrowWidth : 1f;
            Bounds box = geometry.Box;
            Vector3 center = box.center * scale;
            Vector3 offset;
            switch (rule.Where)
            {
                case SlotRule.Place.TailRoot:
                    offset = tailRoot;
                    break;
                case SlotRule.Place.Back:
                    offset = new Vector3(0f, bones[slot.Bone].y - center.y, 0f);
                    break;
                case SlotRule.Place.Crown:
                    offset = new Vector3(-center.x, 0f, -center.z);
                    break;
                case SlotRule.Place.Eyes:
                    offset = Vector3.zero;
                    break;
                case SlotRule.Place.Nose:
                    offset = new Vector3(0f, reference.Nose.y - reference.Face.y, 0f);
                    break;
                case SlotRule.Place.Ears:
                    offset = new Vector3(PieceShape.IsSingle(geometry) ? reference.EarRight.x : 0f, 0f, 0f);
                    break;
                case SlotRule.Place.UnderHead:
                    offset = new Vector3(-center.x, -box.max.y * scale + NeckTuck, -center.z);
                    break;
                default:
                    offset = bones[slot.Bone];
                    break;
            }
            float along = rule.Where == SlotRule.Place.Crown && PieceShape.ClosedBottom(piece.Shape, box)
                ? -ClosedHatSinkPerHeight * box.size.y * scale
                : -rule.Sink;
            Write(piece, WithAlong(slot.Contact, offset, along), Vector3.one * scale, hides);
        }

        private static Vector3 PointOf(HeadMeasures head, AttachmentFrame frame)
        {
            switch (frame)
            {
                case AttachmentFrame.Crown:
                    return head.Crown;
                case AttachmentFrame.Face:
                    return head.Face;
                case AttachmentFrame.Ears:
                    return head.Ears;
                case AttachmentFrame.Neck:
                    return head.Neck;
                default:
                    return Vector3.zero;
            }
        }

        private static float Along(ContactAxis axis, Vector3 point)
        {
            return axis == ContactAxis.Down ? point.y : point.z;
        }

        private static Vector3 WithAlong(ContactAxis axis, Vector3 offset, float along)
        {
            if (axis == ContactAxis.None)
                return offset;
            return axis == ContactAxis.Down
                ? new Vector3(offset.x, along, offset.z)
                : new Vector3(offset.x, offset.y, along);
        }

        // La tête sur laquelle la pièce se règle : déduite du pack, réécrite à chaque passage.
        private static void Reference(AttachmentDefinition piece, HeadDefinition head)
        {
            EditorAssets.Set(piece, fiche => EditorAssets.Property(fiche, "_reference").objectReferenceValue = head);
        }

        private static void Write(AttachmentDefinition piece, Vector3 offset, Vector3 scale, AttachmentSlotDefinition[] hides)
        {
            EditorAssets.Set(piece, fiche =>
            {
                EditorAssets.Property(fiche, "_offset").vector3Value = offset;
                EditorAssets.Property(fiche, "_scale").vector3Value = scale;
                if (hides != null)
                    EditorAssets.SetList(EditorAssets.Property(fiche, "_hides"), hides);
            });
        }
    }
}
