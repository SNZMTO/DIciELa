using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Une pièce du pack teinte comme le poil (SAVOIR, 2.11) : la barbe. Le pack la peint d'une teinte d'objet que ses
    /// douze couleurs laissent presque pareille (le gris du masque des cheveux, teinté de deux bruns à peine différents).
    /// La copie prend le matériau de personnage de sa couleur, et tous ses sommets vont sur la case du poil, que chaque
    /// couleur teinte à sa façon : changer sa couleur change son poil. Enregistrée dans Assets/Art/Personnages/Pieces, le
    /// pack ne change pas.
    /// </summary>
    internal static class FurCopy
    {
        private const string Folder = "Assets/Art/Personnages/Pieces";
        // Savoir du pack : une case blanche de la texture des couleurs, dans le masque de la peau, que le matériau de
        // personnage teinte de la couleur du poil.
        private static readonly Vector2 Fur = new Vector2(0.027f, 0.187f);

        /// <summary>La copie de ce modèle, teinte comme le poil de cette couleur (celle du modèle), sous ce nom.</summary>
        public static GameObject Copy(GameObject model, string name, List<PaletteDefinition> palettes)
        {
            (Mesh mesh, Material[] materials, Quaternion rotation) = PackCopy.Flatten(model, name);
            PaletteDefinition palette = PaletteBuilder.PaletteOf(model.GetComponentInChildren<Renderer>(true), palettes);
            var uvs = new List<Vector2>();
            for (int i = 0; i < mesh.vertexCount; i++)
                uvs.Add(Fur);
            mesh.SetUVs(0, uvs);
            for (int i = 0; i < materials.Length; i++)
                materials[i] = palette.CharacterMaterial;
            EditorAssets.EnsureFolder(Folder);
            Mesh saved = BodyCutter.Save(mesh, Folder + "/" + name + ".asset");
            var copy = new GameObject(name);
            try
            {
                copy.transform.localRotation = rotation;
                copy.AddComponent<MeshFilter>().sharedMesh = saved;
                copy.AddComponent<MeshRenderer>().sharedMaterials = materials;
                return PrefabUtility.SaveAsPrefabAsset(copy, Folder + "/" + name + ".prefab");
            }
            finally
            {
                Object.DestroyImmediate(copy);
                if (saved != mesh)
                    Object.DestroyImmediate(mesh);
            }
        }
    }
}
