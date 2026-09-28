using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un trajet (SAVOIR, 2.1) : l'escalier de mailles que trouve la recherche, du départ à l'arrivée comprises, et la
    /// ligne tendue qu'on en tire, celle où l'on marche : ses coins, en mètres, du milieu de la maille de départ à celui
    /// de l'arrivée, et sa longueur. Seule la recherche le remplit ; celui qui cherche le garde d'une recherche à
    /// l'autre, et ses listes ne s'allouent plus.
    /// </summary>
    public sealed class Path
    {
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private readonly List<Vector2> _corners = new List<Vector2>();

        /// <summary>L'escalier : chaque maille est voisine de la précédente, du départ à l'arrivée comprises.</summary>
        public IReadOnlyList<Vector2Int> Cells => _cells;

        /// <summary>La ligne tendue : ses coins, en mètres sur le plan XZ, du départ à l'arrivée.</summary>
        public IReadOnlyList<Vector2> Corners => _corners;

        /// <summary>La longueur de la ligne tendue, en mètres : ce que marche un piéton.</summary>
        public float Length { get; private set; }

        internal void Clear()
        {
            _cells.Clear();
            _corners.Clear();
            Length = 0f;
        }

        /// <summary>Ajoute une maille à l'escalier ; la recherche les ajoute de l'arrivée au départ, puis les retourne.</summary>
        internal void AddCell(Vector2Int cell)
        {
            _cells.Add(cell);
        }

        internal void ReverseCells()
        {
            _cells.Reverse();
        }

        internal void AddCorner(Vector2 corner)
        {
            if (_corners.Count > 0)
                Length += Vector2.Distance(_corners[_corners.Count - 1], corner);
            _corners.Add(corner);
        }
    }
}
