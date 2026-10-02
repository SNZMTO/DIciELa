using System;
using System.Collections.Generic;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les arbres et les rochers de la carte (GDD, section 2 ; ARCHITECTURE, section 4) : ceux que la carte décrit au
    /// lancement, ceux que le joueur a marqués pour être retirés, dans l'ordre où il les a marqués, et ceux qui sont
    /// retirés. Debout, chacun prend ses mailles ; deux pieds peints l'un contre l'autre se partagent une maille, que le
    /// dernier de la carte tient. Retiré, un pied libère les siennes, et ses voisins debout reprennent celles qu'il tenait
    /// sous eux : une maille reste prise tant qu'un pied debout la couvre. Un pied entièrement recouvert par un autre ne
    /// se rejoint pas : il attend que celui qui le couvre soit retiré, puis reprend ses mailles. Les événements d'un pas
    /// attendent la fin de leur couche (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Nature
    {
        private enum HappeningKind
        {
            Marked,
            Unmarked,
            Removed
        }

        private readonly List<NatureItem> _all = new List<NatureItem>();
        private readonly List<NatureItem> _marked = new List<NatureItem>();
        private readonly List<KeyValuePair<HappeningKind, NatureItem>> _happened =
            new List<KeyValuePair<HappeningKind, NatureItem>>();
        private readonly NavGrid _grid;

        public Nature(NavGrid grid, IReadOnlyList<NatureSpot> spots)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (spots == null)
                throw new ArgumentNullException(nameof(spots));

            _grid = grid;
            for (int i = 0; i < spots.Count; i++)
            {
                var item = new NatureItem(i + 1, spots[i]);
                _all.Add(item);
                grid.SetOccupant(item.Footprint, item.Occupant, walkable: false);
            }
        }

        /// <summary>Levé quand un arbre ou un rocher est marqué pour être retiré.</summary>
        public event Action<NatureItem> ItemMarked;

        /// <summary>Levé quand un arbre ou un rocher n'est plus marqué, sans avoir été retiré.</summary>
        public event Action<NatureItem> ItemUnmarked;

        /// <summary>Levé quand un arbre ou un rocher est retiré : il a quitté la carte.</summary>
        public event Action<NatureItem> ItemRemoved;

        /// <summary>Tous, retirés compris, dans l'ordre de la carte : le numéro n est à la place n − 1.</summary>
        public IReadOnlyList<NatureItem> All => _all;

        /// <summary>Ceux qui sont marqués, dans l'ordre où le joueur les a marqués : on les retire dans cet ordre.</summary>
        public IReadOnlyList<NatureItem> Marked => _marked;

        /// <summary>L'arbre ou le rocher qui porte ce numéro ; null s'il n'y en a pas.</summary>
        public NatureItem Find(int id)
        {
            return id >= 1 && id <= _all.Count ? _all[id - 1] : null;
        }

        /// <summary>Le marque pour être retiré ; rien s'il l'est déjà ou s'il est retiré. Seul le monde l'appelle.</summary>
        internal void Mark(NatureItem item)
        {
            if (item.IsRemoved || item.IsMarked)
                return;
            item.SetMarked(true);
            _marked.Add(item);
            _happened.Add(new KeyValuePair<HappeningKind, NatureItem>(HappeningKind.Marked, item));
        }

        /// <summary>Ne le marque plus ; rien s'il ne l'était pas. Seul le monde l'appelle.</summary>
        internal void Unmark(NatureItem item)
        {
            if (!item.IsMarked)
                return;
            item.SetMarked(false);
            _marked.Remove(item);
            _happened.Add(new KeyValuePair<HappeningKind, NatureItem>(HappeningKind.Unmarked, item));
        }

        /// <summary>
        /// Le retire de la carte : ses mailles se libèrent, sauf celles qu'un voisin debout couvre aussi. Seule l'économie
        /// l'appelle, le travail fini.
        /// </summary>
        internal void Remove(NatureItem item)
        {
            if (item.IsRemoved)
                throw new InvalidOperationException("L'arbre ou le rocher n° " + item.Id + " est déjà retiré.");
            if (item.IsMarked)
                _marked.Remove(item);
            item.BecomeRemoved();
            _grid.ReleaseOccupant(item.Footprint, item.Occupant);
            GiveBackShared(item.Footprint);
            _happened.Add(new KeyValuePair<HappeningKind, NatureItem>(HappeningKind.Removed, item));
        }

        // Les pieds debout qui peuvent partager une maille avec celui qui part (leurs cercles circonscrits, élargis d'une
        // diagonale de maille, se touchent) reprennent les mailles libérées qu'ils couvrent, du dernier de la carte au
        // premier : une maille partagée revient au dernier, comme au lancement. Un passage sur tous les pieds, à chaque
        // retrait seulement ; un pied qui ne partageait rien n'a aucune maille libre à reprendre.
        private void GiveBackShared(RotatedRect gone)
        {
            float cellDiagonal = _grid.Metrics.CellSize * Mathf.Sqrt(2f);
            for (int i = _all.Count - 1; i >= 0; i--)
            {
                NatureItem other = _all[i];
                if (other.IsRemoved)
                    continue;
                RotatedRect footprint = other.Footprint;
                float reach = (gone.Size.magnitude + footprint.Size.magnitude) * 0.5f + cellDiagonal;
                if ((footprint.Center - gone.Center).sqrMagnitude <= reach * reach)
                    _grid.Reclaim(footprint, other.Occupant);
            }
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                {
                    NatureItem item = _happened[i].Value;
                    switch (_happened[i].Key)
                    {
                        case HappeningKind.Marked:
                            ItemMarked?.Invoke(item);
                            break;
                        case HappeningKind.Unmarked:
                            ItemUnmarked?.Invoke(item);
                            break;
                        default:
                            ItemRemoved?.Invoke(item);
                            break;
                    }
                }
            }
            finally
            {
                _happened.Clear();
            }
        }
    }
}
