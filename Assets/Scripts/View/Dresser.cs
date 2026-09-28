using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Habille un personnage d'une apparence (GDD, section 6 ; SAVOIR, 2.11), la seule façon de le faire, pour les
    /// habitants vus comme pour l'aperçu de l'écran de création. Le corps du pack, tel quel, sans ses corps cachés (ils
    /// n'ont pas de matériau dans le projet, et pèsent pour rien). Sa couleur à la place de celle d'origine, sur le
    /// matériau partagé : une copie par personnage empêcherait le rendu de les regrouper. Chaque pièce posée droite, son
    /// pivot sur son point d'accroche, puis accrochée à l'os de son emplacement sans bouger : elle le suivra quand le
    /// personnage sera animé.
    /// </summary>
    public static class Dresser
    {
        /// <summary>
        /// Le personnage habillé, enfant de ce parent, à son origine ; null si son corps n'a pas de personnage (dit dans
        /// la console).
        /// </summary>
        public static GameObject Dress(Appearance appearance, Transform parent)
        {
            GameObject model = appearance.Body.Model;
            if (model == null)
            {
                Debug.LogError("DICIELA : le corps « " + appearance.Body.Id + " » n'a pas de personnage.");
                return null;
            }

            GameObject character = Object.Instantiate(model, parent, false);
            character.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Paint(character, appearance);
            foreach (AttachmentDefinition attachment in appearance.Attachments)
                Attach(character.transform, attachment);
            return character;
        }

        /// <summary>Retire les corps cachés, et pose la couleur choisie à la place de celle d'origine.</summary>
        private static void Paint(GameObject character, Appearance appearance)
        {
            Material from = appearance.Body.Palette != null ? appearance.Body.Palette.Material : null;
            Material to = appearance.Palette.Material;
            foreach (SkinnedMeshRenderer skin in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!skin.gameObject.activeSelf)
                {
                    Remove(skin.gameObject);
                    continue;
                }
                if (from == null || to == null || from == to)
                    continue;
                Material[] materials = skin.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != from)
                        continue;
                    materials[i] = to;
                    changed = true;
                }
                if (changed)
                    skin.sharedMaterials = materials;
            }
        }

        private static void Attach(Transform character, AttachmentDefinition attachment)
        {
            if (attachment.Model == null)
            {
                Debug.LogError("DICIELA : la pièce « " + attachment.Id + " » n'a pas de modèle.");
                return;
            }

            // Posée dans les axes du personnage, telle que le pack la modèle, puis confiée à son os sans bouger.
            GameObject piece = Object.Instantiate(attachment.Model, character, false);
            piece.transform.localPosition = attachment.Anchor;
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

        private static void Remove(GameObject hidden)
        {
            if (Application.isPlaying)
                Object.Destroy(hidden);
            else
                Object.DestroyImmediate(hidden);
        }
    }
}
