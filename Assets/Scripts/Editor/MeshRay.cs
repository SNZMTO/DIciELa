using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Des rayons lancés sur une surface de triangles (en mètres, dans le repère du personnage) : la distance du premier
    /// triangle touché, des deux côtés confondus. Sert à mesurer une tête du pack par sa surface, pas par ses sommets,
    /// trop clairsemés en low poly pour donner un sommet de crâne ou un bout de nez.
    /// </summary>
    internal sealed class MeshRay
    {
        private const float Parallel = 1e-9f;
        private readonly Vector3[] _positions;
        private readonly List<int> _triangles;

        public MeshRay(Vector3[] positions, List<int> triangles)
        {
            _positions = positions;
            _triangles = triangles;
        }

        /// <summary>La distance du premier triangle touché le long de ce rayon ; faux si aucun.</summary>
        public bool TryHit(Vector3 origin, Vector3 direction, out float distance)
        {
            distance = float.MaxValue;
            for (int t = 0; t < _triangles.Count; t += 3)
            {
                if (TryTriangle(origin, direction, _positions[_triangles[t]], _positions[_triangles[t + 1]],
                        _positions[_triangles[t + 2]], out float d) && d < distance)
                    distance = d;
            }
            return distance < float.MaxValue;
        }

        // Möller et Trumbore : le point du rayon dans le plan du triangle, puis s'il tombe dedans.
        private static bool TryTriangle(Vector3 origin, Vector3 direction, Vector3 a, Vector3 b, Vector3 c,
            out float distance)
        {
            distance = 0f;
            Vector3 ab = b - a, ac = c - a;
            Vector3 p = Vector3.Cross(direction, ac);
            float det = Vector3.Dot(ab, p);
            if (Mathf.Abs(det) < Parallel)
                return false;
            float inverse = 1f / det;
            Vector3 s = origin - a;
            float u = Vector3.Dot(s, p) * inverse;
            if (u < 0f || u > 1f)
                return false;
            Vector3 q = Vector3.Cross(s, ab);
            float v = Vector3.Dot(direction, q) * inverse;
            if (v < 0f || u + v > 1f)
                return false;
            distance = Vector3.Dot(ac, q) * inverse;
            return distance >= 0f;
        }
    }
}
