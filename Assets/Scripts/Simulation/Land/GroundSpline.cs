using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Une courbe douce qui passe par des points, dans l'ordre (décision 216) : la spline de Catmull-Rom centripète, qui ne
    /// fait ni boucle ni pointe entre deux points proches ; ouverte (un tracé), ou fermée (un polygone arrondi). On la garde
    /// en petits segments d'au plus une longueur donnée : c'est ainsi qu'un trait ou un polygone la porte.
    /// </summary>
    public static class GroundSpline
    {
        // Le plus petit écart de nœuds : deux points confondus ne divisent pas par zéro.
        private const float MinKnot = 1e-3f;

        /// <summary>
        /// Met dans result la courbe qui passe par ces points, en segments d'au plus step mètres ; fermée, elle revient au
        /// premier point sans le répéter. Moins de trois points (deux pour une courbe fermée) : les points eux-mêmes.
        /// </summary>
        public static void Sample(IReadOnlyList<Vector2> points, bool closed, float step, List<Vector2> result)
        {
            result.Clear();
            int count = points.Count;
            if (count < 3)
            {
                for (int i = 0; i < count; i++)
                    result.Add(points[i]);
                return;
            }
            int segments = closed ? count : count - 1;
            for (int s = 0; s < segments; s++)
            {
                Vector2 p1 = points[s];
                Vector2 p2 = points[(s + 1) % count];
                int pieces = Mathf.Max(1, Mathf.CeilToInt((p2 - p1).magnitude / step));
                Vector2 p0 = Neighbor(points, s - 1, closed);
                Vector2 p3 = Neighbor(points, s + 2, closed);
                for (int i = 0; i < pieces; i++)
                    result.Add(At(p0, p1, p2, p3, (float)i / pieces));
            }
            if (!closed)
                result.Add(points[count - 1]);
        }

        // Le point d'indice i ; fermée, la courbe boucle ; ouverte, un bout se prolonge par le reflet de son voisin.
        private static Vector2 Neighbor(IReadOnlyList<Vector2> points, int i, bool closed)
        {
            int count = points.Count;
            if (closed)
                return points[(i % count + count) % count];
            if (i < 0)
                return 2f * points[0] - points[1];
            if (i >= count)
                return 2f * points[count - 1] - points[count - 2];
            return points[i];
        }

        // Le point de la courbe entre p1 (u = 0) et p2 (u = 1), par la pyramide de Barry et Goldman, aux nœuds
        // centripètes.
        private static Vector2 At(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float u)
        {
            const float t0 = 0f;
            float t1 = t0 + Knot(p0, p1);
            float t2 = t1 + Knot(p1, p2);
            float t3 = t2 + Knot(p2, p3);
            float t = Mathf.Lerp(t1, t2, u);
            Vector2 a1 = Mix(p0, p1, t0, t1, t);
            Vector2 a2 = Mix(p1, p2, t1, t2, t);
            Vector2 a3 = Mix(p2, p3, t2, t3, t);
            Vector2 b1 = Mix(a1, a2, t0, t2, t);
            Vector2 b2 = Mix(a2, a3, t1, t3, t);
            return Mix(b1, b2, t1, t2, t);
        }

        // L'écart de nœuds centripète : la racine de la distance.
        private static float Knot(Vector2 a, Vector2 b)
        {
            return Mathf.Max(Mathf.Sqrt((b - a).magnitude), MinKnot);
        }

        private static Vector2 Mix(Vector2 a, Vector2 b, float ta, float tb, float t)
        {
            return ((tb - t) * a + (t - ta) * b) / (tb - ta);
        }
    }
}
