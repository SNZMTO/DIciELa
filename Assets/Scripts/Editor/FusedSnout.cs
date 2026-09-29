using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le museau cousu à la face (le groin du cochon, les museaux du raton laveur, de la souris, du chat et du lapin, le bec
    /// de l'oiseau, la bouche peinte de la grenouille) : dans
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
        // Un îlot laissé par la découpe est à moins de cette demi-largeur du milieu (les défenses du cochon en sont plus
        // loin).
        private const float IslandHalfWidth = 0.12f;

        /// <summary>
        /// Les triangles du museau cousu à ce crâne (premiers indices), sous le haut de ces yeux, plus en avant que front (en
        /// mètres, face vers +z), avec les petits morceaux de la tête posés au milieu de la face (shells : tous ses morceaux ;
        /// les narines et le sourire de la grenouille) ; vide s'il n'y en a pas.
        /// </summary>
        public static HashSet<(int Sub, int First)> Find(Vector3[] positions, int[][] triangles,
            List<List<(int Sub, int First)>> shells, List<(int Sub, int First)> skull, List<Vector3> eyes, float front)
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
            if (core.Count >= MinTriangles)
            {
                snout.UnionWith(core);
                Grow(snout, flanks, positions, triangles);
            }
            Islands(snout, shells, skull, positions, triangles, eyesTop, front);
            return snout;
        }

        // Ajoute au museau les îlots du milieu de la face, sous le haut des yeux et devant la face : ce que sa découpe
        // laisse flotter (le fond des narines du cochon), les morceaux du crâne restants hors du plus grand, et les petits
        // morceaux de la tête posés sur la face (les narines et le sourire de la grenouille, la bouche du chat). Les yeux,
        // les sourcils et les défenses, plus loin du milieu, restent.
        private static void Islands(HashSet<(int Sub, int First)> snout, List<List<(int Sub, int First)>> pieces,
            List<(int Sub, int First)> skull, Vector3[] positions, int[][] triangles, float eyesTop, float front)
        {
            var rest = new List<(int Sub, int First)>();
            foreach ((int Sub, int First) triangle in skull)
            {
                if (!snout.Contains(triangle))
                    rest.Add(triangle);
            }
            List<List<(int Sub, int First)>> shells = MeshShells.Group(positions, triangles, rest);
            List<(int Sub, int First)> largest = shells[0];
            foreach (List<(int Sub, int First)> piece in pieces)
            {
                if (piece != skull)
                    shells.Add(piece);
            }
            foreach (List<(int Sub, int First)> shell in shells)
            {
                if (shell.Count > largest.Count)
                    largest = shell;
            }
            foreach (List<(int Sub, int First)> shell in shells)
            {
                if (shell == largest)
                    continue;
                Vector3 center = Vector3.zero;
                foreach ((int sub, int first) in shell)
                {
                    for (int k = 0; k < 3; k++)
                        center += positions[triangles[sub][first + k]];
                }
                center /= shell.Count * 3;
                if (center.y < eyesTop && Mathf.Abs(center.x) < IslandHalfWidth && center.z > front - FlankDepth)
                    snout.UnionWith(shell);
            }
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
