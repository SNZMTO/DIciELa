using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les retouches de peinture des pièces découpées des têtes (SAVOIR, 2.11) : dans la copie découpée, jamais dans le
    /// pack, les triangles d'une teinte prennent une autre teinte de la palette. Savoir du pack : l'intérieur de l'oreille
    /// gauche du raton laveur est peint du blanc de sa face, celui de la droite du gris de l'oreille ; posée sur une autre
    /// tête, cette lèvre blanche fait un bandeau en travers du crâne. On la peint comme la droite.
    /// </summary>
    internal static class PartRepaint
    {
        // Deux teintes sont la même à cet écart près de leurs coordonnées de texture (une case de la palette).
        private const float SamePaint = 0.004f;

        // La pièce (le nom de son fichier), la teinte à changer, la teinte qui la remplace.
        private static readonly (string Part, Vector2 From, Vector2 To)[] Repaints =
        {
            ("Oreilles_RatonLaveur", new Vector2(0.028f, 0.163f), new Vector2(0.052f, 0.191f))
        };

        /// <summary>Repeint, en place, les triangles de ce maillage selon les retouches de cette pièce.</summary>
        public static void Apply(string part, Mesh mesh)
        {
            var uvs = new List<Vector2>();
            mesh.GetUVs(0, uvs);
            bool changed = false;
            foreach ((string name, Vector2 from, Vector2 to) in Repaints)
            {
                if (name != part || uvs.Count == 0)
                    continue;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    int[] triangles = mesh.GetTriangles(sub);
                    for (int t = 0; t < triangles.Length; t += 3)
                    {
                        Vector2 paint = (uvs[triangles[t]] + uvs[triangles[t + 1]] + uvs[triangles[t + 2]]) / 3f;
                        if ((paint - from).magnitude > SamePaint)
                            continue;
                        for (int k = 0; k < 3; k++)
                            uvs[triangles[t + k]] = to;
                        changed = true;
                    }
                }
            }
            if (changed)
                mesh.SetUVs(0, uvs);
        }
    }
}
