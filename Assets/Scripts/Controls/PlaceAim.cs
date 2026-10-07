using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Ce que vise un outil de sol, et ce qu'en disent la Place ou les sols, selon son encre (décisions 206 et 210) : la
    /// réponse de la même fonction que l'ordre, les mailles que le geste changerait, la place de charrette qu'un clic
    /// retirerait ; il le montre par l'aperçu. La vérification ne se refait que si quelque chose a changé (la forme, la maille visée, la Place, le
    /// mode) : le rond du pinceau au repos se refait quand le pointeur change de maille, pas à chaque image.
    /// </summary>
    internal sealed class PlaceAim
    {
        private static readonly Vector2Int Nowhere = new Vector2Int(int.MinValue, int.MinValue);

        private readonly World _world;
        private readonly PlaceToolPreview _preview;
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private readonly Vector2[] _dotCenter = new Vector2[1];
        private Vector2Int _dotCell = Nowhere;
        private bool _cellsShown;

        public PlaceAim(World world, PlaceToolPreview preview)
        {
            _world = world;
            _preview = preview;
        }

        /// <summary>Le rond du pinceau sous le pointeur, au milieu de sa maille : ce qu'un clic peint ou gomme.</summary>
        public GroundShape Dot { get; private set; }

        public PaintAnswer Answer { get; private set; } = PaintAnswer.Ok;
        public PlaceMark Hovered { get; private set; }

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

        /// <summary>Montre ce que donnerait cet outil, dans son mode et avec son encre, visant ce point, avec ce geste.</summary>
        public void Show(PlaceTool tool, Vector2 aim, PlaceDrawing drawing)
        {
            PlaceToolMode mode = tool.Mode;
            float radius = tool.BrushRadius;
            if (mode == PlaceToolMode.CartSpot)
            {
                ShowMark(aim, tool.HeadingDegrees);
                return;
            }
            if (!drawing.IsDrawing && (mode == PlaceToolMode.Disc || mode == PlaceToolMode.Box))
            {
                Hide();
                return;
            }
            GroundShape shape = drawing.IsDrawing ? drawing.Shape : DotAt(aim, radius);
            if (IsStale)
            {
                IsStale = false;
                Hovered = null;
                Answer = Check(shape, mode == PlaceToolMode.Eraser, tool.Inks, tool.Entry.Scope);
                ShowCells(Answer.IsOk);
            }
            _preview.ShowOutline(_world.Map.Relief, Outline(mode, aim, radius, drawing),
                Answer.IsOk || Answer.ChangesNothing);
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
            PlaceZone place = _world.Place;
            Hovered = null;
            for (int i = 0; i < place.Marks.Count && Hovered == null; i++)
            {
                if (place.Marks[i].Footprint.Overlaps(aim, 0f))
                    Hovered = place.Marks[i];
            }
            Answer = Hovered != null ? PaintAnswer.Ok : PaintAnswer.Of(place.CheckMark(aim, headingDegrees));
            RotatedRect footprint = Hovered != null ? Hovered.Footprint : place.MarkFootprint(aim, headingDegrees);
            _preview.ShowOutline(_world.Map.Relief, PlaceOutline.Mark(footprint),
                Hovered == null && Answer.IsOk);
        }

        // Ce que la Place ou les sols disent de cette forme, selon ce que pose l'outil ; les mailles changées dans _cells.
        private PaintAnswer Check(GroundShape shape, bool erase, PaintInks inks, FloorScope scope)
        {
            _cells.Clear();
            if (shape == null)
                return PaintAnswer.Of(PlaceResult.Nothing);
            if (inks.LaysTheZone)
                return PaintAnswer.Of(erase
                    ? _world.Place.CheckErase(shape, _cells)
                    : _world.Place.CheckPaint(shape, _cells));
            if (erase)
                return PaintAnswer.Of(_world.Floors.CheckErase(shape, scope, _cells));
            return inks.Floor == null
                ? PaintAnswer.Of(FloorResult.Nothing)
                : PaintAnswer.Of(_world.Floors.CheckTrace(shape, inks.Floor, scope, _cells));
        }

        private GroundShape DotAt(Vector2 aim, float radius)
        {
            Vector2Int cell = _world.Grid.CellOf(aim);
            if (cell == _dotCell && Dot != null)
                return Dot;
            _dotCell = cell;
            _dotCenter[0] = _world.Grid.CellCenter(cell);
            Dot = new GroundStroke(_dotCenter, radius);
            IsStale = true;
            return Dot;
        }

        private void ShowCells(bool allowed)
        {
            _preview.ShowCells(_world.Grid, _world.Map.Relief, _cells, allowed);
            _cellsShown = _cells.Count > 0;
        }

        // Le tour : le cercle du pinceau sous le pointeur, ou la forme qu'on tire.
        private static PlaceOutline Outline(PlaceToolMode mode, Vector2 aim, float radius, PlaceDrawing drawing)
        {
            switch (drawing.Shape)
            {
                case GroundDisc disc when mode == PlaceToolMode.Disc:
                    return PlaceOutline.Circle(disc.Center, disc.Radius);
                case GroundBox box when mode == PlaceToolMode.Box:
                    return PlaceOutline.Box(box.Rect);
            }
            return mode == PlaceToolMode.Brush || mode == PlaceToolMode.Eraser
                ? PlaceOutline.Circle(aim, radius)
                : PlaceOutline.None;
        }
    }
}
