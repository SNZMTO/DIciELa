using System;
using System.Collections.Generic;
using System.Globalization;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la barre de la Place (décision 206) : la barre se montre tant que l'outil de la Place est en
    /// main ; elle montre ses modes (cliquer l'un d'eux le prend, comme sa touche ; son usage au survol), celui qui est
    /// pris, la superficie de la Place sur celle de son niveau (« 312 / 450 m² »), ses places de charrette posées sur
    /// celles qu'elle permet, la taille du pinceau, et, dans la bulle des raisons, pourquoi le geste est refusé ou ce
    /// qu'un clic retirerait ; un ordre que l'aperçu laissait passer mais que la Place refuse en le jouant (« Sa
    /// charrette arrive… ») le dit aussi, jusqu'à ce que la visée change. Il écoute l'outil et la Place : la barre ne
    /// photographie jamais.
    /// </summary>
    public sealed class PlaceBarPresenter : IDisposable
    {
        private readonly IPlaceBarView _view;
        private readonly TextTable _texts;
        private readonly PlaceTool _tool;
        private readonly PlaceZone _place;

        public PlaceBarPresenter(IPlaceBarView view, TextTable texts, PlaceTool tool, World world, IToolKeys keys)
        {
            if (view == null || ReferenceEquals(texts, null) || ReferenceEquals(tool, null) ||
                ReferenceEquals(world, null) || keys == null)
                throw new ArgumentException("La barre de la Place a besoin de sa vue, des textes, de l'outil, du monde " +
                                            "et des touches.");

            _view = view;
            _texts = texts;
            _tool = tool;
            _place = world.Place;
            var modes = new List<PlaceBarMode>();
            foreach (PlaceToolMode mode in Enum.GetValues(typeof(PlaceToolMode)))
            {
                string name = mode.ToString();
                modes.Add(new PlaceBarMode(mode, texts.Get(TextKeys.PlaceMode(name)),
                    texts.Get(TextKeys.PlaceModeDetail(name)), keys.KeyOfCard((int)mode)));
            }
            _view.ShowModes(modes);
            _view.ModeChosen += OnModeChosen;
            _tool.HeldChanged += OnHeldChanged;
            _tool.ResultChanged += OnResultChanged;
            _place.PlaceChanged += ShowValues;
            _place.MarkPlaced += OnMarksChanged;
            _place.MarkRemoved += OnMarksChanged;
            _place.PlaceRefused += OnRefused;
            OnHeldChanged();
            OnResultChanged();
        }

        public void Dispose()
        {
            _view.ModeChosen -= OnModeChosen;
            _tool.HeldChanged -= OnHeldChanged;
            _tool.ResultChanged -= OnResultChanged;
            _place.PlaceChanged -= ShowValues;
            _place.MarkPlaced -= OnMarksChanged;
            _place.MarkRemoved -= OnMarksChanged;
            _place.PlaceRefused -= OnRefused;
            _view.ShowReason("");
            _view.ShowShown(false);
        }

        private void OnModeChosen(PlaceToolMode mode)
        {
            _tool.Choose(mode);
        }

        private void OnHeldChanged()
        {
            _view.ShowShown(_tool.IsActive);
            _view.SelectMode(_tool.Mode);
            ShowValues();
        }

        private void OnMarksChanged(PlaceMark mark)
        {
            ShowValues();
        }

        private void ShowValues()
        {
            string area = _texts.Get(TextKeys.PlaceArea)
                .Replace(TextKeys.AreaParameter, Whole(_place.AreaSquareMeters))
                .Replace(TextKeys.MaxParameter, Whole(_place.MaxAreaSquareMeters));
            string marks = _texts.Get(TextKeys.PlaceMarks)
                .Replace(TextKeys.MarksParameter, _place.Marks.Count.ToString(CultureInfo.InvariantCulture))
                .Replace(TextKeys.MaxParameter, _place.MaxMarks.ToString(CultureInfo.InvariantCulture));
            bool brush = _tool.Mode == PlaceToolMode.Brush || _tool.Mode == PlaceToolMode.Eraser;
            string size = brush
                ? _texts.Get(TextKeys.PlaceBrushSize).Replace(TextKeys.SizeParameter,
                    (_tool.BrushRadius * 2f).ToString("0.#", CultureInfo.InvariantCulture))
                : "";
            _view.ShowValues(area, marks, size);
        }

        // Une place de charrette sous le pointeur dit qu'un clic la retire ; un refus dit sa raison ; « rien ne change »
        // (le pinceau sur la Place déjà peinte) ne dit rien.
        private void OnResultChanged()
        {
            PaintAnswer answer = _tool.Answer;
            if (!_tool.IsActive)
                _view.ShowReason("");
            else if (_tool.HoveredMark != null)
                _view.ShowReason(_texts.Get(TextKeys.PlaceRemoveMark));
            else if (!answer.IsOk && !answer.ChangesNothing)
                _view.ShowReason(_texts.Get(answer.ReasonKey));
            else
                _view.ShowReason("");
        }

        private void OnRefused(PlaceResult reason)
        {
            if (_tool.IsActive && reason != PlaceResult.Nothing)
                _view.ShowReason(_texts.Get(TextKeys.PlaceRefusal(reason.ToString())));
        }

        private static string Whole(float squareMeters)
        {
            return Mathf.RoundToInt(squareMeters).ToString(CultureInfo.InvariantCulture);
        }
    }
}
