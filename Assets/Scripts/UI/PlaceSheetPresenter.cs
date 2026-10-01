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
    /// chose, ou rien, la ferme, comme la croix et Échap. La fiche dit le nom du lieu, ce qu'il tient pour la ville, son
    /// poste et qui le tient, et ses boutons (PlaceSheetActions) : la Chimère au poste, et au lit dans son logis. Il écoute
    /// les stocks, la table des postes et le sommeil de la Chimère : la fiche ne photographie jamais.
    /// </summary>
    public sealed class PlaceSheetPresenter : IDisposable
    {
        private readonly IPlaceSheetView _view;
        private readonly World _world;
        private readonly TextTable _texts;
        private readonly List<PlaceSheetLine> _lines = new List<PlaceSheetLine>();
        private readonly PlaceSheetActions _actions;

        public PlaceSheetPresenter(IPlaceSheetView view, World world, TextTable texts)
        {
            if (view == null || ReferenceEquals(world, null) || ReferenceEquals(texts, null))
                throw new ArgumentException("La fiche d'un lieu a besoin de sa vue, du monde et des textes.");

            _view = view;
            _world = world;
            _texts = texts;
            _actions = new PlaceSheetActions(world, texts);
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
        public Structure Place { get; private set; }

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
            Place = touched;
            Refresh();
            _view.ShowSheet(true);
        }

        /// <summary>Ferme la fiche ; faux si elle l'était déjà.</summary>
        public bool Close()
        {
            if (Place == null)
                return false;
            Place = null;
            _view.ShowSheet(false);
            return true;
        }

        // Un lieu a une fiche s'il a un poste ou un stock, ou s'il est le logis de la Chimère (on l'y envoie dormir).
        private bool HasASheet(Structure place)
        {
            return _world.WorkSlots.IsWorkplace(place) || _world.Warehouses.StockOf(place) != null || IsHome(place);
        }

        private bool IsHome(Structure place)
        {
            return ReferenceEquals(place, _world.Structures.ChimeraHome);
        }

        private void Refresh()
        {
            if (Place == null)
                return;
            _view.ShowTitle(_texts.Get(Place.Definition.NameKey));

            _lines.Clear();
            Inventory stock = _world.Warehouses.StockOf(Place);
            for (int line = 0; stock != null && line < stock.LineCount; line++)
            {
                GoodDefinition good = stock.GoodAt(line);
                _lines.Add(new PlaceSheetLine(good.Icon, _texts.Get(good.NameKey), stock.CountAt(line)));
            }
            _view.ShowStock(_lines);

            Villager chimera = _world.Villagers.Chimera;
            WorkSlot slot = ShownSlot(chimera);
            if (slot == null)
                _view.ShowSlot("", "");
            else
                _view.ShowSlot(_texts.Get(slot.Definition.NameKey), HolderOf(slot, chimera));
            _view.ShowActions(_actions.For(Place, slot));
        }

        private string HolderOf(WorkSlot slot, Villager chimera)
        {
            if (slot.Holder == null)
                return _texts.Get(TextKeys.SheetFreeSlot);
            return ReferenceEquals(slot.Holder, chimera) ? _texts.Get(TextKeys.SheetChimera) : "";
        }

        /// <summary>Le poste que la fiche montre : celui que la Chimère tient ici, sinon le premier du lieu.</summary>
        private WorkSlot ShownSlot(Villager chimera)
        {
            if (chimera != null && PlaceSheetActions.IsHere(chimera, Place))
                return chimera.Slot;
            foreach (WorkSlot slot in _world.WorkSlots.All)
            {
                if (ReferenceEquals(slot.Workplace, Place))
                    return slot;
            }
            return null;
        }

        private void OnActionClicked(int index)
        {
            if (Place != null)
                _actions.Click(index);
        }

        private void OnCloseClicked()
        {
            Close();
        }

        private void OnStockChanged(Structure warehouse)
        {
            if (ReferenceEquals(warehouse, Place))
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
