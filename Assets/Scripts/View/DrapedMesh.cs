using System.Collections.Generic;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Un maillage posé sur le relief (décision 206) : des mailles du maillage en carrés qui partagent leurs coins, des
    /// traits en bandes fines, des pointes ; chaque sommet à la hauteur du relief, un peu au-dessus pour ne pas se mêler
    /// au sol. Un trait se coupe tous les demi-mètres pour suivre les pentes. Ses listes servent d'une construction à
    /// l'autre : rien n'est alloué une fois à leur taille.
    /// </summary>
    internal sealed class DrapedMesh
    {
        // Un trait suit le relief par morceaux de cette longueur, en mètres.
        private const float Step = 0.5f;

        // La pointe d'une place de charrette, en parts de sa demi-largeur : son écart au bord avant, sa longueur, sa
        // demi-base.
        private const float TipMargin = 0.2f;
        private const float TipLength = 0.6f;
        private const float TipHalfBase = 0.45f;

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _triangles = new List<int>();
        private readonly Dictionary<int, int> _corners = new Dictionary<int, int>();
        private Relief _relief;
        private float _lift;

        /// <summary>Commence un maillage vide sur ce relief, à cette hauteur au-dessus de lui.</summary>
        public void Begin(Relief relief, float lift)
        {
            _relief = relief;
            _lift = lift;
            _vertices.Clear();
            _triangles.Clear();
            _corners.Clear();
        }

        /// <summary>Des mailles du maillage, en carrés qui se touchent : deux voisines partagent leurs deux coins.</summary>
        public void AddCells(IReadOnlyList<Vector2Int> cells, NavGrid grid)
        {
            float size = grid.Metrics.CellSize;
            int stride = grid.Width + 1;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                Vector2 min = grid.CellCenter(cell) - new Vector2(size, size) * 0.5f;
                int a = Corner(cell.x, cell.y, stride, min);
                int b = Corner(cell.x + 1, cell.y, stride, min + new Vector2(size, 0f));
                int c = Corner(cell.x + 1, cell.y + 1, stride, min + new Vector2(size, size));
                int d = Corner(cell.x, cell.y + 1, stride, min + new Vector2(0f, size));
                // Vus d'en haut, dans le sens des aiguilles d'une montre : la face visible pour Unity.
                AddTriangle(a, d, c);
                AddTriangle(a, c, b);
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

        /// <summary>Le tour d'un rectangle tourné, en traits de cette largeur.</summary>
        public void AddOutline(RotatedRect rect, float width)
        {
            for (int corner = 0; corner < 4; corner++)
                AddSegment(rect.Corner(corner), rect.Corner((corner + 1) % 4), width);
        }

        /// <summary>Le tour d'un cercle, en traits de cette largeur, d'au moins seize côtés.</summary>
        public void AddCircle(Vector2 center, float radius, float width)
        {
            int sides = Mathf.Clamp(Mathf.CeilToInt(2f * Mathf.PI * radius / Step), 16, 128);
            Vector2 previous = center + new Vector2(radius, 0f);
            for (int i = 1; i <= sides; i++)
            {
                float angle = 2f * Mathf.PI * i / sides;
                Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                AddSegment(previous, next, width);
                previous = next;
            }
        }

        /// <summary>
        /// Une place de charrette : son cadre, et une pointe près de son bord avant, vers où l'avant de la charrette se
        /// gare (un remplaçant simple, comme le fanion des arbres).
        /// </summary>
        public void AddMark(RotatedRect footprint, float width)
        {
            AddOutline(footprint, width);
            float halfWidth = footprint.Size.x * 0.5f, halfLength = footprint.Size.y * 0.5f;
            Vector2 tip = footprint.ToWorld(new Vector2(0f, halfLength - halfWidth * TipMargin));
            float back = halfLength - halfWidth * (TipMargin + TipLength);
            AddTip(tip, footprint.ToWorld(new Vector2(-halfWidth * TipHalfBase, back)),
                footprint.ToWorld(new Vector2(halfWidth * TipHalfBase, back)));
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

        private int Corner(int x, int y, int stride, Vector2 position)
        {
            int key = x + y * stride;
            if (_corners.TryGetValue(key, out int index))
                return index;
            index = _vertices.Count;
            _vertices.Add(Ground(position));
            _corners.Add(key, index);
            return index;
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
