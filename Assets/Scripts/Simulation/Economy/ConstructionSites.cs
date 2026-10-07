using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Les chantiers et leur file (GDD, section 9) : chaque construction posée en fantôme en a un, chaque bâtiment qu'on
    /// déplace (décision 199), chaque sol qu'on trace ou qu'on efface (décision 209), dans l'ordre où l'ordre est venu,
    /// et les chantiers se servent dans cet ordre. Ce qu'un chantier bâtit (SiteWork) dit ce qui arrive quand on y
    /// travaille ; quand le dernier travail est fait, ses matériaux entrent dans ce qu'il bâtit (ils disparaissent du
    /// bilan), et une construction posée, bâtie, est mise en service par le monde à la fin du pas (décision 179). Un sol
    /// gommé avant d'être bâti quitte la file. Les événements d'un pas attendent la fin de leur couche (ARCHITECTURE,
    /// section 5).
    /// </summary>
    public sealed class ConstructionSites
    {
        private readonly List<ConstructionSite> _queue = new List<ConstructionSite>();
        private readonly List<ConstructionSite> _supplied = new List<ConstructionSite>();
        private readonly List<Structure> _finished = new List<Structure>();
        private readonly Structures _structures;
        private readonly Floors _floors;
        private readonly GoodsLedger _ledger;
        private readonly double _takeDownHours;
        private readonly double _putBackHours;
        private readonly double _floorHoursPerSquareMeter;
        private readonly double _floorEraseShare;

        public ConstructionSites(Structures structures, Floors floors, GoodsLedger ledger, SimulationSettings settings)
        {
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));
            if (ReferenceEquals(structures, null))
                throw new ArgumentNullException(nameof(structures));
            if (ReferenceEquals(floors, null))
                throw new ArgumentNullException(nameof(floors));
            if (ReferenceEquals(ledger, null))
                throw new ArgumentNullException(nameof(ledger));

            _structures = structures;
            _floors = floors;
            _ledger = ledger;
            _takeDownHours = settings.TakeDownHours;
            _putBackHours = settings.PutBackHours;
            _floorHoursPerSquareMeter = settings.FloorHoursPerSquareMeter;
            _floorEraseShare = settings.FloorEraseShare;
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
            // Un sol en chantier n'a pas de construction : null ne désigne aucun chantier.
            if (ReferenceEquals(structure, null))
                throw new ArgumentNullException(nameof(structure));
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
            _queue.Add(new ConstructionSite(new StructureWork(structure, _structures)));
        }

        /// <summary>
        /// On déplace ce bâtiment bâti, dont la nouvelle place est retenue : son petit chantier s'ouvre, au bout de la file.
        /// Seul le monde l'appelle.
        /// </summary>
        internal void TrackMove(Structure structure)
        {
            if (structure.State != StructureState.Built || !structure.MovingTo.HasValue || SiteOf(structure) != null)
                return;
            _queue.Add(new ConstructionSite(new MoveWork(structure, _structures.Moves, _takeDownHours, _putBackHours)));
        }

        /// <summary>
        /// Un sol vient d'être tracé, ou un sol bâti gommé : son chantier s'ouvre, au bout de la file ; effacer demande
        /// une part du travail de tracer. Seul le monde l'appelle.
        /// </summary>
        internal void TrackFloor(FloorPatch patch)
        {
            double hours = _floorHoursPerSquareMeter * (patch.IsErase ? _floorEraseShare : 1d);
            _queue.Add(new ConstructionSite(new FloorWork(patch, _floors, hours)));
        }

        /// <summary>
        /// La gomme a vidé ce sol en fantôme : son chantier quitte la file, et ses ouvriers en partent. Seul le monde
        /// l'appelle.
        /// </summary>
        internal void Drop(FloorPatch patch)
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                if (!ReferenceEquals(_queue[i].Floor, patch))
                    continue;
                _queue[i].MarkFinished();
                _queue.RemoveAt(i);
                return;
            }
        }

        /// <summary>Des matériaux en route arrivent sur le chantier.</summary>
        internal void Receive(ConstructionSite site, GoodDefinition good, int amount)
        {
            site.Receive(good, amount);
            if (!_supplied.Contains(site))
                _supplied.Add(site);
        }

        /// <summary>
        /// Ajoute ce travail au chantier ; vrai s'il est fini (il l'était peut-être déjà). Fini, il quitte la file, ses
        /// matériaux entrent dans ce qu'il bâtit, et une construction posée attend sa mise en service.
        /// </summary>
        internal bool Work(ConstructionSite site, double hours)
        {
            if (site.IsFinished)
                return true;
            bool done = site.AddWork(hours);
            site.Work.Progress(site.WorkDone);
            if (!done)
                return false;
            _queue.Remove(site);
            site.MarkFinished();
            for (int line = 0; line < site.LineCount; line++)
            {
                if (site.DeliveredAt(line) > 0)
                    _ledger.Destroyed(site.GoodAt(line), site.DeliveredAt(line), GoodsCause.Construction);
            }
            site.Work.Finish();
            if (site.Work.Commissions)
                _finished.Add(site.Structure);
            return true;
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
