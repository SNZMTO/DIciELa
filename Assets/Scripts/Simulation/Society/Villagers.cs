using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Les habitants de la ville (ARCHITECTURE, section 4) : la vérité de qui vit ici et où. Pour l'instant, la Chimère,
    /// que le joueur crée au lancement d'une partie ; les arrivées viendront avec les logements. Chacun joue son pas
    /// dans l'ordre de son arrivée. Les événements d'un pas attendent la fin de leur couche, dans une seule file
    /// (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class Villagers
    {
        private readonly List<Villager> _all = new List<Villager>();
        private readonly List<Happening> _happened = new List<Happening>();
        private readonly NavGrid _grid;
        private readonly Islands _islands;
        private readonly PathFinder _finder;
        private readonly Clock _clock;
        private int _nextId = 1;

        public Villagers(NavGrid grid, Islands islands, PathFinder finder, Clock clock)
        {
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));
            if (ReferenceEquals(finder, null))
                throw new ArgumentNullException(nameof(finder));
            if (ReferenceEquals(clock, null))
                throw new ArgumentNullException(nameof(clock));

            _grid = grid;
            _islands = islands;
            _finder = finder;
            _clock = clock;
        }

        /// <summary>Levé quand un habitant arrive en ville : pour l'instant, la Chimère, à sa création.</summary>
        public event Action<Villager> VillagerArrived;

        /// <summary>Levé quand un ordre « créer la Chimère » est refusé, avec la sorte refusée : elle existe déjà.</summary>
        public event Action<VillagerKindDefinition> ChimeraRefused;

        public IReadOnlyList<Villager> All => _all;

        /// <summary>La Chimère, une fois créée ; null avant.</summary>
        public Villager Chimera { get; private set; }

        /// <summary>
        /// Crée la Chimère au point d'arrivée, sur le milieu de sa maille. Une seule par monde : si elle existe déjà,
        /// faux, rien ne change, et <see cref="ChimeraRefused"/> le dira. Seul l'ordre « créer la Chimère » l'appelle.
        /// </summary>
        internal bool CreateChimera(VillagerKindDefinition kind, Vector2 arrivalPoint)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));
            if (Chimera != null)
            {
                _happened.Add(new Happening(null, kind));
                return false;
            }

            Vector2Int cell = _grid.CellOf(arrivalPoint);
            cell = new Vector2Int(Math.Clamp(cell.x, 0, _grid.Width - 1), Math.Clamp(cell.y, 0, _grid.Depth - 1));
            var chimera = new Villager(_nextId++, kind, _grid.CellCenter(cell), _grid, _islands, _finder, _clock);
            _all.Add(chimera);
            Chimera = chimera;
            _happened.Add(new Happening(chimera, null));
            return true;
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                {
                    Happening happening = _happened[i];
                    if (happening.Arrived != null)
                        VillagerArrived?.Invoke(happening.Arrived);
                    else
                        ChimeraRefused?.Invoke(happening.Refused);
                }
            }
            finally
            {
                _happened.Clear();
            }
        }

        /// <summary>Joue un pas pour chaque habitant, dans l'ordre de leur arrivée. Seul le monde l'appelle.</summary>
        internal void Step()
        {
            for (int i = 0; i < _all.Count; i++)
                _all[i].Step();
        }

        /// <summary>Un ordre joué, en attendant d'être annoncé : l'habitant arrivé, ou la sorte refusée.</summary>
        private readonly struct Happening
        {
            public Happening(Villager arrived, VillagerKindDefinition refused)
            {
                Arrived = arrived;
                Refused = refused;
            }

            public Villager Arrived { get; }
            public VillagerKindDefinition Refused { get; }
        }
    }
}
