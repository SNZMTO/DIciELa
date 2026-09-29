using System;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Une carte de surface : sur une grille posée dans un plan (x et z pour le dessus d'une tête, x et y pour l'avant
    /// d'une face ou le dos d'un corps), la position de la surface le long du troisième axe, case par case (SAVOIR, 2.11).
    /// Une tête ou un corps a la carte de sa surface ; une pièce, celle de sa face qui la touche, dans ses propres axes à
    /// sa taille d'origine. Mesurée par le menu, jamais réglée à la main. Rangée en millimètres sur deux octets par case,
    /// pour rester petite dans les fiches.
    /// </summary>
    [Serializable]
    public sealed class SurfaceMap
    {
        /// <summary>La valeur d'une case sans surface.</summary>
        public const float Empty = -1e9f;

        private const float MetersPerUnit = 0.001f;

        [SerializeField] private Vector2 _origin;
        [SerializeField] private float _cell;
        [SerializeField] private int _columns;
        [SerializeField] private int _rows;
        [SerializeField] private byte[] _values = Array.Empty<byte>();

        /// <summary>Une carte vide, pas encore mesurée.</summary>
        public SurfaceMap()
        {
        }

        /// <summary>Une carte de colonnes × lignes cases de cell mètres, depuis le coin origin, de ces valeurs (Empty : vide).</summary>
        public SurfaceMap(Vector2 origin, float cell, int columns, int rows, float[] values)
        {
            if (cell <= 0f || columns <= 0 || rows <= 0 || values == null || values.Length != columns * rows)
                throw new ArgumentException("Une carte de surface demande des cases et une valeur par case.");
            _origin = origin;
            _cell = cell;
            _columns = columns;
            _rows = rows;
            _values = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                short stored = values[i] <= Empty
                    ? short.MinValue
                    : (short)Mathf.Clamp(Mathf.RoundToInt(values[i] / MetersPerUnit), short.MinValue + 1, short.MaxValue);
                _values[2 * i] = (byte)(stored & 0xFF);
                _values[2 * i + 1] = (byte)((stored >> 8) & 0xFF);
            }
        }

        /// <summary>Vrai si la carte n'a aucune case : pas encore mesurée.</summary>
        public bool IsEmpty => _values == null || _values.Length == 0 || _columns * _rows * 2 != _values.Length;

        public int Columns => _columns;
        public int Rows => _rows;

        /// <summary>La valeur d'une case, en mètres ; Empty si elle n'a pas de surface.</summary>
        public float At(int column, int row)
        {
            int i = 2 * (row * _columns + column);
            var stored = (short)(_values[i] | (_values[i + 1] << 8));
            return stored == short.MinValue ? Empty : stored * MetersPerUnit;
        }

        /// <summary>Le centre d'une case, dans le plan de la carte.</summary>
        public Vector2 CenterOf(int column, int row)
        {
            return _origin + new Vector2((column + 0.5f) * _cell, (row + 0.5f) * _cell);
        }

        /// <summary>La valeur de la case sous ce point du plan ; Empty hors de la carte ou sans surface.</summary>
        public float Sample(Vector2 point)
        {
            if (IsEmpty)
                return Empty;
            int column = Mathf.FloorToInt((point.x - _origin.x) / _cell);
            int row = Mathf.FloorToInt((point.y - _origin.y) / _cell);
            if (column < 0 || row < 0 || column >= _columns || row >= _rows)
                return Empty;
            return At(column, row);
        }
    }
}
