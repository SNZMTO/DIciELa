using System;
using Diciela.Data;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le pas d'une charrette tirée, maille par maille (GDD, section 9 ; décision 209), en part du pas de qui la tire :
    /// dans l'herbe, celui de la fiche de la charrette ; sur un sol qui sert (bâti, ou qu'on efface), celui de la fiche du
    /// sol ; un fantôme n'accélère personne. La recherche des trajets en tire le prix d'une maille pour la charrette,
    /// l'inverse de son pas, en entiers (SAVOIR, 2.1), et le plus bas d'aujourd'hui, pour une estimation qui ne dépasse
    /// jamais le vrai coût. Une lecture des sols, qui ne garde que les prix de leurs sortes.
    /// </summary>
    public sealed class CartPace
    {
        // Le prix d'une maille au pas de qui tire : assez grand pour que l'inverse d'un pas tienne en entiers au
        // pourcent près (l'herbe à la moitié du pas : 24 ; la terre : 12 ; un sol dur, une fois et demie : 8).
        private const float CostAtFullPace = 12f;

        private readonly NavGrid _grid;
        private readonly FloorCells _cells;
        private readonly float _grassSpeed;
        private readonly int _grassCost;
        // Par sorte de sol, son prix, calculé la première fois qu'on le lit ; 0 : pas encore.
        private readonly int[] _costOfKind = new int[byte.MaxValue + 1];

        /// <summary>
        /// Le pas des charrettes de cette fiche sur ces sols ; sans charrette au catalogue, personne n'en tire : dans
        /// l'herbe, le pas d'un piéton.
        /// </summary>
        public CartPace(NavGrid grid, Floors floors, CartDefinition cart)
        {
            if (ReferenceEquals(floors, null))
                throw new ArgumentNullException(nameof(floors));

            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _cells = floors.Cells;
            _grassSpeed = ReferenceEquals(cart, null) ? 1f : cart.SpeedFactor;
            _grassCost = CostOf(_grassSpeed);
        }

        /// <summary>Le sol de chaque maille, que lit aussi l'estimation de la charrette.</summary>
        internal FloorCells Cells => _cells;

        /// <summary>Le prix d'une maille d'herbe, ou d'un sol qui ne va pas plus vite qu'elle.</summary>
        internal int GrassCost => _grassCost;

        /// <summary>Le pas d'une charrette sur cette maille, en part du pas de qui la tire.</summary>
        public float SpeedAt(Vector2Int cell)
        {
            int kind = _cells.ServingKindAt(_grid.IndexOf(cell));
            return kind == 0 ? _grassSpeed : _cells.Kind(kind).CartSpeedFactor;
        }

        /// <summary>Le prix d'entrer dans la maille à cette place du maillage, pour une charrette.</summary>
        internal int CostAt(int index)
        {
            int kind = _cells.ServingKindAt(index);
            return kind == 0 ? _grassCost : CostOfKind(kind);
        }

        /// <summary>
        /// Le prix le plus bas d'une maille aujourd'hui : l'herbe, ou le plus rapide des sols qui servent. L'estimation de
        /// la recherche s'y compte, si bien qu'elle ne dépasse jamais le vrai coût.
        /// </summary>
        internal int LowestCost
        {
            get
            {
                int lowest = _grassCost;
                for (int kind = 1; kind < _cells.KindCount; kind++)
                {
                    if (_cells.Serves(kind))
                        lowest = Math.Min(lowest, CostOfKind(kind));
                }
                return lowest;
            }
        }

        private int CostOfKind(int kind)
        {
            if (_costOfKind[kind] == 0)
                _costOfKind[kind] = CostOf(_cells.Kind(kind).CartSpeedFactor);
            return _costOfKind[kind];
        }

        private static int CostOf(float speed)
        {
            return Math.Max(1, Mathf.RoundToInt(CostAtFullPace / speed));
        }
    }
}
