using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Un habitant (ARCHITECTURE, section 4) : sa sorte, son apparence, ses statistiques, sa place, ce qu'il porte et ce
    /// qu'il fait. Sa place est sa position, en mètres sur le plan XZ ; il garde aussi celle du pas d'avant, pour que la
    /// vue le montre entre les deux. Avec un poste, il y travaille ; sans poste, il flâne ; envoyé dormir, il laisse sa
    /// tâche (il garde son poste), va à son lit, s'y couche et n'en bouge plus jusqu'à ce qu'on le réveille. Une
    /// construction posée sur lui ne l'arrête pas : il laisse sa tâche, en sort à pied, puis reprend sa marche vers son lit
    /// ou son travail. Chaque pas tire son hasard de son numéro et du numéro du pas : les mêmes ordres rejouent la même
    /// partie.
    /// </summary>
    public sealed class Villager
    {
        private readonly VillagerServices _services;
        private readonly Walk _walk;
        private readonly Wander _wander;
        private readonly Bedtime _bedtime;
        private readonly GroundExit _exit;
        private readonly Job _job;

        internal Villager(int id, VillagerKindDefinition kind, Appearance appearance, Vector2 position,
            VillagerServices services)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));
            if (ReferenceEquals(appearance, null))
                throw new ArgumentNullException(nameof(appearance));
            if (ReferenceEquals(services, null))
                throw new ArgumentNullException(nameof(services));

            Id = id;
            Kind = kind;
            Appearance = appearance;
            Position = position;
            PreviousPosition = position;
            _services = services;
            Statistics = new VillagerStatistics(services.Statistics, kind.StartingStatistic);
            Load = new Inventory(kind.CarryCapacity);
            float metersPerStep = services.Clock.MetersPerStep(kind.WalkMetersPerSecond);
            _walk = new Walk(services.Grid, services.Finder);
            _wander = new Wander(kind, services.Grid, services.Islands, services.Clock, _walk, metersPerStep,
                services.Settings.WanderDraws);
            _bedtime = new Bedtime(_walk, metersPerStep);
            _exit = new GroundExit(services.Grid, services.Islands, metersPerStep);
            _job = new Job(services, kind, Statistics, Load, _walk, _wander, metersPerStep);
            HashRandom random = RandomOfTheStep();
            _wander.Begin(position, ref random);
        }

        /// <summary>Son numéro dans ce monde ; jamais 0.</summary>
        public int Id { get; }

        public VillagerKindDefinition Kind { get; }

        /// <summary>Son corps, sa couleur et ses pièces : la vue l'habille ainsi.</summary>
        public Appearance Appearance { get; }

        /// <summary>Ses statistiques : sa valeur pour chaque tâche en vient.</summary>
        public VillagerStatistics Statistics { get; }

        /// <summary>Ce qu'il porte : sa charge d'un voyage.</summary>
        public Inventory Load { get; }

        /// <summary>Sa position au dernier pas, en mètres sur le plan XZ.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Sa position au pas d'avant : la vue le montre entre les deux.</summary>
        public Vector2 PreviousPosition { get; private set; }

        /// <summary>
        /// Vrai quand il marche le long d'un trajet ; pas quand il sort d'une construction posée sur lui, ni quand il y
        /// est enfermé, immobile, ni quand il travaille sur place.
        /// </summary>
        public bool IsWalking => _walk.IsWalking && !_exit.IsLeaving && !_exit.IsEnclosed;

        /// <summary>Le trajet qu'il suit, dont la ligne tendue ; il ne vaut que pendant qu'il marche.</summary>
        public Path Path => _walk.Path;

        /// <summary>Le poste qu'il tient ; null s'il n'en tient aucun.</summary>
        public WorkSlot Slot => _job.Slot;

        /// <summary>Vrai pendant qu'il travaille sur un chantier, sur place.</summary>
        public bool IsBuilding => _job.IsBuilding;

        /// <summary>Vrai pendant qu'il marche vers son lit.</summary>
        public bool IsGoingToBed => _bedtime.IsGoing;

        /// <summary>Vrai quand il dort, couché dans son logis, devant sa porte pour la simulation.</summary>
        public bool IsAsleep => _bedtime.IsAsleep;

        /// <summary>
        /// Joue un pas : endormi, rien ; sinon, sortir d'une construction posée sur lui d'abord, puis aller dormir,
        /// travailler à son poste, ou flâner.
        /// </summary>
        internal void Step()
        {
            PreviousPosition = Position;
            if (_bedtime.IsAsleep)
                return;

            Vector2 position = Position;
            HashRandom random = RandomOfTheStep();
            if (_exit.Step(ref position, out bool justLeft))
            {
                // Poussé hors de sa place, il laisse sa tâche, comme pour aller dormir : ce qu'il porte retourne à la
                // ville et ses réservations sont rendues ; il en reprendra une une fois dehors (décision 156).
                if (_job.HasTask)
                    _job.Interrupt();
                if (justLeft)
                    ResumeAfterLeaving(position, ref random);
            }
            else if (_bedtime.IsGoing)
            {
                if (!_bedtime.Step(ref position))
                    _wander.Begin(position, ref random);
            }
            else if (!_job.Step(ref position, ref random))
                _wander.Step(ref position, ref random);
            Position = position;
        }

        /// <summary>
        /// L'envoie dormir dans ce lit, le milieu de cette maille. Faux s'il ne peut pas l'atteindre d'où il est (il est
        /// encore sur une construction posée sur lui, ou son lit est sur un autre îlot) : rien ne change alors. Sinon, il
        /// laisse sa tâche (ce qu'il porte retourne à la ville) et garde son poste. S'il finit de sortir d'une
        /// construction, il en sort d'abord, puis va dormir. Seuls les habitants l'appellent, qui ont vérifié qu'il est
        /// debout.
        /// </summary>
        internal bool TryGoToBed(Vector2Int bed)
        {
            // Une maille prise est sur l'îlot 0 : sur un sol couvert, rien ne se rejoint.
            int island = _services.Islands.IslandOf(_services.Grid.CellOf(Position));
            if (island == 0 || _services.Islands.IslandOf(bed) != island)
                return false;
            _job.Interrupt();
            return _bedtime.TryStart(Position, bed);
        }

        /// <summary>
        /// Le réveille, ou l'arrête s'il allait dormir : il se remet à flâner devant sa porte, même s'il n'y était pas
        /// encore arrivé (GDD, section 6), ou reprend son poste. Seuls les habitants l'appellent, qui ont vérifié qu'il
        /// dort ou y va.
        /// </summary>
        internal void WakeUp()
        {
            Vector2 door = _services.Grid.CellCenter(_bedtime.Bed);
            _bedtime.Stop();
            _job.Interrupt();
            HashRandom random = RandomOfTheStep();
            _wander.Begin(door, ref random);
        }

        /// <summary>Il prend ce poste. Seule la table des postes l'appelle.</summary>
        internal void TakeSlot(WorkSlot slot)
        {
            _job.Take(slot);
        }

        /// <summary>Il quitte son poste : debout, il se remet à flâner autour d'où il est. Seule la table des postes l'appelle.</summary>
        internal void LeaveSlot()
        {
            _job.Quit();
            if (_bedtime.IsGoing || _bedtime.IsAsleep)
                return;
            HashRandom random = RandomOfTheStep();
            _wander.Begin(Position, ref random);
        }

        private HashRandom RandomOfTheStep()
        {
            // Au-delà de deux milliards de pas (sept ans de jeu), le numéro fait le tour : le hasard reste le même.
            return new HashRandom(Id, unchecked((int)_services.Clock.StepCount));
        }

        /// <summary>
        /// Il vient de sortir d'une construction posée sur lui : il reprend d'ici sa marche vers son lit, son travail ou
        /// sa flânerie.
        /// </summary>
        private void ResumeAfterLeaving(Vector2 position, ref HashRandom random)
        {
            if (_bedtime.IsGoing)
            {
                if (!_bedtime.Resume(position))
                    _wander.Begin(position, ref random);
            }
            else if (!_job.Resume(position, ref random))
                _wander.Resume(position, ref random);
        }
    }
}
