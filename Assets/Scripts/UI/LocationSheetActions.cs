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
    internal sealed class LocationSheetActions
    {
        private readonly World _world;
        private readonly TextTable _texts;
        private readonly List<LocationSheetAction> _shown = new List<LocationSheetAction>();
        private readonly List<Action> _clicks = new List<Action>();
        private readonly Action _postClick;
        private readonly Action _sleepClick;
        private Structure _location;

        public LocationSheetActions(World world, TextTable texts)
        {
            _world = world;
            _texts = texts;
            _postClick = OnPostClicked;
            _sleepClick = OnSleepClicked;
        }

        /// <summary>Les boutons de ce lieu, dont voici un poste (null : il n'en a aucun).</summary>
        public IReadOnlyList<LocationSheetAction> For(Structure location, WorkSlot slot)
        {
            _location = location;
            _shown.Clear();
            _clicks.Clear();
            Villager chimera = _world.Villagers.Chimera;
            if (chimera == null)
                return _shown;
            if (slot != null)
                Add(IsHere(chimera, location) ? TextKeys.SheetRemoveChimera : TextKeys.SheetPutChimera, Pastel.Menthe,
                    _postClick);
            if (ReferenceEquals(location, _world.Structures.ChimeraHome))
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
        public static bool IsHere(Villager villager, Structure location)
        {
            return villager.Slot != null && ReferenceEquals(villager.Slot.Workplace, location);
        }

        private void Add(string textKey, Pastel pastel, Action click)
        {
            _shown.Add(new LocationSheetAction(_texts.Get(textKey), pastel));
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
            if (chimera == null || _location == null)
                return;
            if (IsHere(chimera, _location))
                _world.Submit(new UnassignVillagerCommand(chimera.Id));
            else
                _world.Submit(new AssignVillagerCommand(chimera.Id, _location.Id));
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
