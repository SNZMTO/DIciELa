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
    /// couleur teinte à sa façon : changer sa couleur change son poil. Deux copies réduites : le bas seul, sous la bouche
    /// (le menton, pour un gros museau), et la barbe sans sa moustache (pour une bouche large, que ses côtés encadrent). Enregistrées dans
    /// Assets/Art/Personnages/Pieces, le pack ne change pas.
    /// </summary>
    internal static class FurCopy
    {
        private const string Folder = "Assets/Art/Personnages/Pieces";
        // Savoir du pack : une case blanche de la texture des couleurs, dans le masque de la peau, que le matériau de
        // personnage teinte de la couleur du poil.
        private static readonly Vector2 Fur = new Vector2(0.027f, 0.187f);
        // Le menton : les triangles sous cette part de la hauteur de la barbe (sa moustache et ses favoris sont au-dessus).
        // La moustache : au-dessus de cette part de sa hauteur, à moins de cette part de sa largeur du milieu.
        private const float ChinShare = 0.4f;
        private const float MustacheFrom = 0.72f;
        private const float MustacheHalfWidth = 0.28f;

        // Les copies : le nom ajouté, et ce qu'elle garde (la position d'un triangle, en parts de la boîte de la barbe).
        private static readonly (string Suffix, System.Func<Vector2, bool> Keeps)[] Cuts =
        {
            ("_Menton", at => at.y < ChinShare),
            ("_SansMoustache", at => at.y < MustacheFrom || Mathf.Abs(at.x - 0.5f) > MustacheHalfWidth)
        };

        /// <summary>
        /// La copie de ce modèle, teinte comme le poil de cette couleur (celle du modèle), sous ce nom ; et ses copies
        /// réduites, sous ce nom suivi de _Menton et de _SansMoustache.
        /// </summary>
        public static GameObject Copy(GameObject model, string name, List<PaletteDefinition> palettes)
        {
            foreach ((string suffix, System.Func<Vector2, bool> keeps) in Cuts)
                Save(model, name + suffix, palettes, keeps);
            return Save(model, name, palettes, null);
        }

        // Enregistre la copie, réduite à ce qu'elle garde (null : entière) ; la rend.
        private static GameObject Save(GameObject model, string name, List<PaletteDefinition> palettes,
            System.Func<Vector2, bool> keeps)
        {
            (Mesh mesh, Material[] materials, Quaternion rotation) = PackCopy.Flatten(model, name);
            if (keeps != null)
                Keep(mesh, keeps);
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

        // Ne garde de ce maillage que les triangles dont le centre, en parts de sa boîte (x, y), est gardé.
        private static void Keep(Mesh mesh, System.Func<Vector2, bool> keeps)
        {
            Vector3[] vertices = mesh.vertices;
            Bounds box = mesh.bounds;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                int[] triangles = mesh.GetTriangles(sub);
                var kept = new List<int>();
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    Vector3 center = (vertices[triangles[t]] + vertices[triangles[t + 1]] + vertices[triangles[t + 2]]) / 3f;
                    var at = new Vector2((center.x - box.min.x) / box.size.x, (center.y - box.min.y) / box.size.y);
                    if (keeps(at))
                        kept.AddRange(new[] { triangles[t], triangles[t + 1], triangles[t + 2] });
                }
                mesh.SetTriangles(kept, sub);
            }
        }
    }
}
