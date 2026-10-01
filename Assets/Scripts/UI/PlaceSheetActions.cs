using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;

namespace Diciela.UI
{
    /// <summary>
    /// Les boutons de la fiche d'un lieu (GDD, section 16 ; décision 172) et ce qu'ils font, par les ordres du joueur : au
    /// poste du lieu, mettre la Chimère ou l'en retirer (feutre menthe) ; au logis de la Chimère, l'envoyer dormir, ou la
    /// réveiller, même en chemin vers son lit (feutre lavande). Ils se refont à chaque rafraîchissement de la fiche, sans
    /// rien allouer.
    /// </summary>
    internal sealed class PlaceSheetActions
    {
        private readonly World _world;
        private readonly TextTable _texts;
        private readonly List<PlaceSheetAction> _shown = new List<PlaceSheetAction>();
        private readonly List<Action> _clicks = new List<Action>();
        private readonly Action _postClick;
        private readonly Action _sleepClick;
        private Structure _place;

        public PlaceSheetActions(World world, TextTable texts)
        {
            _world = world;
            _texts = texts;
            _postClick = OnPostClicked;
            _sleepClick = OnSleepClicked;
        }

        /// <summary>Les boutons de ce lieu, dont la fiche montre ce poste (null : aucun).</summary>
        public IReadOnlyList<PlaceSheetAction> For(Structure place, WorkSlot slot)
        {
            _place = place;
            _shown.Clear();
            _clicks.Clear();
            Villager chimera = _world.Villagers.Chimera;
            if (chimera == null)
                return _shown;
            if (slot != null)
                Add(IsHere(chimera, place) ? TextKeys.SheetRemoveChimera : TextKeys.SheetPutChimera, Pastel.Menthe,
                    _postClick);
            if (ReferenceEquals(place, _world.Structures.ChimeraHome))
                Add(InBed(chimera) ? TextKeys.SheetWake : TextKeys.SheetSleep, Pastel.Lavande, _sleepClick);
            return _shown;
        }

        /// <summary>Le joueur clique le bouton de ce rang.</summary>
        public void Click(int index)
        {
            if (index >= 0 && index < _clicks.Count)
                _clicks[index]();
        }

        /// <summary>Vrai si cet habitant tient un poste de ce lieu.</summary>
        public static bool IsHere(Villager villager, Structure place)
        {
            return villager.Slot != null && ReferenceEquals(villager.Slot.Workplace, place);
        }

        private void Add(string textKey, Pastel pastel, Action click)
        {
            _shown.Add(new PlaceSheetAction(_texts.Get(textKey), pastel));
            _clicks.Add(click);
        }

        // Endormie, ou en chemin vers son lit : « Réveiller » l'arrête dans les deux cas.
        private static bool InBed(Villager chimera)
        {
            return chimera.IsAsleep || chimera.IsGoingToBed;
        }

        private void OnPostClicked()
        {
            Villager chimera = _world.Villagers.Chimera;
            if (chimera == null || _place == null)
                return;
            if (IsHere(chimera, _place))
                _world.Submit(new UnassignVillagerCommand(chimera.Id));
            else
                _world.Submit(new AssignVillagerCommand(chimera.Id, _place.Id));
        }

        private void OnSleepClicked()
        {
            Villager chimera = _world.Villagers.Chimera;
            if (chimera == null)
                return;
            if (InBed(chimera))
                _world.Submit(new WakeChimeraCommand());
            else
                _world.Submit(new SendChimeraToBedCommand());
        }
    }
}
