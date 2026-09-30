using System;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un anneau carré autour d'une maille : les décalages à exactement tant de mailles d'elle sur l'un des deux axes au
    /// moins, et pas plus sur l'autre ; l'anneau 0 est la maille elle-même. Dans l'ordre : la rangée du bas, de gauche à
    /// droite, puis les deux bords de chaque rangée du milieu, puis la rangée du haut ; le même ordre à chaque fois.
    /// Chercher anneau après anneau voit les places proches d'abord, à √2 près : les coins d'un anneau sont plus loin que
    /// le milieu du suivant. Le seul parcours par anneaux ; comme les mailles couvertes, il se parcourt lui-même avec
    /// <c>foreach</c> et n'alloue rien.
    /// </summary>
    public struct SquareRing
    {
        private readonly int _ring;
        private bool _started;
        private int _dx;
        private int _dy;

        public SquareRing(int ring)
        {
            if (ring < 0)
                throw new ArgumentOutOfRangeException(nameof(ring), "Un anneau est à 0 maille ou plus.");

            _ring = ring;
            _started = false;
            _dx = 0;
            _dy = 0;
        }

        /// <summary>Le décalage en cours, en mailles : en x la colonne, en y la rangée sur l'axe z du monde.</summary>
        public Vector2Int Current => new Vector2Int(_dx, _dy);

        public SquareRing GetEnumerator()
        {
            return this;
        }

        /// <summary>Passe au décalage suivant de l'anneau ; chacun est vu une fois.</summary>
        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                _dx = -_ring;
                _dy = -_ring;
                return true;
            }
            // Sur les rangées du bas et du haut, toutes les places ; entre elles, les deux des bords.
            _dx += _dy == -_ring || _dy == _ring ? 1 : 2 * _ring;
            if (_dx <= _ring)
                return true;
            _dx = -_ring;
            _dy++;
            return _dy <= _ring;
        }
    }
}
