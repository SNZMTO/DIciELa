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
    public sealed class PaintBarPresenter : IDisposable
    {
        private readonly IPaintBarView _view;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly PaintTool[] _tools;
        private readonly PlazaZone _plaza;
        private readonly Floors _floors;
        private readonly List<FloorDefinition> _shownFloors = new List<FloorDefinition>();
        private PaintTool _shown;
        private bool _inksShown;

        public PaintBarPresenter(IPaintBarView view, TextTable texts, PaintTool plaza, PaintTool paths, World world,
            IToolKeys keys)
        {
            if (view == null || ReferenceEquals(texts, null) || ReferenceEquals(plaza, null) ||
                ReferenceEquals(paths, null) || ReferenceEquals(world, null) || keys == null)
                throw new ArgumentException("La barre des outils de sol a besoin de sa vue, des textes, des deux outils, " +
                                            "du monde et des touches.");

            _view = view;
            _texts = texts;
            _keys = keys;
            _tools = new[] { plaza, paths };
            _plaza = world.Plaza;
            _floors = world.Floors;
            _view.ModeChosen += OnModeChosen;
            _view.InkChosen += OnInkChosen;
            foreach (PaintTool tool in _tools)
            {
                tool.HeldChanged += OnHeldChanged;
                tool.ResultChanged += OnResultChanged;
            }
            _plaza.PlazaChanged += ShowValues;
            _plaza.MarkPlaced += OnMarksChanged;
            _plaza.MarkRemoved += OnMarksChanged;
            _plaza.PlazaRefused += OnRefused;
            _floors.FloorRefused += OnFloorRefused;
            OnHeldChanged();
        }

        public void Dispose()
        {
            _view.ModeChosen -= OnModeChosen;
            _view.InkChosen -= OnInkChosen;
            foreach (PaintTool tool in _tools)
            {
                tool.HeldChanged -= OnHeldChanged;
                tool.ResultChanged -= OnResultChanged;
            }
            _plaza.PlazaChanged -= ShowValues;
            _plaza.MarkPlaced -= OnMarksChanged;
            _plaza.MarkRemoved -= OnMarksChanged;
            _plaza.PlazaRefused -= OnRefused;
            _floors.FloorRefused -= OnFloorRefused;
            _view.ShowReason("");
            _view.ShowShown(false);
        }

        // L'outil de sol en main ; null si aucun ne l'est.
        private PaintTool Held
        {
            get
            {
                foreach (PaintTool tool in _tools)
                {
                    if (tool.IsActive)
                        return tool;
                }
                return null;
            }
        }

        private void OnModeChosen(PaintToolMode mode)
        {
            Held?.Choose(mode);
        }

        private void OnInkChosen(PaintInk ink)
        {
            Held?.ChooseInk(ink);
        }

        // Un outil pris, lâché, ou qui change : la barre montre celui qui est en main, ses modes et ses encres refaits
        // seulement quand ils changent.
        private void OnHeldChanged()
        {
            PaintTool held = Held;
            _view.ShowShown(held != null);
            if (held == null)
            {
                _view.ShowReason("");
                return;
            }
            if (!ReferenceEquals(held, _shown))
            {
                _shown = held;
                _view.ShowModes(PaintBarContent.Modes(held, _texts, _keys));
                _inksShown = false;
            }
            if (!_inksShown || !SameFloors(held.Inks.Known))
            {
                _inksShown = true;
                _shownFloors.Clear();
                _shownFloors.AddRange(held.Inks.Known);
                _view.ShowInks(PaintBarContent.Inks(held, _texts));
            }
            _view.SelectMode(held.Mode);
            _view.SelectInk(held.Inks.Chosen);
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

        private void OnMarksChanged(PlazaMark mark)
        {
            ShowValues();
        }

        private void ShowValues()
        {
            PaintTool held = Held;
            if (held == null)
                return;
            (string area, string marks, string size) = PaintBarContent.Values(held, _plaza, _texts);
            _view.ShowValues(area, marks, size);
        }

        // Une place de charrette sous le pointeur dit qu'un clic la retire ; un refus dit sa raison ; « rien ne change »
        // ne dit rien.
        private void OnResultChanged()
        {
            PaintTool held = Held;
            if (held == null)
                _view.ShowReason("");
            else if (held.HoveredMark != null)
                _view.ShowReason(_texts.Get(TextKeys.PlazaRemoveMark));
            else if (!held.Answer.IsOk && !held.Answer.ChangesNothing)
                _view.ShowReason(_texts.Get(held.Answer.ReasonKey));
            else
                _view.ShowReason("");
        }

        private void OnRefused(PlazaResult reason)
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
