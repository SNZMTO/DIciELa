using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Flâner (GDD, section 7) : sans poste, un habitant va à un point libre tiré autour de l'endroit où il s'est mis à
    /// flâner, au plus au rayon que dit sa sorte et pas tout près de lui, s'y arrête un moment, puis recommence. Tiré
    /// autour de ce point et non de lui-même, il ne dérive pas à travers la carte. Un point sans issue, ou devenu
    /// injoignable en chemin, n'est qu'une pause de plus ; si l'endroit même où il flânait est pris ou coupé de lui, il
    /// se remet à flâner autour d'où il est (décision 89). Les tirages viennent du hasard de son pas (voir
    /// <see cref="Villager"/>).
    /// </summary>
    internal sealed class Wander
    {
        private readonly NavGrid _grid;
        private readonly Islands _islands;
        private readonly Clock _clock;
        private readonly Walk _walk;
        private readonly float _metersPerStep;
        private readonly int _radiusCells;
        private readonly float _minHopMeters;
        private readonly double _pauseMinHours;
        private readonly double _pauseMaxHours;
        // Les essais pour trouver un point à chaque départ ; tous ratés, c'est une pause de plus.
        private readonly int _draws;
        private Vector2 _anchor;
        private long _pauseLeft;

        public Wander(VillagerKindDefinition kind, NavGrid grid, Islands islands, Clock clock, Walk walk,
            float metersPerStep, int draws)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));
            if (ReferenceEquals(clock, null))
                throw new ArgumentNullException(nameof(clock));
            if (ReferenceEquals(walk, null))
                throw new ArgumentNullException(nameof(walk));

            _grid = grid;
            _islands = islands;
            _clock = clock;
            _walk = walk;
            _metersPerStep = metersPerStep;
            _radiusCells = Math.Max(1, (int)(kind.WanderRadiusMeters * grid.Metrics.CellsPerMeter));
            _minHopMeters = kind.WanderMinHopMeters;
            _pauseMinHours = kind.WanderPauseMinHours;
            _pauseMaxHours = kind.WanderPauseMaxHours;
            _draws = Math.Max(1, draws);
        }

        /// <summary>Se met à flâner autour de ce point, en commençant par une pause.</summary>
        public void Begin(Vector2 anchor, ref HashRandom random)
        {
            _anchor = anchor;
            _walk.Stop();
            Pause(ref random);
        }

        /// <summary>Un pas de flânerie : marcher vers le point tiré, attendre, ou tirer le suivant.</summary>
        public void Step(ref Vector2 position, ref HashRandom random)
        {
            if (_walk.IsWalking)
            {
                if (_walk.Advance(ref position, _metersPerStep) != WalkOutcome.Walking)
                    Pause(ref random);
                return;
            }
            if (_pauseLeft > 0)
            {
                _pauseLeft--;
                return;
            }
            // Une maille prise est sur l'îlot 0 : l'endroit où il flânait n'est plus sur son îlot s'il est pris, ou si une
            // construction l'en a coupé.
            if (_islands.IslandOf(_grid.CellOf(_anchor)) != _islands.IslandOf(_grid.CellOf(position)))
                _anchor = position;
            if (!TryDraw(position, ref random, out Vector2Int target) || !_walk.TryStart(position, target))
                Pause(ref random);
        }

        /// <summary>
        /// Il a quitté son trajet pour sortir d'une construction posée sur lui : il repart d'ici vers le même point, ou,
        /// s'il n'est plus joignable, fait une pause.
        /// </summary>
        public void Resume(Vector2 position, ref HashRandom random)
        {
            if (_walk.IsWalking && !_walk.TryResume(position))
                Pause(ref random);
        }

        private void Pause(ref HashRandom random)
        {
            double hours = _pauseMinHours + random.Unit() * (_pauseMaxHours - _pauseMinHours);
            _pauseLeft = _clock.StepsIn(hours);
        }

        /// <summary>
        /// Un point libre du disque autour de l'ancre, sur l'îlot où il se trouve (donc joignable), et pas tout près de
        /// lui ; faux si aucun essai ne tombe juste.
        /// </summary>
        private bool TryDraw(Vector2 position, ref HashRandom random, out Vector2Int target)
        {
            Vector2Int anchor = _grid.CellOf(_anchor);
            int island = _islands.IslandOf(_grid.CellOf(position));
            int span = 2 * _radiusCells + 1;
            for (int draw = 0; draw < _draws; draw++)
            {
                var offset = new Vector2Int(random.Range(span) - _radiusCells, random.Range(span) - _radiusCells);
                target = anchor + offset;
                if (offset.sqrMagnitude > _radiusCells * _radiusCells || !_grid.Contains(target))
                    continue;
                // Une maille prise est sur l'îlot 0 ; la sienne est libre, sur un vrai îlot.
                if (_islands.IslandOf(target) != island)
                    continue;
                if (Vector2.Distance(_grid.CellCenter(target), position) >= _minHopMeters)
                    return true;
            }
            target = default;
            return false;
        }
    }
}
