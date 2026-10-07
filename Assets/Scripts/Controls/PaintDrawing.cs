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
    /// caméra au cran de rotation. Pointé, un tracé (ouvert) ou un polygone (fermé) : chaque clic pose un point, un
    /// dernier bout suit le pointeur jusqu'au clic suivant, les points se déplacent, et ils se relient droit ou en courbe.
    /// La forme glissée ne se refait que quand elle change d'au moins une demi-maille : pas une forme par image.
    /// </summary>
    internal sealed class PaintDrawing
    {
        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<Vector2> _traced = new List<Vector2>();
        private readonly List<Vector2> _curve = new List<Vector2>();
        private PaintToolMode _mode;
        private PaintShape _shape;
        private float _radius;
        private float _boxDegrees;
        private Vector2 _end;
        private bool _closed;
        private bool _curved;
        private bool _settled;
        private float _step;

        /// <summary>Vrai entre l'appui glissé et le relâchement, ou entre le premier point d'un geste pointé et son départ.</summary>
        public bool IsDrawing { get; private set; }

        /// <summary>Vrai pour un tracé ou un polygone : ils se font de clics, et ne partent pas au relâchement.</summary>
        public bool IsPointed { get; private set; }

        /// <summary>La forme tracée jusqu'ici ; null tant qu'elle est trop petite pour couvrir une maille.</summary>
        public GroundShape Shape { get; private set; }

        /// <summary>Les points posés d'un geste pointé, dans l'ordre.</summary>
        public IReadOnlyList<Vector2> Points => _points;

        /// <summary>
        /// Commence un geste glissé à ce point du sol : le pinceau ou la gomme de ce rayon, ou cette forme, le rectangle
        /// tourné de ce cap.
        /// </summary>
        public void Begin(PaintToolMode mode, PaintShape shape, Vector2 start, float radius, float boxDegrees)
        {
            End();
            _mode = mode;
            _shape = shape;
            _radius = radius;
            _boxDegrees = boxDegrees;
            _points.Add(start);
            _end = start;
            IsDrawing = true;
            Shape = IsBrush ? new GroundStroke(_points, _radius) : null;
        }

        /// <summary>
        /// Commence un geste pointé : un tracé de ce rayon, ou un polygone (fermé), droit ou en courbe gardée en segments
        /// d'au plus step mètres ; ses points viennent des clics (AddPoint).
        /// </summary>
        public void BeginPointed(bool closed, bool curved, float radius, float step)
        {
            End();
            _mode = closed ? PaintToolMode.Shape : PaintToolMode.Trace;
            _closed = closed;
            _curved = curved;
            _radius = radius;
            _step = step;
            IsDrawing = true;
            IsPointed = true;
        }

        /// <summary>Pose un point du geste pointé.</summary>
        public void AddPoint(Vector2 point)
        {
            _points.Add(point);
            _end = point;
            _settled = false;
            Shape = Pointed();
        }

        /// <summary>Déplace ce point posé ; pendant qu'on le tient, le bout qui suit le pointeur se tait.</summary>
        public void MovePoint(int index, Vector2 to)
        {
            _points[index] = to;
            _settled = true;
            Shape = Pointed();
        }

        /// <summary>Relie les points droit, ou en courbe.</summary>
        public void Restyle(bool curved)
        {
            _curved = curved;
            if (IsPointed)
                Shape = Pointed();
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
                _settled = false;
                Shape = Pointed();
                return true;
            }
            GroundShape before = Shape;
            Shape = _shape == PaintShape.Disc ? Disc(cellSize) : Box(cellSize);
            return Shape != null || before != null;
        }

        /// <summary>Le geste pointé tel qu'il partira : ses points posés, sans le bout qui suit le pointeur.</summary>
        public void Settle()
        {
            if (!IsPointed)
                return;
            _settled = true;
            Shape = Pointed();
        }

        /// <summary>Le geste est fini ou abandonné : plus de forme.</summary>
        public void End()
        {
            IsDrawing = false;
            IsPointed = false;
            _settled = false;
            Shape = null;
            _points.Clear();
        }

        private bool IsBrush => !IsPointed && (_mode == PaintToolMode.Brush || _mode == PaintToolMode.Eraser);

        // La forme pointée : ses points posés, puis le pointeur (tant qu'il n'est pas posé), droits ou en courbe ; un
        // polygone attend trois coins.
        private GroundShape Pointed()
        {
            _traced.Clear();
            _traced.AddRange(_points);
            if (!_settled)
                _traced.Add(_end);
            List<Vector2> line = _traced;
            if (_curved)
            {
                GroundSpline.Sample(_traced, _closed, _step, _curve);
                line = _curve;
            }
            if (_closed)
                return line.Count >= 3 ? new GroundPolygon(line) : null;
            return line.Count > 0 ? new GroundStroke(line, _radius) : null;
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
