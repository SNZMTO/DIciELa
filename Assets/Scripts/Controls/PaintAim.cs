using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Ce que vise un outil de sol, et ce qu'en disent la Place ou les sols, selon son encre (décisions 206 et 210) :
    /// la réponse de la même fonction que l'ordre, les mailles que le geste changerait, la place de charrette qu'un
    /// clic retirerait ; il le montre par l'aperçu. La vérification ne se refait que si quelque chose a changé (la
    /// forme, la maille visée, la Place, le mode) : le rond du pinceau au repos se refait quand le pointeur change de
    /// maille, pas à chaque image.
    /// </summary>
    internal sealed class PaintAim
    {
        private static readonly Vector2Int Nowhere = new Vector2Int(int.MinValue, int.MinValue);
        private static readonly Vector2[] NoPoints = new Vector2[0];
        private readonly List<GroundCopy> _copies = new List<GroundCopy>();
        private readonly List<PaintOutline> _outlines = new List<PaintOutline>();
        private readonly PaintPinView _pin = new PaintPinView();

        private readonly World _world;
        private readonly PaintToolPreview _preview;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private readonly Vector2[] _dotCenter = new Vector2[1];
        private Vector2Int _dotCell = Nowhere;
        // Le rond du pinceau sous le pointeur, au milieu de sa maille : ce qu'un clic peint ou gomme.
        private GroundShape _dot;
        private bool _cellsShown;

        public PaintAim(World world, PaintToolPreview preview)
        {
            _world = world;
            _preview = preview;
        }

        public PaintAnswer Answer { get; private set; } = PaintAnswer.Ok;

        /// <summary>Ce que le geste ferait, avec ses copies par symétrie : la forme vérifiée, celle qui partira.</summary>
        public GroundShape Checked { get; private set; }

        /// <summary>Les copies de la symétrie, la forme elle-même la première.</summary>
        public IReadOnlyList<GroundCopy> Copies => _copies;
        public PlazaMark Hovered { get; private set; }

        /// <summary>Vrai tant que la réponse n'a pas été redemandée depuis le dernier changement.</summary>
        public bool IsStale { get; private set; } = true;

        /// <summary>Quelque chose a changé : la réponse se redemandera.</summary>
        public void Invalidate()
        {
            IsStale = true;
        }

        /// <summary>Le pinceau a changé de taille : son rond se refera.</summary>
        public void ForgetDot()
        {
            _dotCell = Nowhere;
            IsStale = true;
        }

        /// <summary>
        /// Montre ce que donnerait cet outil, dans son mode et avec son encre, visant ce point, avec ce geste.
        /// </summary>
        public void Show(PaintTool tool, Vector2 aim, PaintDrawing drawing)
        {
            PaintToolMode mode = tool.Mode;
            float radius = tool.Options.BrushRadius;
            _preview.ShowHandles(_world.Map.Relief, drawing.IsPointed ? drawing.Points : NoPoints);
            _pin.Show(_preview, _world.Map.Relief, tool.Options);
            PaintCopies.Fill(tool.Options, _copies);
            if (mode == PaintToolMode.CartSpot)
            {
                ShowMark(aim, tool.Options.HeadingDegrees);
                return;
            }
            GroundShape shape = drawing.IsDrawing ? drawing.Shape : mode == PaintToolMode.Shape ? null : DotAt(aim, radius);
            if (IsStale)
            {
                IsStale = false;
                Hovered = null;
                Checked = PaintCopies.Apply(shape, _copies);
                Answer = Check(Checked, mode == PaintToolMode.Eraser, tool.Inks, tool.Entry.Scope);
                ShowCells(Answer.IsOk);
            }
            _outlines.Clear();
            _outlines.Add(Outline(mode, aim, radius, drawing));
            _preview.ShowOutlines(_world.Map.Relief, _outlines, Answer.IsOk || Answer.ChangesNothing);
        }

        /// <summary>Plus rien n'est visé : l'aperçu se cache.</summary>
        public void Hide()
        {
            _preview.Hide();
            _cellsShown = false;
            Answer = PaintAnswer.Ok;
            Hovered = null;
            IsStale = true;
        }

        // Une place de charrette : celle qu'on vise, qu'un clic retirerait, sinon celle qu'un clic poserait ici.
        private void ShowMark(Vector2 aim, float headingDegrees)
        {
            IsStale = false;
            if (_cellsShown)
            {
                _cells.Clear();
                ShowCells(true);
            }
            PlazaZone plaza = _world.Plaza;
            Hovered = null;
            for (int i = 0; i < plaza.Marks.Count && Hovered == null; i++)
            {
                if (plaza.Marks[i].Footprint.Overlaps(aim, 0f))
                    Hovered = plaza.Marks[i];
            }
            Answer = Hovered != null ? PaintAnswer.Ok : PaintAnswer.Of(plaza.CheckMark(aim, headingDegrees));
            _outlines.Clear();
            if (Hovered != null)
                _outlines.Add(PaintOutline.Mark(Hovered.Footprint));
            else
            {
                foreach (GroundCopy copy in _copies)
                    _outlines.Add(PaintOutline.Mark(plaza.MarkFootprint(copy.Apply(aim), copy.HeadingOf(headingDegrees))));
            }
            _preview.ShowOutlines(_world.Map.Relief, _outlines,
                Hovered == null && Answer.IsOk);
        }

        // Ce que la Place ou les sols disent de cette forme, selon ce que pose l'outil ; les mailles changées dans _cells.
        private PaintAnswer Check(GroundShape shape, bool erase, PaintInks inks, FloorScope scope)
        {
            _cells.Clear();
            if (shape == null)
                return PaintAnswer.Of(PlazaResult.Nothing);
            PaintInk ink = inks.Chosen;
            if (ink.IsZone)
                return PaintAnswer.Of(erase
                    ? _world.Plaza.CheckErase(shape, _cells)
                    : _world.Plaza.CheckPaint(shape, _cells));
            if (erase)
                return PaintAnswer.Of(_world.Floors.CheckErase(shape, scope, _cells));
            return ink.IsNone
                ? PaintAnswer.Of(FloorResult.Nothing)
                : PaintAnswer.Of(_world.Floors.CheckTrace(shape, ink.Floor, scope, _cells));
        }

        private GroundShape DotAt(Vector2 aim, float radius)
        {
            Vector2Int cell = _world.Grid.CellOf(aim);
            if (cell == _dotCell && _dot != null)
                return _dot;
            _dotCell = cell;
            _dotCenter[0] = _world.Grid.CellCenter(cell);
            _dot = new GroundStroke(_dotCenter, radius);
            IsStale = true;
            return _dot;
        }

        private void ShowCells(bool allowed)
        {
            _preview.ShowCells(_world.Grid, _world.Map.Relief, _cells, allowed);
            _cellsShown = _cells.Count > 0;
        }

        // Le tour : le cercle du pinceau sous le pointeur (le pinceau, la gomme, le tracé), ou la forme qu'on tire.
        private static PaintOutline Outline(PaintToolMode mode, Vector2 aim, float radius, PaintDrawing drawing)
        {
            switch (drawing.Shape)
            {
                case GroundDisc disc when mode == PaintToolMode.Shape:
                    return PaintOutline.Circle(disc.Center, disc.Radius);
                case GroundBox box when mode == PaintToolMode.Shape:
                    return PaintOutline.Box(box.Rect);
            }
            return mode == PaintToolMode.Shape ? PaintOutline.None : PaintOutline.Circle(aim, radius);
        }
    }
}
