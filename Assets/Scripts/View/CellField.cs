using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le champ lissé d'un ensemble de mailles (décision 227). En chaque coin de maille, il donne la part de l'ensemble
    /// autour de lui, pesée par un flou binomial de quelques mailles. Son niveau 0,5 suit les mailles le long d'un bord
    /// droit et arrondit leurs coins : c'est le contour qu'on dessine (ContourMesh) et le bord d'un sol peint
    /// (FloorWeights), sans escalier. Ses tableaux servent d'un champ à l'autre : rien n'est alloué une fois à leur
    /// taille.
    /// </summary>
    internal sealed class CellField
    {
        /// <summary>Le niveau du contour : au-dessus, dedans.</summary>
        public const float Edge = 0.5f;

        private float[] _mask = Array.Empty<float>();
        private float[] _rows = Array.Empty<float>();
        private float[] _values = Array.Empty<float>();
        private float[] _weights = Array.Empty<float>();
        private RectInt _cells;
        private float _size;
        private int _radius;

        /// <summary>Le nombre de coins sur chaque axe : une ligne de plus que de mailles.</summary>
        public int CornersX => _cells.width + 1;

        public int CornersY => _cells.height + 1;

        /// <summary>Vrai si aucune maille n'a été ajoutée depuis Begin.</summary>
        public bool IsEmpty { get; private set; } = true;

        /// <summary>
        /// La région qu'il faut pour lisser ces mailles : elles, et autour d'elles le rayon du flou, plus une maille,
        /// pour que le contour se ferme dans la région.
        /// </summary>
        public static RectInt Around(RectInt cells, int radiusCells)
        {
            int margin = radiusCells + 1;
            return new RectInt(cells.xMin - margin, cells.yMin - margin, cells.width + 2 * margin,
                cells.height + 2 * margin);
        }

        /// <summary>Le rayon du flou en mailles pour cet arrondi en mètres : une maille au moins.</summary>
        public static int RadiusCells(float roundingMeters, float cellSize)
        {
            return Mathf.Max(1, Mathf.RoundToInt(roundingMeters / cellSize));
        }

        /// <summary>Le champ lissé de ces mailles, sur la région qu'il leur faut (Around) ; vide sans maille.</summary>
        public void Build(IReadOnlyList<Vector2Int> cells, float cellSize, int radiusCells)
        {
            if (cells.Count == 0)
            {
                Begin(new RectInt(0, 0, 0, 0), cellSize, radiusCells);
                return;
            }
            Vector2Int min = cells[0], max = cells[0];
            for (int k = 1; k < cells.Count; k++)
            {
                min = Vector2Int.Min(min, cells[k]);
                max = Vector2Int.Max(max, cells[k]);
            }
            Begin(Around(new RectInt(min, max - min + Vector2Int.one), radiusCells), cellSize, radiusCells);
            for (int k = 0; k < cells.Count; k++)
                Add(cells[k]);
            Smooth();
        }

        /// <summary>
        /// Commence un champ vide sur ces mailles (Around), de ce côté en mètres, flouté sur ce rayon en mailles (le
        /// flou en prend le double, de part et d'autre de chaque coin).
        /// </summary>
        public void Begin(RectInt cells, float cellSize, int radiusCells)
        {
            _cells = cells;
            _size = cellSize;
            _radius = Mathf.Max(1, radiusCells);
            int count = cells.width * cells.height;
            Grow(ref _mask, count);
            Array.Clear(_mask, 0, count);
            Grow(ref _rows, CornersX * cells.height);
            Grow(ref _values, CornersX * CornersY);
            Grow(ref _weights, 2 * _radius);
            IsEmpty = true;
        }

        /// <summary>Ajoute cette maille à l'ensemble ; hors de la région, elle ne compte pas.</summary>
        public void Add(Vector2Int cell)
        {
            int x = cell.x - _cells.xMin, y = cell.y - _cells.yMin;
            if (x < 0 || y < 0 || x >= _cells.width || y >= _cells.height)
                return;
            _mask[x + y * _cells.width] = 1f;
            IsEmpty = false;
        }

        /// <summary>
        /// Calcule le champ en chaque coin, d'après les mailles ajoutées : deux passes du flou, en x puis en y.
        /// </summary>
        public void Smooth()
        {
            Binomial(_weights, 2 * _radius);
            int width = _cells.width, height = _cells.height, cornersX = CornersX;
            for (int y = 0; y < height; y++)
            {
                for (int i = 0; i < cornersX; i++)
                    _rows[i + y * cornersX] = Blur(_mask, i - _radius, y * width, 1, width);
            }
            for (int j = 0; j < CornersY; j++)
            {
                for (int i = 0; i < cornersX; i++)
                    _values[i + j * cornersX] = Blur(_rows, j - _radius, i, cornersX, height);
            }
        }

        /// <summary>Le champ en ce coin (0 : celui du bas à gauche de la région).</summary>
        public float Value(int i, int j)
        {
            return _values[i + j * CornersX];
        }

        /// <summary>La place de ce coin sur le sol, en mètres.</summary>
        public Vector2 Corner(int i, int j)
        {
            return new Vector2((_cells.xMin + i) * _size, (_cells.yMin + j) * _size);
        }

        /// <summary>Le champ en ce point du sol, entre ses quatre coins ; 0 hors de la région.</summary>
        public float Sample(Vector2 point)
        {
            float u = point.x / _size - _cells.xMin, v = point.y / _size - _cells.yMin;
            int i = Mathf.FloorToInt(u), j = Mathf.FloorToInt(v);
            if (i < 0 || j < 0 || i >= CornersX - 1 || j >= CornersY - 1)
                return 0f;
            float tu = u - i, tv = v - j;
            float bottom = Mathf.Lerp(Value(i, j), Value(i + 1, j), tu);
            float top = Mathf.Lerp(Value(i, j + 1), Value(i + 1, j + 1), tu);
            return Mathf.Lerp(bottom, top, tv);
        }

        // La somme pesée de count valeurs d'une ligne (ou d'une colonne), à partir du rang from, d'un pas de stride ;
        // ce qui sort de la ligne vaut 0.
        private float Blur(float[] source, int from, int offset, int stride, int length)
        {
            float sum = 0f;
            for (int k = 0; k < 2 * _radius; k++)
            {
                int at = from + k;
                if (at >= 0 && at < length)
                    sum += _weights[k] * source[offset + at * stride];
            }
            return sum;
        }

        // Les poids binomiaux d'un flou de count valeurs (1 3 3 1, 1 5 10 10 5 1…), de somme 1.
        private static void Binomial(float[] weights, int count)
        {
            weights[0] = 1f;
            for (int n = 1; n < count; n++)
            {
                weights[n] = 1f;
                for (int k = n - 1; k > 0; k--)
                    weights[k] += weights[k - 1];
            }
            float total = Mathf.Pow(2f, count - 1);
            for (int k = 0; k < count; k++)
                weights[k] /= total;
        }

        private static void Grow(ref float[] array, int count)
        {
            if (array.Length < count)
                array = new float[count];
        }
    }
}
