using System.Collections.Generic;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Un maillage posé sur le relief (décisions 206 et 227) : des faces dont les sommets se partagent par leur clé (le
    /// dedans d'un contour lissé, ContourMesh), des traits en bandes fines, des lignes brisées aux angles joints, des
    /// pointes ; chaque sommet à la hauteur du relief, un peu au-dessus pour ne pas se mêler au sol. Un trait se coupe
    /// tous les demi-mètres pour suivre les pentes. Ses listes servent d'une construction à l'autre : rien n'est alloué
    /// une fois à leur taille.
    /// </summary>
    internal sealed class DrapedMesh
    {
        // Un trait suit le relief par morceaux de cette longueur, en mètres.
        internal const float Step = 0.5f;

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _triangles = new List<int>();
        private readonly Dictionary<long, int> _shared = new Dictionary<long, int>();
        private readonly List<Vector2> _line = new List<Vector2>();
        private Relief _relief;
        private float _lift;

        /// <summary>Commence un maillage vide sur ce relief, à cette hauteur au-dessus de lui.</summary>
        public void Begin(Relief relief, float lift)
        {
            _relief = relief;
            _lift = lift;
            _vertices.Clear();
            _triangles.Clear();
            _shared.Clear();
        }

        /// <summary>
        /// Le sommet qui porte cette clé, posé en ce point la première fois qu'on le demande : deux faces qui le citent
        /// le partagent.
        /// </summary>
        public int Point(long key, Vector2 position)
        {
            if (_shared.TryGetValue(key, out int index))
                return index;
            index = _vertices.Count;
            _vertices.Add(Ground(position));
            _shared.Add(key, index);
            return index;
        }

        /// <summary>
        /// Une face de ces trois sommets, dans le sens des aiguilles d'une montre vus d'en haut : sa face visible.
        /// </summary>
        public void AddFace(int a, int b, int c)
        {
            AddTriangle(a, b, c);
        }

        /// <summary>
        /// Une ligne brisée de cette largeur par ces points, fermée ou non : ses angles se joignent en onglet (la
        /// pointe bornée au double de la demi-largeur), chaque côté coupé tous les demi-mètres pour suivre les pentes.
        /// </summary>
        public void AddPolyline(IReadOnlyList<Vector2> points, bool closed, float width)
        {
            _line.Clear();
            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                _line.Add(points[i]);
                if (!closed && i == count - 1)
                    break;
                Vector2 next = points[(i + 1) % count];
                int pieces = Mathf.CeilToInt(Vector2.Distance(points[i], next) / Step);
                for (int piece = 1; piece < pieces; piece++)
                    _line.Add(Vector2.Lerp(points[i], next, (float)piece / pieces));
            }
            if (_line.Count < 2)
                return;
            float half = width * 0.5f;
            int first = _vertices.Count, total = _line.Count;
            for (int i = 0; i < total; i++)
            {
                Vector2 offset = Miter(i, closed, half);
                _vertices.Add(Ground(_line[i] + offset));
                _vertices.Add(Ground(_line[i] - offset));
            }
            int sides = closed ? total : total - 1;
            for (int i = 0; i < sides; i++)
            {
                int l0 = first + 2 * i, r0 = l0 + 1;
                int l1 = first + 2 * ((i + 1) % total), r1 = l1 + 1;
                AddTriangle(l0, l1, r1);
                AddTriangle(l0, r1, r0);
            }
        }

        /// <summary>Un trait de cette largeur, de a à b, prolongé d'une demi-largeur à chaque bout pour fermer les angles.</summary>
        public void AddSegment(Vector2 a, Vector2 b, float width)
        {
            Vector2 along = b - a;
            float length = along.magnitude;
            if (length < 1e-5f)
                return;
            Vector2 direction = along / length;
            float half = width * 0.5f;
            a -= direction * half;
            length += width;
            var left = new Vector2(-direction.y, direction.x) * half;
            int pieces = Mathf.Max(1, Mathf.CeilToInt(length / Step));
            int first = _vertices.Count;
            for (int i = 0; i <= pieces; i++)
            {
                Vector2 point = a + direction * (length * i / pieces);
                _vertices.Add(Ground(point + left));
                _vertices.Add(Ground(point - left));
            }
            for (int i = 0; i < pieces; i++)
            {
                int l0 = first + 2 * i, r0 = l0 + 1, l1 = l0 + 2, r1 = l0 + 3;
                AddTriangle(l0, l1, r1);
                AddTriangle(l0, r1, r0);
            }
        }

        /// <summary>Une pointe pleine : sa pointe, et les deux coins de sa base.</summary>
        public void AddTip(Vector2 tip, Vector2 baseLeft, Vector2 baseRight)
        {
            int first = _vertices.Count;
            _vertices.Add(Ground(baseLeft));
            _vertices.Add(Ground(tip));
            _vertices.Add(Ground(baseRight));
            AddTriangle(first, first + 1, first + 2);
        }

        /// <summary>Met ce qui a été ajouté dans ce maillage, qui perd ce qu'il avait.</summary>
        public void ApplyTo(Mesh mesh)
        {
            mesh.Clear();
            mesh.indexFormat = _vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(_vertices);
            mesh.SetTriangles(_triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        // Le décalage du bord gauche de la ligne en ce point : la moyenne des normales des côtés qui s'y touchent,
        // allongée pour garder la demi-largeur de chaque côté, bornée au double ; un bout ouvert garde la normale de
        // son côté.
        private Vector2 Miter(int i, bool closed, float half)
        {
            int total = _line.Count;
            bool hasPrevious = closed || i > 0, hasNext = closed || i < total - 1;
            Vector2 before = hasPrevious ? Normal(_line[(i - 1 + total) % total], _line[i]) : Vector2.zero;
            Vector2 after = hasNext ? Normal(_line[i], _line[(i + 1) % total]) : Vector2.zero;
            Vector2 sum = before + after;
            if (sum.sqrMagnitude < 1e-8f)
                return (hasPrevious ? before : after) * half;
            Vector2 miter = sum.normalized;
            float along = Vector2.Dot(miter, hasPrevious ? before : after);
            return miter * Mathf.Min(half / Mathf.Max(along, 1e-3f), 2f * half);
        }

        // La normale gauche du côté de a à b, de longueur 1 ; nulle pour un côté sans longueur.
        private static Vector2 Normal(Vector2 a, Vector2 b)
        {
            Vector2 along = b - a;
            float length = along.magnitude;
            return length < 1e-6f ? Vector2.zero : new Vector2(-along.y, along.x) / length;
        }

        private Vector3 Ground(Vector2 point)
        {
            return new Vector3(point.x, _relief.HeightAt(point.x, point.y) + _lift, point.y);
        }

        private void AddTriangle(int a, int b, int c)
        {
            _triangles.Add(a);
            _triangles.Add(b);
            _triangles.Add(c);
        }
    }
}
