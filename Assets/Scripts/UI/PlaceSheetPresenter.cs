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
    /// Le présentateur de la fiche d'un lieu (GDD, section 16 ; décisions 141 et 152) : toucher un lieu de travail ouvre
    /// sa fiche, qui remplace celle qui était ouverte ; toucher autre chose, ou rien, la ferme, comme la croix et Échap.
    /// La fiche dit le nom du lieu, ce qu'il tient pour la ville, son poste et qui le tient, et son bouton met la Chimère
    /// au poste ou l'en retire, par les ordres du joueur. Il écoute les stocks et la table des postes : la fiche ne
    /// photographie jamais.
    /// </summary>
    public sealed class PlaceSheetPresenter : IDisposable
    {
        private readonly IPlaceSheetView _view;
        private readonly World _world;
        private readonly TextTable _texts;
        private readonly List<PlaceSheetLine> _lines = new List<PlaceSheetLine>();

        public PlaceSheetPresenter(IPlaceSheetView view, World world, TextTable texts)
        {
            if (view == null || ReferenceEquals(world, null) || ReferenceEquals(texts, null))
                throw new ArgumentException("La fiche d'un lieu a besoin de sa vue, du monde et des textes.");

            _view = view;
            _world = world;
            _texts = texts;
            _view.ButtonClicked += OnButtonClicked;
            _view.CloseClicked += OnCloseClicked;
            _world.Warehouses.StockChanged += OnStockChanged;
            _world.WorkSlots.VillagerAssigned += OnSlotChanged;
            _world.WorkSlots.VillagerUnassigned += OnSlotChanged;
            _view.ShowSheet(false);
        }

        /// <summary>Le lieu dont la fiche est ouverte ; null quand elle est fermée.</summary>
        public Structure Place { get; private set; }

        public void Dispose()
        {
            _view.ButtonClicked -= OnButtonClicked;
            _view.CloseClicked -= OnCloseClicked;
            _world.Warehouses.StockChanged -= OnStockChanged;
            _world.WorkSlots.VillagerAssigned -= OnSlotChanged;
            _world.WorkSlots.VillagerUnassigned -= OnSlotChanged;
        }

        /// <summary>On a touché cette construction (null : rien) : un lieu de travail ouvre sa fiche, le reste la ferme.</summary>
        public void Touch(Structure touched)
        {
            if (touched == null || !_world.WorkSlots.IsWorkplace(touched))
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

            WorkSlot slot = FirstSlot();
            Villager chimera = _world.Villagers.Chimera;
            if (slot == null)
                _view.ShowSlot("", "");
            else
                _view.ShowSlot(_texts.Get(slot.Definition.NameKey), HolderOf(slot, chimera));
            if (slot == null || chimera == null)
                _view.ShowButton("");
            else
                _view.ShowButton(_texts.Get(IsHere(chimera) ? TextKeys.SheetRemoveChimera : TextKeys.SheetPutChimera));
        }

        private string HolderOf(WorkSlot slot, Villager chimera)
        {
            if (slot.Holder == null)
                return _texts.Get(TextKeys.SheetFreeSlot);
            return ReferenceEquals(slot.Holder, chimera) ? _texts.Get(TextKeys.SheetChimera) : "";
        }

        private WorkSlot FirstSlot()
        {
            foreach (WorkSlot slot in _world.WorkSlots.All)
            {
                if (ReferenceEquals(slot.Workplace, Place))
                    return slot;
            }
            return null;
        }

        private bool IsHere(Villager villager)
        {
            return villager.Slot != null && ReferenceEquals(villager.Slot.Workplace, Place);
        }

        private void OnButtonClicked()
        {
            Villager chimera = _world.Villagers.Chimera;
            if (Place == null || chimera == null)
                return;
            if (IsHere(chimera))
                _world.Submit(new UnassignVillagerCommand(chimera.Id));
            else
                _world.Submit(new AssignVillagerCommand(chimera.Id, Place.Id));
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
    }
}
