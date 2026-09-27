using System;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le maillage : un quadrillage invisible, plus fin que le mètre, qui dit ce qui occupe chaque maille (SAVOIR, 2.1).
    /// Il est déduit des constructions, jamais sauvegardé : chaque maille garde l'identifiant de ce qui la couvre (0 :
    /// libre) et le nombre de portes qui en ont besoin pour rester dégagées. Une maille se désigne par un Vector2Int :
    /// en x sa colonne, en y sa rangée sur l'axe z du monde.
    /// </summary>
    public sealed class NavGrid
    {
        private readonly int[] _occupant;
        private readonly byte[] _doorCount;

        public NavGrid(Relief relief, GridMetrics metrics)
        {
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Metrics = metrics;
            Width = metrics.CellCount(relief.SizeX);
            Depth = metrics.CellCount(relief.SizeZ);
            _occupant = new int[Width * Depth];
            _doorCount = new byte[Width * Depth];
        }

        public GridMetrics Metrics { get; }
        public int Width { get; }
        public int Depth { get; }

        /// <summary>L'identifiant de ce qui occupe cette maille, ou 0 si elle est libre.</summary>
        public int OccupantAt(Vector2Int cell)
        {
            return _occupant[IndexOf(cell)];
        }

        /// <summary>Vrai si cette maille doit rester dégagée devant une porte.</summary>
        public bool IsDoorway(Vector2Int cell)
        {
            return _doorCount[IndexOf(cell)] > 0;
        }

        /// <summary>Le milieu de cette maille, en mètres sur le plan XZ.</summary>
        public Vector2 CellCenter(Vector2Int cell)
        {
            return new Vector2(Metrics.CellCenter(cell.x), Metrics.CellCenter(cell.y));
        }

        /// <summary>Les mailles que couvre ce rectangle (leur milieu tombe dedans), à parcourir avec foreach.</summary>
        public CoveredCells CellsCoveredBy(RotatedRect rect)
        {
            return new CoveredCells(this, rect);
        }

        /// <summary>Donne à chaque maille du rectangle cet occupant (0 pour la libérer).</summary>
        internal void SetOccupant(RotatedRect rect, int occupant)
        {
            foreach (Vector2Int cell in CellsCoveredBy(rect))
                _occupant[IndexOf(cell)] = occupant;
        }

        /// <summary>Compte une porte de plus devant chaque maille du rectangle.</summary>
        internal void AddDoorway(RotatedRect rect)
        {
            foreach (Vector2Int cell in CellsCoveredBy(rect))
            {
                int index = IndexOf(cell);
                if (_doorCount[index] < byte.MaxValue)
                    _doorCount[index]++;
            }
        }

        private int IndexOf(Vector2Int cell)
        {
            return cell.y * Width + cell.x;
        }
    }
}
