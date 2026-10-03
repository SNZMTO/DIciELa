using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// La tâche en cours d'un habitant (ARCHITECTURE, section 7) : marcher jusqu'où mène son geste, y faire ce que le geste
    /// dit, travailler sur place, et ainsi de suite. Les gestes : porter, bâtir, défricher, conduire, un par valeur de
    /// TaskWork ; un geste peut ralentir la marche (une charrette tirée), et suit chacun de ses pas.
    /// Quand le travail s'arrête, il finit sa marche et dépose ce qu'il porte, mais n'entame pas l'étape suivante ; le
    /// travail sur place s'arrête tout de suite (décisions 140 et 149). Échec : le but n'est plus joignable ; ce qu'il
    /// porte retourne à la ville et ses réservations sont rendues. La marche est celle de l'habitant.
    /// </summary>
    internal sealed class Chore
    {
        private readonly VillagerServices _services;
        private readonly VillagerStatistics _statistics;
        private readonly Walk _walk;
        private readonly float _metersPerStep;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private readonly Gesture[] _gestures;
        private Gesture _gesture;
        private TaskKindDefinition _task;
        private bool _working;

        public Chore(VillagerServices services, VillagerStatistics statistics, Inventory load, Walk walk,
            float metersPerStep)
        {
            if (ReferenceEquals(services, null))
                throw new ArgumentNullException(nameof(services));

            _services = services;
            _statistics = statistics;
            _walk = walk;
            _metersPerStep = metersPerStep;
            // Un geste par valeur de TaskWork, à sa place : Carry, Build, Clear, Drive.
            _gestures = new Gesture[]
            {
                new CarryGesture(services.Offers, load),
                new BuildGesture(services.Offers),
                new ClearGesture(services.Clearings, load),
                new DriveGesture(services.Carts)
            };
        }

        /// <summary>Vrai tant qu'une tâche est en cours.</summary>
        public bool IsActive => _gesture != null;

        /// <summary>Vrai pendant qu'il travaille sur un chantier, sur place.</summary>
        public bool IsBuilding => _working && _task.Work == TaskWork.Build;

        /// <summary>
        /// Prend une tâche de cette sorte, pour un habitant sur cet îlot, à ce point, et part ; faux s'il n'y a rien de
        /// cette sorte à faire qu'il puisse rejoindre.
        /// </summary>
        public bool TryTake(TaskKindDefinition task, int island, Vector2 position)
        {
            Gesture gesture = _gestures[(int)task.Work];
            if (!gesture.TryTake(island, position))
                return false;
            _gesture = gesture;
            _task = task;
            _working = false;
            if (WalkTo(position))
                return true;
            End(ChoreOutcome.Failed);
            return false;
        }

        /// <summary>Un pas de la tâche.</summary>
        public ChoreOutcome Step(ref Vector2 position, bool workTime)
        {
            if (_working)
            {
                if (!workTime)
                    return End(ChoreOutcome.Done);
                double hours = _statistics.WorkRateFor(_task) * _services.Clock.HoursPerStep;
                return Follow(_gesture.Work(hours), position);
            }

            WalkOutcome outcome = _walk.IsWalking
                ? _walk.Advance(ref position, _metersPerStep * _gesture.SpeedFactor)
                : WalkOutcome.Lost;
            if (outcome == WalkOutcome.Lost)
                return End(ChoreOutcome.Failed);
            _gesture.Walked(position);
            if (outcome == WalkOutcome.Walking)
                return ChoreOutcome.Continuing;
            return Follow(_gesture.Arrive(position, workTime), position);
        }

        /// <summary>
        /// La tâche s'arrête là : sa marche aussi, ce qui est encore réservé est rendu, et ce qu'il porte retourne à la
        /// ville.
        /// </summary>
        public void Stop()
        {
            if (_gesture == null)
                return;
            _walk.Stop();
            End(ChoreOutcome.Done);
        }

        /// <summary>Fait ce que le geste demande : marcher vers son nouveau but, travailler sur place, ou finir.</summary>
        private ChoreOutcome Follow(GestureStep step, Vector2 position)
        {
            switch (step)
            {
                case GestureStep.Walk:
                    _working = false;
                    return WalkTo(position) ? ChoreOutcome.Continuing : End(ChoreOutcome.Failed);
                case GestureStep.Work:
                    _working = true;
                    return ChoreOutcome.Continuing;
                default:
                    return End(ChoreOutcome.Done);
            }
        }

        /// <summary>Part vers le but du geste, ou vers ses abords ; faux s'il n'est pas joignable d'ici.</summary>
        private bool WalkTo(Vector2 position)
        {
            ChoreGoal goal = _gesture.Goal;
            if (goal.IsCell)
                return _walk.TryStart(position, goal.Cell);
            _services.Surroundings.Collect(goal.Footprint, goal.Occupant, _cells);
            return _walk.TryStart(position, _cells);
        }

        private ChoreOutcome End(ChoreOutcome outcome)
        {
            _gesture.Stop();
            _gesture = null;
            _task = null;
            _working = false;
            return outcome;
        }
    }
}
