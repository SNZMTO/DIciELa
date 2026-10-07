using System.Collections.Generic;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Le geste en cours d'un outil de sol (décisions 206 et 216) : ce qu'on trace, en mètres au sol. Glissé, depuis
    /// l'appui : le pinceau et la gomme gardent un point tous les quelques décimètres (SAVOIR, 2.6) ; le cercle a son
    /// milieu à l'appui et son bord sous le pointeur ; le rectangle va d'un coin à l'autre, ses côtés tournés comme la
    /// caméra au cran de rotation. Pointé, le tracé : chaque clic pose un point, et un dernier segment suit le pointeur
    /// jusqu'au clic suivant. La forme ne se refait que quand elle change d'au moins une demi-maille : pas une forme par
    /// image.
    /// </summary>
    internal sealed class PaintDrawing
    {
        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<Vector2> _traced = new List<Vector2>();
        private PaintToolMode _mode;
        private PaintShape _shape;
        private float _radius;
        private float _boxDegrees;
        private Vector2 _end;

        /// <summary>Vrai entre l'appui glissé et le relâchement, ou entre le premier point d'un tracé et son départ.</summary>
        public bool IsDrawing { get; private set; }

        /// <summary>Vrai pour un tracé : il se fait de clics, et ne part pas au relâchement.</summary>
        public bool IsPointed { get; private set; }

        /// <summary>La forme tracée jusqu'ici ; null tant qu'elle est trop petite pour couvrir une maille.</summary>
        public GroundShape Shape { get; private set; }

        /// <summary>Le milieu du cercle, ou le premier coin du rectangle : là où l'on a appuyé.</summary>
        public Vector2 Start => _points.Count > 0 ? _points[0] : Vector2.zero;

        /// <summary>Les points posés d'un tracé, dans l'ordre.</summary>
        public IReadOnlyList<Vector2> Points => _points;

        /// <summary>
        /// Commence un geste glissé à ce point du sol : le pinceau ou la gomme de ce rayon, ou cette forme, le rectangle
        /// tourné de ce cap.
        /// </summary>
        public void Begin(PaintToolMode mode, PaintShape shape, Vector2 start, float radius, float boxDegrees)
        {
            _mode = mode;
            _shape = shape;
            _radius = radius;
            _boxDegrees = boxDegrees;
            _points.Clear();
            _points.Add(start);
            _end = start;
            IsDrawing = true;
            IsPointed = false;
            Shape = IsBrush ? new GroundStroke(_points, _radius) : null;
        }

        /// <summary>Pose un point du tracé, de ce rayon ; le premier commence le tracé.</summary>
        public void AddPoint(Vector2 point, float radius)
        {
            if (!IsDrawing)
            {
                _mode = PaintToolMode.Trace;
                _points.Clear();
                IsDrawing = true;
                IsPointed = true;
            }
            _radius = radius;
            _points.Add(point);
            _end = point;
            Shape = Traced();
        }

        /// <summary>Le pointeur est à ce point du sol : vrai si la forme a changé.</summary>
        public bool ExtendTo(Vector2 point, float spacing, float cellSize)
        {
            if (!IsDrawing)
                return false;
            if (IsBrush)
            {
                if ((point - _points[_points.Count - 1]).sqrMagnitude < spacing * spacing)
                    return false;
                _points.Add(point);
                Shape = new GroundStroke(_points, _radius);
                return true;
            }
            if ((point - _end).sqrMagnitude < cellSize * cellSize * 0.25f)
                return false;
            _end = point;
            if (IsPointed)
            {
                Shape = Traced();
                return true;
            }
            GroundShape before = Shape;
            Shape = _shape == PaintShape.Disc ? Disc(cellSize) : Box(cellSize);
            return Shape != null || before != null;
        }

        /// <summary>Le tracé tel qu'il partira : ses points posés, sans le segment qui suit le pointeur.</summary>
        public void Settle()
        {
            if (!IsPointed)
                return;
            _end = _points[_points.Count - 1];
            Shape = Traced();
        }

        /// <summary>Le geste est fini ou abandonné : plus de forme.</summary>
        public void End()
        {
            IsDrawing = false;
            IsPointed = false;
            Shape = null;
            _points.Clear();
        }

        private bool IsBrush => _mode == PaintToolMode.Brush || _mode == PaintToolMode.Eraser;

        // Le tracé : ses points posés, puis là où est le pointeur.
        private GroundShape Traced()
        {
            _traced.Clear();
            _traced.AddRange(_points);
            _traced.Add(_end);
            return new GroundStroke(_traced, _radius);
        }

        private GroundShape Disc(float cellSize)
        {
            float radius = (_end - _points[0]).magnitude;
            return radius >= cellSize ? new GroundDisc(_points[0], radius) : null;
        }

        // Ses côtés le long du cap et à sa droite ; ses coins, l'appui et le pointeur.
        private GroundShape Box(float cellSize)
        {
            Vector2 forward = Heading.Direction(_boxDegrees);
            var right = new Vector2(forward.y, -forward.x);
            Vector2 diagonal = _end - _points[0];
            var size = new Vector2(Mathf.Abs(Vector2.Dot(diagonal, right)), Mathf.Abs(Vector2.Dot(diagonal, forward)));
            if (size.x < cellSize || size.y < cellSize)
                return null;
            return new GroundBox(new RotatedRect((_points[0] + _end) * 0.5f, size, _boxDegrees));
        }
    }
}
