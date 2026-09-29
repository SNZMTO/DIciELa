using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Habille un personnage d'une apparence (GDD, section 6 ; SAVOIR, 2.11), la seule façon de le faire, pour les
    /// habitants vus comme pour l'aperçu de l'écran de création. Le personnage du pack d'où vient le corps sert de
    /// squelette : tous les animaux ont le même. On lui retire ce qu'il porte (ses seize corps, ses pièces d'office, ses
    /// capes cachées), puis il reçoit le corps et le crâne choisis, chacun de sa couleur, sur ses os. Une petite cape
    /// qu'il porte dans le pack reste, comme un habit, de la couleur du corps, sauf sous une pièce qui couvre le dos (une
    /// cape). Chaque pièce se pose (PiecePlacement), puis s'accroche à l'os de son emplacement sans bouger ; une pièce
    /// qu'une autre cache (des oreilles sous un chapeau) ne se pose pas.
    /// </summary>
    public static class Dresser
    {
        /// <summary>
        /// Le personnage habillé, enfant de ce parent, à son origine ; null si une fiche n'a pas son modèle (dit dans la
        /// console).
        /// </summary>
        public static GameObject Dress(Appearance appearance, Transform parent)
        {
            BodyDefinition body = appearance.Body;
            HeadDefinition head = appearance.Head;
            if (body.Model == null || body.Mesh == null || head.Mesh == null)
            {
                Debug.LogError("DICIELA : le corps « " + body.Id + " » ou la tête « " + head.Id +
                               " » n'a pas son modèle.");
                return null;
            }

            GameObject character = Object.Instantiate(body.Model, parent, false);
            character.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            bool coversBack = false;
            foreach (WornAttachment worn in appearance.Attachments)
                coversBack |= worn.Attachment.CoversBack;
            if (!TryStrip(character.transform, coversBack, out Skeleton skeleton))
            {
                Debug.LogError("DICIELA : le personnage de « " + body.Id + " » n'a pas de corps allumé.");
                return character;
            }

            PaletteSwap.Apply(character.GetComponentsInChildren<Renderer>(), body.Palette, appearance.BodyPalette);
            Part(character.transform, "Corps", body.Mesh, appearance.BodyPalette, skeleton);
            Part(character.transform, "Tête", head.Mesh, appearance.HeadPalette, skeleton);
            var hidden = new HashSet<AttachmentSlotDefinition>();
            foreach (WornAttachment worn in appearance.Attachments)
            {
                foreach (AttachmentSlotDefinition slot in worn.Attachment.Hides)
                    hidden.Add(slot);
            }
            foreach (WornAttachment worn in appearance.Attachments)
            {
                if (!hidden.Contains(worn.Attachment.Slot) && !head.Refuses(worn.Attachment.Slot))
                    Attach(character.transform, appearance, worn);
            }
            return character;
        }

        /// <summary>Les os du corps allumé, relevés avant qu'on le retire : la tête et le corps choisis s'y posent.</summary>
        private readonly struct Skeleton
        {
            public Skeleton(SkinnedMeshRenderer shown)
            {
                Bones = shown.bones;
                Root = shown.rootBone;
                Bounds = shown.localBounds;
            }

            public Transform[] Bones { get; }
            public Transform Root { get; }
            public Bounds Bounds { get; }
        }

        /// <summary>
        /// Retire ce que porte le personnage du pack : ses corps (les maillages à peau enfants directs), ses pièces
        /// d'office (ses maillages sans peau), ses capes cachées, et sa petite cape si une pièce couvre le dos. Faux s'il n'a
        /// pas de corps allumé, dont on garde les os.
        /// </summary>
        private static bool TryStrip(Transform character, bool coversBack, out Skeleton skeleton)
        {
            skeleton = default;
            bool found = false;
            foreach (SkinnedMeshRenderer skin in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                bool body = skin.transform.parent == character;
                if (body && skin.gameObject.activeSelf && !found)
                {
                    skeleton = new Skeleton(skin);
                    found = true;
                }
                if (body || !skin.gameObject.activeSelf || coversBack)
                    Remove(skin.gameObject);
            }
            foreach (MeshRenderer piece in character.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter filter = piece.GetComponent<MeshFilter>();
                Remove(piece);
                if (filter != null)
                    Remove(filter);
            }
            return found;
        }

        /// <summary>Un maillage à peau sur les os du personnage, de cette couleur.</summary>
        private static void Part(Transform character, string name, Mesh mesh, PaletteDefinition palette,
            Skeleton skeleton)
        {
            var part = new GameObject(name);
            part.transform.SetParent(character, false);
            var skin = part.AddComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = mesh;
            skin.bones = skeleton.Bones;
            skin.rootBone = skeleton.Root;
            // Sans la boîte du corps d'origine, autour de son os racine, le rendu la croirait hors de vue.
            skin.localBounds = skeleton.Bounds;
            skin.sharedMaterial = palette.CharacterMaterial;
        }

        private static void Attach(Transform character, Appearance appearance, WornAttachment worn)
        {
            AttachmentDefinition attachment = worn.Attachment;
            if (attachment.Model == null || !attachment.HasScale)
            {
                Debug.LogError("DICIELA : la pièce « " + attachment.Id + " » n'a pas de modèle, ou pas de taille.");
                return;
            }

            // Posée dans les axes du personnage, telle que le modèle est fait, puis confiée à son os sans bouger.
            GameObject piece = Object.Instantiate(attachment.Model, character, false);
            PiecePlacement.Place(piece.transform, appearance, attachment);
            PaletteSwap.Apply(piece.GetComponentsInChildren<Renderer>(), attachment.Palette, worn.Palette);
            Transform bone = Find(character, attachment.Slot.Bone);
            if (bone != null)
                piece.transform.SetParent(bone, true);
            else
                Debug.LogError("DICIELA : le personnage n'a pas d'os « " + attachment.Slot.Bone + " » pour la pièce « " +
                               attachment.Id + " ».");
        }

        private static Transform Find(Transform root, string name)
        {
            var pending = new Stack<Transform>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                Transform current = pending.Pop();
                if (current.name == name)
                    return current;
                foreach (Transform child in current)
                    pending.Push(child);
            }
            return null;
        }

        private static void Remove(Object thing)
        {
            if (Application.isPlaying)
                Object.Destroy(thing);
            else
                Object.DestroyImmediate(thing);
        }
    }
}
