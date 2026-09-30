using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// La tâche en cours d'un habitant (ARCHITECTURE, section 7) : une suite d'étapes, chacune avec son issue d'échec.
    /// Porter : aller à l'entrepôt, prendre, aller au chantier, déposer. Bâtir : aller au chantier, y travailler. Quand le
    /// travail s'arrête, il finit sa marche et dépose ce qu'il porte, mais n'entame pas l'étape suivante ; le travail sur
    /// un chantier s'arrête tout de suite (décisions 140 et 149). Échec : le but n'est plus joignable ; ce qu'il porte
    /// retourne à la ville et ses réservations sont rendues. La marche est celle de l'habitant.
    /// </summary>
    internal sealed class Chore
    {
        private enum Stage
        {
            None,
            ToSource,
            ToSite,
            Working
        }

        private readonly VillagerServices _services;
        private readonly VillagerStatistics _statistics;
        private readonly Inventory _load;
        private readonly Walk _walk;
        private readonly float _metersPerStep;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private Delivery _delivery;
        private BuildShift _shift;
        private TaskKindDefinition _task;
        private Stage _stage;

        public Chore(VillagerServices services, VillagerStatistics statistics, Inventory load, Walk walk,
            float metersPerStep)
        {
            if (ReferenceEquals(services, null))
                throw new ArgumentNullException(nameof(services));

            _services = services;
            _statistics = statistics;
            _load = load;
            _walk = walk;
            _metersPerStep = metersPerStep;
        }

        /// <summary>Vrai tant qu'une tâche est en cours.</summary>
        public bool IsActive => _stage != Stage.None;

        /// <summary>Vrai pendant qu'il travaille sur un chantier, sur place.</summary>
        public bool IsBuilding => _stage == Stage.Working;

        /// <summary>
        /// Prend une tâche de cette sorte, pour un habitant sur cet îlot, à ce point, et part ; faux s'il n'y a rien de
        /// cette sorte à faire qu'il puisse rejoindre.
        /// </summary>
        public bool TryTake(TaskKindDefinition task, int island, Vector2 position)
        {
            TaskOffers offers = _services.Offers;
            if (task.Work == TaskWork.Carry && offers.TryTakeDelivery(island, position, _load.Capacity, out Delivery delivery))
            {
                _delivery = delivery;
                return Begin(task, Stage.ToSource, position, delivery.Source);
            }
            if (task.Work == TaskWork.Build && offers.TryTakeBuilding(island, out BuildShift shift))
            {
                _shift = shift;
                return Begin(task, Stage.ToSite, position, shift.Site.Structure);
            }
            return false;
        }

        /// <summary>
        /// Un pas de la tâche. <paramref name="built"/> est la construction qu'il vient de finir, s'il en finit une.
        /// </summary>
        public ChoreOutcome Step(ref Vector2 position, bool workTime, out Structure built)
        {
            built = null;
            if (_stage == Stage.Working)
            {
                if (!workTime)
                    return End(ChoreOutcome.Done);
                double hours = _statistics.WorkRateFor(_task) * _services.Clock.HoursPerStep;
                if (!_services.Offers.Work(_shift, hours))
                    return ChoreOutcome.Continuing;
                built = _shift.Site.Structure;
                return End(ChoreOutcome.Done);
            }

            WalkOutcome outcome = _walk.IsWalking ? _walk.Advance(ref position, _metersPerStep) : WalkOutcome.Lost;
            if (outcome == WalkOutcome.Walking)
                return ChoreOutcome.Continuing;
            if (outcome == WalkOutcome.Lost)
                return End(ChoreOutcome.Failed);
            return Arrive(position, workTime);
        }

        /// <summary>Il vient de sortir d'une construction posée sur lui : il repart d'ici vers le même but.</summary>
        public ChoreOutcome Resume(Vector2 position)
        {
            if (_stage != Stage.ToSource && _stage != Stage.ToSite)
                return ChoreOutcome.Continuing;
            return _walk.TryResume(position) ? ChoreOutcome.Continuing : End(ChoreOutcome.Failed);
        }

        /// <summary>
        /// La tâche s'arrête là : sa marche aussi, ce qui est encore réservé est rendu, et ce qu'il porte retourne à la
        /// ville.
        /// </summary>
        public void Stop()
        {
            if (_stage == Stage.None)
                return;
            _walk.Stop();
            End(ChoreOutcome.Done);
        }

        private bool Begin(TaskKindDefinition task, Stage stage, Vector2 position, Structure target)
        {
            _task = task;
            _stage = stage;
            if (WalkTo(position, target))
                return true;
            End(ChoreOutcome.Failed);
            return false;
        }

        /// <summary>Il est au bout de sa marche : l'étape suivante, si le travail n'est pas fini.</summary>
        private ChoreOutcome Arrive(Vector2 position, bool workTime)
        {
            if (_stage == Stage.ToSite && _delivery != null)
            {
                // Il dépose ce qu'il porte, même le travail fini : c'est la fin de son étape.
                _services.Offers.DropOff(_delivery, _load);
                _delivery = null;
                return End(ChoreOutcome.Done);
            }
            if (!workTime)
                return End(ChoreOutcome.Done);
            if (_stage == Stage.ToSite)
            {
                _stage = Stage.Working;
                return ChoreOutcome.Continuing;
            }
            _services.Offers.PickUp(_delivery, _load);
            _stage = Stage.ToSite;
            return WalkTo(position, _delivery.Site.Structure) ? ChoreOutcome.Continuing : End(ChoreOutcome.Failed);
        }

        /// <summary>Part vers les abords de cette construction ; faux s'ils ne sont pas joignables d'ici.</summary>
        private bool WalkTo(Vector2 position, Structure structure)
        {
            _services.Surroundings.Collect(structure, _cells);
            return _walk.TryStart(position, _cells);
        }

        private ChoreOutcome End(ChoreOutcome outcome)
        {
            if (_delivery != null)
                _services.Offers.Cancel(_delivery, _load);
            if (_shift != null)
                _services.Offers.Leave(_shift);
            _delivery = null;
            _shift = null;
            _task = null;
            _stage = Stage.None;
            return outcome;
        }
    }
}
