using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Le travail d'un habitant (GDD, section 7) : le poste qu'il tient. Aux heures de travail, il prend la première
    /// tâche que son poste permet, dans l'ordre du poste (le poste « à tout faire » : porter, bâtir, puis défricher) ;
    /// sans rien à faire, et hors des heures de travail, il flâne près de son poste. Après une tâche qui échoue, il flâne
    /// une pause avant d'en reprendre une : il ne reprend pas en boucle celle qui vient d'échouer. La flânerie est celle
    /// de l'habitant.
    /// </summary>
    internal sealed class Job
    {
        private readonly VillagerServices _services;
        private readonly Wander _wander;
        private readonly Chore _chore;
        private readonly double _failurePauseHours;
        private bool _idle;
        private long _nextTryStep;

        public Job(VillagerServices services, VillagerKindDefinition kind, VillagerStatistics statistics,
            Inventory load, Walk walk, Wander wander, float metersPerStep)
        {
            if (ReferenceEquals(services, null))
                throw new ArgumentNullException(nameof(services));

            _services = services;
            _wander = wander;
            _chore = new Chore(services, statistics, load, walk, metersPerStep);
            _failurePauseHours = kind.WanderPauseMaxHours;
        }

        /// <summary>Le poste qu'il tient ; null s'il n'en tient aucun.</summary>
        public WorkSlot Slot { get; private set; }

        /// <summary>Vrai tant qu'une tâche est en cours.</summary>
        public bool HasTask => _chore.IsActive;

        /// <summary>Vrai pendant qu'il travaille sur un chantier, sur place.</summary>
        public bool IsBuilding => _chore.IsBuilding;

        /// <summary>Il prend ce poste : il laisse sa tâche, et commencera par regarder ce qu'il y a à faire.</summary>
        public void Take(WorkSlot slot)
        {
            Interrupt();
            Slot = slot;
        }

        /// <summary>Il quitte son poste et sa tâche.</summary>
        public void Quit()
        {
            Interrupt();
            Slot = null;
        }

        /// <summary>
        /// Il laisse sa tâche (il va dormir, il change de poste) : ce qu'il porte retourne à la ville, ses réservations
        /// sont rendues ; il garde son poste.
        /// </summary>
        public void Interrupt()
        {
            _chore.Stop();
            _idle = false;
        }

        /// <summary>
        /// Un pas de travail ; faux s'il ne tient aucun poste (il flâne alors où il veut). Sinon : sa tâche, une nouvelle
        /// tâche, ou flâner près de son poste.
        /// </summary>
        public bool Step(ref Vector2 position, ref HashRandom random)
        {
            if (Slot == null)
                return false;
            bool workTime = _services.Clock.Phase == DayPhase.Work;
            if (_chore.IsActive)
            {
                Follow(_chore.Step(ref position, workTime));
                return true;
            }
            if (workTime && _services.Clock.StepCount >= _nextTryStep && TryTakeTask(position))
                return true;

            if (!_idle)
            {
                _wander.Begin(_services.Surroundings.NearestTo(Slot.Workplace, position), ref random);
                _idle = true;
            }
            _wander.Step(ref position, ref random);
            return true;
        }

        /// <summary>
        /// Il vient de sortir d'une construction posée sur lui (sa tâche laissée en y entrant) : il reprend sa flânerie
        /// près du poste. Faux s'il ne tient aucun poste.
        /// </summary>
        public bool Resume(Vector2 position, ref HashRandom random)
        {
            if (Slot == null)
                return false;
            if (_idle)
                _wander.Resume(position, ref random);
            return true;
        }

        private bool TryTakeTask(Vector2 position)
        {
            int island = _services.Islands.IslandOf(_services.Grid.CellOf(position));
            if (island == 0)
                return false;
            IReadOnlyList<TaskKindDefinition> tasks = Slot.Definition.Tasks;
            for (int i = 0; i < tasks.Count; i++)
            {
                if (_chore.TryTake(tasks[i], island, position))
                {
                    _idle = false;
                    return true;
                }
            }
            return false;
        }

        private void Follow(ChoreOutcome outcome)
        {
            if (outcome == ChoreOutcome.Failed)
                _nextTryStep = _services.Clock.StepCount + _services.Clock.StepsIn(_failurePauseHours);
        }
    }
}
