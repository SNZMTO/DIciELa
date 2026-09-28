using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Un habitant (ARCHITECTURE, section 4) : sa sorte, sa place et ce qu'il fait. Sa place est sa position, en mètres
    /// sur le plan XZ ; il garde aussi celle du pas d'avant, pour que la vue le montre entre les deux. Sans poste, il
    /// flâne. Une construction posée sur lui ne l'arrête pas : il en sort à pied, tout droit vers la maille libre la
    /// plus proche (décisions 73 et 74), puis reprend ce qu'il faisait. Chaque pas tire son hasard de son numéro et du
    /// numéro du pas : les mêmes ordres rejouent la même partie.
    /// </summary>
    public sealed class Villager
    {
        // Jusqu'où chercher une maille libre pour sortir d'une construction, en mètres : plus que la plus grande.
        private const float WayOutSearchMeters = 20f;

        private readonly NavGrid _grid;
        private readonly Clock _clock;
        private readonly Walk _walk;
        private readonly Wander _wander;
        private readonly float _metersPerStep;
        private readonly int _wayOutRings;
        private bool _leaving;
        private Vector2 _wayOut;
        private bool _enclosed;
        private int _enclosedVersion;

        internal Villager(int id, VillagerKindDefinition kind, Vector2 position, NavGrid grid, Islands islands,
            PathFinder finder, Clock clock)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));
            if (ReferenceEquals(finder, null))
                throw new ArgumentNullException(nameof(finder));
            if (ReferenceEquals(clock, null))
                throw new ArgumentNullException(nameof(clock));

            Id = id;
            Kind = kind;
            Position = position;
            PreviousPosition = position;
            _grid = grid;
            _clock = clock;
            _metersPerStep = clock.MetersPerStep(kind.WalkMetersPerSecond);
            _wayOutRings = grid.Metrics.CellCount(WayOutSearchMeters);
            _walk = new Walk(grid, finder);
            _wander = new Wander(kind, grid, islands, clock, _walk, _metersPerStep);
            HashRandom random = RandomOfTheStep();
            _wander.Begin(position, ref random);
        }

        /// <summary>Son numéro dans ce monde ; jamais 0.</summary>
        public int Id { get; }

        public VillagerKindDefinition Kind { get; }

        /// <summary>Sa position au dernier pas, en mètres sur le plan XZ.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Sa position au pas d'avant : la vue le montre entre les deux.</summary>
        public Vector2 PreviousPosition { get; private set; }

        /// <summary>
        /// Vrai quand il marche le long d'un trajet ; pas quand il sort d'une construction posée sur lui, ni quand il y
        /// est enfermé, immobile.
        /// </summary>
        public bool IsWalking => _walk.IsWalking && !_leaving && !_enclosed;

        /// <summary>Le trajet qu'il suit, dont la ligne tendue ; il ne vaut que pendant qu'il marche.</summary>
        public Path Path => _walk.Path;

        /// <summary>Joue un pas : sortir d'une construction posée sur lui d'abord, sinon flâner.</summary>
        internal void Step()
        {
            PreviousPosition = Position;
            Vector2 position = Position;
            HashRandom random = RandomOfTheStep();
            if (!LeaveCoveredGround(ref position, ref random))
                _wander.Step(ref position, ref random);
            Position = position;
        }

        private HashRandom RandomOfTheStep()
        {
            // Au-delà de deux milliards de pas (sept ans de jeu), le numéro fait le tour : le hasard reste le même.
            return new HashRandom(Id, unchecked((int)_clock.StepCount));
        }

        /// <summary>
        /// Vrai tant qu'il sort d'une construction posée sur lui, ou qu'il y est enfermé : il ne fait rien d'autre. Le
        /// chemin de sortie traverse la construction ; il ne suit pas le maillage.
        /// </summary>
        private bool LeaveCoveredGround(ref Vector2 position, ref HashRandom random)
        {
            if (!_leaving)
            {
                if (_grid.IsWalkable(_grid.CellOf(position)))
                {
                    _enclosed = false;
                    return false;
                }
                // Enfermé : il attend que le maillage change pour chercher de nouveau.
                if (_enclosed && _enclosedVersion == _grid.Version)
                    return true;
                _enclosed = !_grid.TryFindNearestWalkable(position, _wayOutRings, out Vector2Int free);
                _enclosedVersion = _grid.Version;
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
            _wander.Resume(position, ref random);
            return true;
        }
    }
}
