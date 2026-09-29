using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le museau cousu à la face (le groin du cochon, le museau du raton laveur et de la souris, le bec de l'oiseau) : dans
    /// le crâne, ce qui dépasse devant la face, sous le haut des yeux, près du milieu. Le noyau est ce qui est tout entier
    /// plus en avant que la profondeur donnée par le pack (savoir du pack, mesuré tête par tête : AppearanceBuilder) ; on y
    /// ajoute, de proche en proche, ses flancs (les triangles voisins tournés sur le côté, en haut ou en bas, pas vers
    /// l'avant comme la face), jusqu'à la face. Le trou laissé dans la face, SkullCap le referme ; le dessous ouvert du
    /// museau aussi.
    /// </summary>
    internal static class FusedSnout
    {
        // Un museau est près du milieu, à moins de cette demi-largeur ; ses flancs descendent jusqu'à tant de mètres
        // derrière la profondeur du noyau ; un flanc ne regarde pas vers l'avant (la composante avant de son orientation
        // reste sous le seuil). Le noyau compte au moins tant de triangles.
        private const float HalfWidth = 0.25f;
        private const float FlankDepth = 0.08f;
        private const float FlankFacing = 0.6f;
        private const int MinTriangles = 12;

        /// <summary>
        /// Les triangles du museau cousu à ce crâne (premiers indices), sous le haut de ces yeux, plus en avant que front (en
        /// mètres, face vers +z) ; vide s'il n'y en a pas.
        /// </summary>
        public static HashSet<(int Sub, int First)> Find(Vector3[] positions, int[][] triangles,
            List<(int Sub, int First)> skull, List<Vector3> eyes, float front)
        {
            float eyesTop = float.MinValue;
            foreach (Vector3 eye in eyes)
                eyesTop = Mathf.Max(eyesTop, eye.y);
            var core = new List<(int Sub, int First)>();
            var flanks = new List<(int Sub, int First)>();
            foreach ((int sub, int first) in skull)
            {
                Vector3 a = positions[triangles[sub][first]], b = positions[triangles[sub][first + 1]];
                Vector3 c = positions[triangles[sub][first + 2]];
                Vector3 center = (a + b + c) / 3f;
                if (center.y > eyesTop || Mathf.Abs(center.x) > HalfWidth)
                    continue;
                if (Mathf.Min(a.z, Mathf.Min(b.z, c.z)) > front)
                    core.Add((sub, first));
                else if (center.z > front - FlankDepth && Vector3.Cross(b - a, c - a).normalized.z < FlankFacing)
                    flanks.Add((sub, first));
            }
            // Tout le noyau, en un ou plusieurs morceaux (le bec de l'oiseau a deux mâchoires).
            var snout = new HashSet<(int Sub, int First)>();
            if (core.Count < MinTriangles)
                return snout;
            snout.UnionWith(core);
            Grow(snout, flanks, positions, triangles);
            return snout;
        }

        // Ajoute au museau, de proche en proche, les flancs qui le touchent par un sommet. Borné : chaque tour en ajoute au
        // moins un, ou s'arrête.
        private static void Grow(HashSet<(int Sub, int First)> snout, List<(int Sub, int First)> flanks,
            Vector3[] positions, int[][] triangles)
        {
            int[] weld = MeshShells.Weld(positions);
            var touched = new HashSet<int>();
            foreach ((int sub, int first) in snout)
            {
                for (int k = 0; k < 3; k++)
                    touched.Add(weld[triangles[sub][first + k]]);
            }
            bool grew = true;
            while (grew)
            {
                grew = false;
                for (int i = flanks.Count - 1; i >= 0; i--)
                {
                    (int sub, int first) = flanks[i];
                    int[] corners = { weld[triangles[sub][first]], weld[triangles[sub][first + 1]],
                        weld[triangles[sub][first + 2]] };
                    if (!touched.Contains(corners[0]) && !touched.Contains(corners[1]) && !touched.Contains(corners[2]))
                        continue;
                    snout.Add(flanks[i]);
                    touched.UnionWith(corners);
                    flanks.RemoveAt(i);
                    grew = true;
                }
            }
        }
    }
}
