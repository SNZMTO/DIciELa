using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la fiche d'un lieu (GDD, section 16 ; décisions 141, 152 et 172) : toucher un lieu qui a un
    /// poste ou un stock, ou le logis de la Chimère, ouvre sa fiche, qui remplace celle qui était ouverte ; toucher autre
    /// chose, ou rien, la ferme, comme la croix et Échap. La fiche dit le nom du lieu, ce qu'il tient pour la ville, ses
    /// postes et qui les tient, chacun par son nom (décision 189), et ses boutons (LocationSheetActions) : la Chimère au poste,
    /// et au lit dans son logis. Il écoute les stocks, la table des postes et le sommeil : la fiche ne photographie jamais.
    /// </summary>
    public sealed class LocationSheetPresenter : IDisposable
    {
        private readonly ILocationSheetView _view;
        private readonly World _world;
        private readonly TextTable _texts;
        private readonly List<LocationSheetLine> _lines = new List<LocationSheetLine>();
        private readonly List<LocationSheetSlot> _slots = new List<LocationSheetSlot>();
        private readonly LocationSheetActions _actions;

        public LocationSheetPresenter(ILocationSheetView view, World world, TextTable texts)
        {
            if (view == null || ReferenceEquals(world, null) || ReferenceEquals(texts, null))
                throw new ArgumentException("La fiche d'un lieu a besoin de sa vue, du monde et des textes.");

            _view = view;
            _world = world;
            _texts = texts;
            _actions = new LocationSheetActions(world, texts);
            _view.ActionClicked += OnActionClicked;
            _view.CloseClicked += OnCloseClicked;
            _world.Warehouses.StockChanged += OnStockChanged;
            _world.WorkSlots.VillagerAssigned += OnSlotChanged;
            _world.WorkSlots.VillagerUnassigned += OnSlotChanged;
            _world.Villagers.VillagerWentToBed += OnSleepChanged;
            _world.Villagers.VillagerFellAsleep += OnSleepChanged;
            _world.Villagers.VillagerWokeUp += OnSleepChanged;
            _view.ShowSheet(false);
        }

        /// <summary>Le lieu dont la fiche est ouverte ; null quand elle est fermée.</summary>
        public Structure Location { get; private set; }

        public void Dispose()
        {
            _view.ActionClicked -= OnActionClicked;
            _view.CloseClicked -= OnCloseClicked;
            _world.Warehouses.StockChanged -= OnStockChanged;
            _world.WorkSlots.VillagerAssigned -= OnSlotChanged;
            _world.WorkSlots.VillagerUnassigned -= OnSlotChanged;
            _world.Villagers.VillagerWentToBed -= OnSleepChanged;
            _world.Villagers.VillagerFellAsleep -= OnSleepChanged;
            _world.Villagers.VillagerWokeUp -= OnSleepChanged;
        }

        /// <summary>On a touché cette construction (null : rien) : un lieu qui a une fiche l'ouvre, le reste la ferme.</summary>
        public void Touch(Structure touched)
        {
            if (touched == null || !HasASheet(touched))
            {
                Close();
                return;
            }
            Location = touched;
            Refresh();
            _view.ShowSheet(true);
        }

        /// <summary>Ferme la fiche ; faux si elle l'était déjà.</summary>
        public bool Close()
        {
            if (Location == null)
                return false;
            Location = null;
            _view.ShowSheet(false);
            return true;
        }

        // Un lieu a une fiche s'il a un poste ou un stock, ou s'il est le logis de la Chimère (on l'y envoie dormir).
        private bool HasASheet(Structure location)
        {
            return _world.WorkSlots.IsWorkplace(location) || _world.Warehouses.StockOf(location) != null || IsHome(location);
        }

        private bool IsHome(Structure location)
        {
            return ReferenceEquals(location, _world.Structures.ChimeraHome);
        }

        private void Refresh()
        {
            if (Location == null)
                return;
            _view.ShowTitle(_texts.Get(Location.Definition.NameKey));

            _lines.Clear();
            Inventory stock = _world.Warehouses.StockOf(Location);
            for (int line = 0; stock != null && line < stock.LineCount; line++)
            {
                GoodDefinition good = stock.GoodAt(line);
                _lines.Add(new LocationSheetLine(good.Icon, _texts.Get(good.NameKey), stock.CountAt(line)));
            }
            _view.ShowStock(_lines);

            // Les postes du lieu, dans l'ordre où ils se sont ouverts : le sien d'abord, puis ceux ouverts pour qui arrive.
            _slots.Clear();
            WorkSlot first = null;
            IReadOnlyList<WorkSlot> all = _world.WorkSlots.All;
            for (int i = 0; i < all.Count; i++)
            {
                WorkSlot slot = all[i];
                if (!ReferenceEquals(slot.Workplace, Location))
                    continue;
                first ??= slot;
                _slots.Add(new LocationSheetSlot(_texts.Get(slot.Definition.NameKey),
                    _texts.Get(slot.Holder == null ? TextKeys.SheetFreeSlot : slot.Holder.NameKey)));
            }
            _view.ShowSlots(_slots);
            _view.ShowActions(_actions.For(Location, first));
        }

        private void OnActionClicked(int index)
        {
            if (Location != null)
                _actions.Click(index);
        }

        private void OnCloseClicked()
        {
            Close();
        }

        private void OnStockChanged(Structure warehouse)
        {
            if (ReferenceEquals(warehouse, Location))
                Refresh();
        }

        private void OnSlotChanged(Villager villager, WorkSlot slot)
        {
            Refresh();
        }

        private void OnSleepChanged(Villager villager)
        {
            Refresh();
        }
    }
}
