using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la barre des outils de sol (décisions 206 et 210) : une seule barre pour l'outil de la Place et
    /// l'outil Chemins, montrée tant que l'un d'eux est en main, pour celui-là. Devant ses modes, ce qu'il peut poser (la
    /// zone, les sols que la ville sait faire ; cliquer l'un d'eux le prend) ; ses modes (cliquer l'un d'eux le prend,
    /// comme sa touche ; son usage au survol) ; dessous, pour l'outil de la Place, la superficie sur celle de son niveau
    /// (« 312 / 450 m² ») et ses places de charrette sur celles qu'elle permet, et pour les deux la taille du pinceau ;
    /// dans la bulle des raisons, pourquoi le geste est refusé ou ce qu'un clic retirerait ; un ordre que l'aperçu
    /// laissait passer mais que le monde refuse en le jouant le dit aussi, jusqu'à ce que la visée change. Il écoute les
    /// outils, la Place et les sols : la barre ne photographie jamais.
    /// </summary>
    public sealed class PlaceBarPresenter : IDisposable
    {
        private readonly IPlaceBarView _view;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly PlaceTool[] _tools;
        private readonly PlaceZone _place;
        private readonly Floors _floors;
        private readonly List<FloorDefinition> _shownFloors = new List<FloorDefinition>();
        private PlaceTool _shown;
        private bool _inksShown;

        public PlaceBarPresenter(IPlaceBarView view, TextTable texts, PlaceTool place, PlaceTool paths, World world,
            IToolKeys keys)
        {
            if (view == null || ReferenceEquals(texts, null) || ReferenceEquals(place, null) ||
                ReferenceEquals(paths, null) || ReferenceEquals(world, null) || keys == null)
                throw new ArgumentException("La barre des outils de sol a besoin de sa vue, des textes, des deux outils, " +
                                            "du monde et des touches.");

            _view = view;
            _texts = texts;
            _keys = keys;
            _tools = new[] { place, paths };
            _place = world.Place;
            _floors = world.Floors;
            _view.ModeChosen += OnModeChosen;
            _view.InkChosen += OnInkChosen;
            foreach (PlaceTool tool in _tools)
            {
                tool.HeldChanged += OnHeldChanged;
                tool.ResultChanged += OnResultChanged;
            }
            _place.PlaceChanged += ShowValues;
            _place.MarkPlaced += OnMarksChanged;
            _place.MarkRemoved += OnMarksChanged;
            _place.PlaceRefused += OnRefused;
            _floors.FloorRefused += OnFloorRefused;
            OnHeldChanged();
        }

        public void Dispose()
        {
            _view.ModeChosen -= OnModeChosen;
            _view.InkChosen -= OnInkChosen;
            foreach (PlaceTool tool in _tools)
            {
                tool.HeldChanged -= OnHeldChanged;
                tool.ResultChanged -= OnResultChanged;
            }
            _place.PlaceChanged -= ShowValues;
            _place.MarkPlaced -= OnMarksChanged;
            _place.MarkRemoved -= OnMarksChanged;
            _place.PlaceRefused -= OnRefused;
            _floors.FloorRefused -= OnFloorRefused;
            _view.ShowReason("");
            _view.ShowShown(false);
        }

        // L'outil de sol en main ; null si aucun ne l'est.
        private PlaceTool Held
        {
            get
            {
                foreach (PlaceTool tool in _tools)
                {
                    if (tool.IsActive)
                        return tool;
                }
                return null;
            }
        }

        private void OnModeChosen(PlaceToolMode mode)
        {
            Held?.Choose(mode);
        }

        private void OnInkChosen(FloorDefinition floor)
        {
            Held?.ChooseFloor(floor);
        }

        // Un outil pris, lâché, ou qui change : la barre montre celui qui est en main, ses modes et ses encres refaits
        // seulement quand ils changent.
        private void OnHeldChanged()
        {
            PlaceTool held = Held;
            _view.ShowShown(held != null);
            if (held == null)
            {
                _view.ShowReason("");
                return;
            }
            if (!ReferenceEquals(held, _shown))
            {
                _shown = held;
                _view.ShowModes(PlaceBarContent.Modes(held, _texts, _keys));
                _inksShown = false;
            }
            if (!_inksShown || !SameFloors(held.Inks.Known))
            {
                _inksShown = true;
                _shownFloors.Clear();
                _shownFloors.AddRange(held.Inks.Known);
                _view.ShowInks(PlaceBarContent.Inks(held, _texts));
            }
            _view.SelectMode(held.Mode);
            _view.SelectInk(held.Inks.Floor);
            ShowValues();
            OnResultChanged();
        }

        private bool SameFloors(IReadOnlyList<FloorDefinition> known)
        {
            if (known.Count != _shownFloors.Count)
                return false;
            for (int i = 0; i < known.Count; i++)
            {
                if (!ReferenceEquals(known[i], _shownFloors[i]))
                    return false;
            }
            return true;
        }

        private void OnMarksChanged(PlaceMark mark)
        {
            ShowValues();
        }

        private void ShowValues()
        {
            PlaceTool held = Held;
            if (held == null)
                return;
            (string area, string marks, string size) = PlaceBarContent.Values(held, _place, _texts);
            _view.ShowValues(area, marks, size);
        }

        // Une place de charrette sous le pointeur dit qu'un clic la retire ; un refus dit sa raison ; « rien ne change »
        // ne dit rien.
        private void OnResultChanged()
        {
            PlaceTool held = Held;
            if (held == null)
                _view.ShowReason("");
            else if (held.HoveredMark != null)
                _view.ShowReason(_texts.Get(TextKeys.PlaceRemoveMark));
            else if (!held.Answer.IsOk && !held.Answer.ChangesNothing)
                _view.ShowReason(_texts.Get(held.Answer.ReasonKey));
            else
                _view.ShowReason("");
        }

        private void OnRefused(PlaceResult reason)
        {
            Say(PaintAnswer.Of(reason));
        }

        private void OnFloorRefused(FloorResult reason)
        {
            Say(PaintAnswer.Of(reason));
        }

        private void Say(PaintAnswer refusal)
        {
            if (Held != null && !refusal.ChangesNothing)
                _view.ShowReason(_texts.Get(refusal.ReasonKey));
        }
    }
}
