using System;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Sortir d'une construction posée sur soi (GDD, section 9 ; décisions 73 et 88) : tout droit, à travers elle, vers la
    /// maille libre qui la borde du côté du plus grand îlot (SAVOIR, 2.1). Enfermé, on attend que le maillage change pour
    /// chercher de nouveau ; si le maillage change pendant qu'on sort, on revérifie sa maille de sortie. Le chemin de
    /// sortie traverse la construction : il ne suit pas le maillage.
    /// </summary>
    internal sealed class GroundExit
    {
        private readonly NavGrid _grid;
        private readonly WayOutFinder _wayOuts;
        private readonly float _metersPerStep;
        private bool _leaving;
        private Vector2 _wayOut;
        private bool _enclosed;
        // La version du maillage où il a cherché sa sortie : enfermé, il attend qu'elle change ; en sortant, il revérifie.
        private int _wayOutVersion;

        public GroundExit(NavGrid grid, Islands islands, float metersPerStep)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));

            _grid = grid;
            _wayOuts = new WayOutFinder(grid, islands);
            _metersPerStep = metersPerStep;
        }

        /// <summary>Vrai pendant qu'il sort, tout droit à travers la construction.</summary>
        public bool IsLeaving => _leaving;

        /// <summary>Vrai quand il est enfermé, immobile, sans maille libre au bord de ce qui le couvre.</summary>
        public bool IsEnclosed => _enclosed;

        /// <summary>
        /// Un pas de sortie : vrai tant qu'il sort ou qu'il est enfermé, et il ne fait rien d'autre ce pas-là.
        /// <paramref name="justLeft"/> dit qu'il vient d'atteindre sa maille de sortie : il reprend ce qu'il faisait.
        /// </summary>
        public bool Step(ref Vector2 position, out bool justLeft)
        {
            justLeft = false;
            // Le maillage a changé pendant qu'il sort : si sa maille de sortie a été prise, il en cherche une autre.
            if (_leaving && _wayOutVersion != _grid.Version)
            {
                _wayOutVersion = _grid.Version;
                _leaving = _grid.IsWalkable(_grid.CellOf(_wayOut));
            }
            if (!_leaving)
            {
                if (_grid.IsWalkable(_grid.CellOf(position)))
                {
                    _enclosed = false;
                    return false;
                }
                // Enfermé : il attend que le maillage change pour chercher de nouveau.
                if (_enclosed && _wayOutVersion == _grid.Version)
                    return true;
                _wayOutVersion = _grid.Version;
                _enclosed = !_wayOuts.TryFind(position, out Vector2Int free);
                if (_enclosed)
                    return true;
                _wayOut = _grid.CellCenter(free);
                _leaving = true;
            }

            if (Vector2.Distance(position, _wayOut) > _metersPerStep)
            {
                position = Vector2.MoveTowards(position, _wayOut, _metersPerStep);
                return true;
            }
            position = _wayOut;
            _leaving = false;
            justLeft = true;
            return true;
        }
    }
}
