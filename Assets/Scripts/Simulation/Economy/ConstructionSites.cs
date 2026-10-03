using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Les chantiers et leur file (GDD, section 9) : chaque construction posée en fantôme en a un, dans l'ordre où elle
    /// a été posée, et les chantiers se servent dans cet ordre. Quand le dernier travail est fait, ses matériaux entrent
    /// dans la construction (ils disparaissent du bilan) et elle devient bâtie ; le monde la met en service à la fin du
    /// pas (décision 179). Un bâtiment qu'on déplace a son petit chantier (décision 199) : le premier travail le défait,
    /// défait il saute à sa nouvelle place, remonté il sert de nouveau, sans nouvelle mise en service. Les événements d'un pas attendent la fin de leur couche (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class ConstructionSites
    {
        private readonly List<ConstructionSite> _queue = new List<ConstructionSite>();
        private readonly List<ConstructionSite> _supplied = new List<ConstructionSite>();
        private readonly List<Structure> _finished = new List<Structure>();
        private readonly Structures _structures;
        private readonly GoodsLedger _ledger;
        private readonly double _takeDownHours;
        private readonly double _putBackHours;

        public ConstructionSites(Structures structures, GoodsLedger ledger, SimulationSettings settings)
        {
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));
            if (ReferenceEquals(structures, null))
                throw new ArgumentNullException(nameof(structures));
            if (ReferenceEquals(ledger, null))
                throw new ArgumentNullException(nameof(ledger));

            _structures = structures;
            _ledger = ledger;
            _takeDownHours = settings.TakeDownHours;
            _putBackHours = settings.PutBackHours;
        }

        /// <summary>Levé quand des matériaux arrivent sur un chantier.</summary>
        public event Action<ConstructionSite> GoodsReceived;

        /// <summary>La file des chantiers, dans l'ordre où ils se servent.</summary>
        public IReadOnlyList<ConstructionSite> Queue => _queue;

        /// <summary>
        /// Les constructions dont le chantier a fini depuis la dernière mise en service, dans l'ordre où elles ont fini :
        /// le monde les met en service à la fin du pas, puis les oublie.
        /// </summary>
        internal IReadOnlyList<Structure> Finished => _finished;

        /// <summary>Le chantier de cette construction ; null si elle n'en a pas (bâtie, ou dressée dès la pose).</summary>
        public ConstructionSite SiteOf(Structure structure)
        {
            foreach (ConstructionSite site in _queue)
            {
                if (ReferenceEquals(site.Structure, structure))
                    return site;
            }
            return null;
        }

        /// <summary>
        /// Une construction vient d'être posée : si elle attend d'être bâtie, son chantier s'ouvre, au bout de la file.
        /// Seul le monde l'appelle.
        /// </summary>
        internal void Track(Structure structure)
        {
            if (structure.State != StructureState.Planned || SiteOf(structure) != null)
                return;
            _queue.Add(new ConstructionSite(structure));
        }

        /// <summary>
        /// On déplace ce bâtiment bâti, dont la nouvelle place est retenue : son petit chantier s'ouvre, au bout de la file.
        /// Seul le monde l'appelle.
        /// </summary>
        internal void TrackMove(Structure structure)
        {
            if (structure.State != StructureState.Built || !structure.MovingTo.HasValue || SiteOf(structure) != null)
                return;
            _queue.Add(ConstructionSite.ForMove(structure, _takeDownHours, _putBackHours));
        }

        /// <summary>Des matériaux en route arrivent sur le chantier.</summary>
        internal void Receive(ConstructionSite site, GoodDefinition good, int amount)
        {
            site.Receive(good, amount);
            if (!_supplied.Contains(site))
                _supplied.Add(site);
        }

        /// <summary>Ajoute ce travail au chantier ; vrai s'il est fini (il l'était peut-être déjà).</summary>
        internal bool Work(ConstructionSite site, double hours)
        {
            if (site.IsFinished)
                return true;
            if (site.IsMove)
                return WorkOnMove(site, hours);
            if (!site.AddWork(hours))
                return false;
            Finish(site);
            return true;
        }

        // Le premier travail défait le bâtiment, qui ne sert plus ; défait, il saute à sa nouvelle place ; remonté, il sert
        // de nouveau, et son petit chantier quitte la file.
        private bool WorkOnMove(ConstructionSite site, double hours)
        {
            Structure structure = site.Structure;
            if (structure.State == StructureState.Built)
                _structures.Moves.TakeDown(structure);
            bool done = site.AddWork(hours);
            if (!site.HasLanded && site.WorkDone >= site.TakeDownHours)
            {
                _structures.Moves.Land(structure);
                site.MarkLanded();
            }
            if (!done)
                return false;
            _queue.Remove(site);
            site.MarkFinished();
            _structures.Moves.Finish(structure);
            return true;
        }

        private void Finish(ConstructionSite site)
        {
            _queue.Remove(site);
            site.MarkFinished();
            for (int line = 0; line < site.LineCount; line++)
            {
                if (site.DeliveredAt(line) > 0)
                    _ledger.Destroyed(site.GoodAt(line), site.DeliveredAt(line), GoodsCause.Construction);
            }
            _structures.Finish(site.Structure);
            _finished.Add(site.Structure);
        }

        /// <summary>Le monde a mis en service les constructions finies : il les oublie. Seul le monde l'appelle.</summary>
        internal void ForgetFinished()
        {
            _finished.Clear();
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide les files. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _supplied.Count; i++)
                    GoodsReceived?.Invoke(_supplied[i]);
            }
            finally
            {
                _supplied.Clear();
            }
        }
    }
}
