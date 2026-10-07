using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Le geste pointé d'un outil de sol (décision 216) : un tracé, ou un polygone au mode Forme, point par point. Un clic
    /// pose un point ; sur le point qui le finit (le dernier d'un tracé, le premier d'un polygone d'au moins trois coins),
    /// il demande son départ ; un clic sur un autre point ne fait rien ; un appui sur un point le prend, et ce point suit le
    /// pointeur jusqu'au relâchement. Il ne parle ni à la Place ni aux ordres : la surface (PaintCanvas) le fait.
    /// </summary>
    internal sealed class PaintPoints
    {
        private readonly PaintDrawing _drawing;
        private readonly PaintHandles _handles;
        private int _held = -1;

        public PaintPoints(PaintDrawing drawing, PaintHandles handles)
        {
            _drawing = drawing;
            _handles = handles;
        }

        /// <summary>Vrai si cet outil trace point par point dans son mode : le tracé, ou le polygone du mode Forme.</summary>
        public static bool Wanted(PaintTool tool)
        {
            return tool.Mode == PaintToolMode.Trace ||
                   tool.Mode == PaintToolMode.Shape && tool.Options.Shape == PaintShape.Polygon;
        }

        /// <summary>
        /// Un clic à ce point du sol, vu à ce point de l'écran, pour cet outil ; le premier commence le geste. Vrai si le
        /// geste demande à partir.
        /// </summary>
        public bool Click(Vector2 ground, Vector2 screen, PaintTool tool, float curveStep)
        {
            bool closed = tool.Mode == PaintToolMode.Shape;
            if (!_drawing.IsDrawing)
                _drawing.BeginPointed(closed, tool.Options.Style == PaintTraceStyle.Curved, tool.Options.BrushRadius,
                    curveStep);
            IReadOnlyList<Vector2> points = _drawing.Points;
            int hit = _handles.At(points, screen);
            int finish = closed ? 0 : points.Count - 1;
            if (hit >= 0 && hit == finish && points.Count >= (closed ? 3 : 1))
                return true;
            if (hit < 0)
                _drawing.AddPoint(ground);
            return false;
        }

        /// <summary>L'appui à ce point de l'écran : vrai s'il prend un point posé du geste pointé.</summary>
        public bool Press(Vector2 screen)
        {
            _held = _drawing.IsPointed ? _handles.At(_drawing.Points, screen) : -1;
            return _held >= 0;
        }

        /// <summary>Le pointeur est à ce point du sol : le point tenu le suit ; vrai s'il en tenait un.</summary>
        public bool Drag(Vector2 ground)
        {
            if (_held < 0)
                return false;
            _drawing.MovePoint(_held, ground);
            return true;
        }

        /// <summary>Le relâchement, ou l'abandon : vrai s'il tenait un point, qu'il lâche.</summary>
        public bool Release()
        {
            bool held = _held >= 0;
            _held = -1;
            return held;
        }
    }
}
