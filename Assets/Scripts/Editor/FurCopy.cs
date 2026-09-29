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
    /// couleur teinte à sa façon : changer sa couleur change son poil. Une seconde copie ne garde que le bas, sous la
    /// bouche (le menton) : sous un bec ou un gros museau, la moustache rend mal. Enregistrées dans
    /// Assets/Art/Personnages/Pieces, le pack ne change pas.
    /// </summary>
    internal static class FurCopy
    {
        private const string Folder = "Assets/Art/Personnages/Pieces";
        // Savoir du pack : une case blanche de la texture des couleurs, dans le masque de la peau, que le matériau de
        // personnage teinte de la couleur du poil.
        private static readonly Vector2 Fur = new Vector2(0.027f, 0.187f);
        // Le menton : les triangles sous cette part de la hauteur de la barbe (sa moustache et ses favoris sont au-dessus).
        private const float ChinShare = 0.5f;

        /// <summary>
        /// La copie de ce modèle, teinte comme le poil de cette couleur (celle du modèle), sous ce nom ; et, sous ce nom suivi
        /// de _Menton, celle de son bas.
        /// </summary>
        public static GameObject Copy(GameObject model, string name, List<PaletteDefinition> palettes)
        {
            Save(model, name, palettes, false);
            return Save(model, name, palettes, true);
        }

        // Enregistre la copie entière, ou son menton seul (sous le nom suivi de _Menton) ; rend la copie entière.
        private static GameObject Save(GameObject model, string name, List<PaletteDefinition> palettes, bool whole)
        {
            if (!whole)
                name += "_Menton";
            (Mesh mesh, Material[] materials, Quaternion rotation) = PackCopy.Flatten(model, name);
            if (!whole)
                KeepChin(mesh);
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

        // Ne garde de ce maillage que les triangles du bas de sa hauteur.
        private static void KeepChin(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Bounds box = mesh.bounds;
            float cut = box.min.y + ChinShare * box.size.y;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                int[] triangles = mesh.GetTriangles(sub);
                var kept = new List<int>();
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    float y = (vertices[triangles[t]].y + vertices[triangles[t + 1]].y + vertices[triangles[t + 2]].y) / 3f;
                    if (y < cut)
                        kept.AddRange(new[] { triangles[t], triangles[t + 1], triangles[t + 2] });
                }
                mesh.SetTriangles(kept, sub);
            }
        }
    }
}
