using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la barre des outils de sol (décisions 206, 210 et 216) : une seule barre pour l'outil de la
    /// Place et l'outil Chemins, montrée tant que l'un d'eux est en main, pour celui-là. Au-dessus de ses modes, ce qu'il
    /// peut poser (la zone, les sols que la ville sait faire ; cliquer l'un d'eux le prend) ; ses modes en familles
    /// (cliquer l'un d'eux le prend, comme sa touche ; son usage au survol) ; dessous, ses options : les formes du mode
    /// Forme, le style d'un tracé ou d'un polygone, la taille du pinceau ; pour l'outil de la Place, la superficie sur celle de son niveau (« 312 / 450 m² ») et
    /// ses places de charrette sur celles qu'elle permet ; dans la bulle des raisons, ce qu'en dit PaintBarReasons. Il
    /// écoute les outils, la Place et les sols : la barre ne photographie jamais.
    /// </summary>
    public sealed class PaintBarPresenter : IDisposable
    {
        private static readonly IReadOnlyList<PaintBarChoice<PaintShape>> NoShapes =
            Array.Empty<PaintBarChoice<PaintShape>>();
        private static readonly IReadOnlyList<PaintBarChoice<PaintTraceStyle>> NoStyles =
            Array.Empty<PaintBarChoice<PaintTraceStyle>>();
        private readonly IPaintBarView _view;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly PaintTool[] _tools;
        private readonly PlazaZone _plaza;
        private readonly PaintBarReasons _reasons;
        private readonly List<FloorDefinition> _shownFloors = new List<FloorDefinition>();
        private PaintTool _shown;
        private bool _inksShown;
        private bool _shapesShown;
        private bool _stylesShown;

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
            _reasons = new PaintBarReasons(view, texts, () => Held, world);
            _view.ModeChosen += OnModeChosen;
            _view.ShapeChosen += OnShapeChosen;
            _view.StyleChosen += OnStyleChosen;
            _view.InkChosen += OnInkChosen;
            foreach (PaintTool tool in _tools)
            {
                tool.HeldChanged += OnHeldChanged;
                tool.ResultChanged += _reasons.Show;
            }
            _plaza.PlazaChanged += ShowValues;
            _plaza.MarkPlaced += OnMarksChanged;
            _plaza.MarkRemoved += OnMarksChanged;
            OnHeldChanged();
        }

        public void Dispose()
        {
            _view.ModeChosen -= OnModeChosen;
            _view.ShapeChosen -= OnShapeChosen;
            _view.StyleChosen -= OnStyleChosen;
            _view.InkChosen -= OnInkChosen;
            foreach (PaintTool tool in _tools)
            {
                tool.HeldChanged -= OnHeldChanged;
                tool.ResultChanged -= _reasons.Show;
            }
            _plaza.PlazaChanged -= ShowValues;
            _plaza.MarkPlaced -= OnMarksChanged;
            _plaza.MarkRemoved -= OnMarksChanged;
            _reasons.Dispose();
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

        private void OnShapeChosen(PaintShape shape)
        {
            Held?.Choose(shape);
        }

        private void OnStyleChosen(PaintTraceStyle style)
        {
            Held?.Choose(style);
        }

        private void OnInkChosen(PaintInk ink)
        {
            Held?.ChooseInk(ink);
        }

        // Un outil pris, lâché, ou qui change : la barre montre celui qui est en main ; ses modes, ses encres et ses formes
        // ne sont refaits que quand ils changent.
        private void OnHeldChanged()
        {
            PaintTool held = Held;
            _view.ShowShown(held != null);
            if (held == null)
            {
                _reasons.Show();
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
            ShowOptions(held);
            _view.SelectMode(held.Mode);
            _view.SelectInk(held.Inks.Chosen);
            ShowValues();
            _reasons.Show();
        }

        // Les rangées d'options du mode pris, refaites seulement quand elles paraissent ou disparaissent : les formes au
        // mode Forme ; les styles au tracé, et au polygone du mode Forme.
        private void ShowOptions(PaintTool held)
        {
            bool shapes = held.Mode == PaintToolMode.Shape;
            if (shapes != _shapesShown)
            {
                _shapesShown = shapes;
                _view.ShowShapes(shapes ? PaintBarContent.Shapes(_texts) : NoShapes);
            }
            bool styles = held.Mode == PaintToolMode.Trace || shapes && held.Options.Shape == PaintShape.Polygon;
            if (styles != _stylesShown)
            {
                _stylesShown = styles;
                _view.ShowStyles(styles ? PaintBarContent.Styles(_texts) : NoStyles);
            }
            _view.SelectShape(held.Options.Shape);
            _view.SelectStyle(held.Options.Style);
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
    }
}
