using System.Collections.Generic;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Le geste en cours de l'outil de la Place (décision 206) : ce qu'on trace depuis l'appui, en mètres au sol. Le
    /// pinceau et la gomme gardent un point tous les quelques décimètres (SAVOIR, 2.6) ; le disque a son milieu à l'appui
    /// et son bord sous le pointeur ; le rectangle va d'un coin à l'autre, ses côtés tournés comme la caméra au cran de
    /// rotation. La forme ne se refait que quand elle change d'au moins une demi-maille : pas une forme par image.
    /// </summary>
    internal sealed class PlaceDrawing
    {
        private readonly List<Vector2> _points = new List<Vector2>();
        private PlaceToolMode _mode;
        private float _radius;
        private float _boxDegrees;
        private Vector2 _end;

        /// <summary>Vrai entre l'appui glissé et le relâchement.</summary>
        public bool IsDrawing { get; private set; }

        /// <summary>La forme tracée jusqu'ici ; null tant qu'elle est trop petite pour couvrir une maille.</summary>
        public GroundShape Shape { get; private set; }

        /// <summary>Le milieu du disque, ou le premier coin du rectangle : là où l'on a appuyé.</summary>
        public Vector2 Start => _points.Count > 0 ? _points[0] : Vector2.zero;

        /// <summary>Commence un geste à ce point du sol ; le pinceau de ce rayon, le rectangle tourné de ce cap.</summary>
        public void Begin(PlaceToolMode mode, Vector2 start, float radius, float boxDegrees)
        {
            _mode = mode;
            _radius = radius;
            _boxDegrees = boxDegrees;
            _points.Clear();
            _points.Add(start);
            _end = start;
            IsDrawing = true;
            Shape = IsBrush ? new GroundStroke(_points, _radius) : null;
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
            GroundShape before = Shape;
            Shape = _mode == PlaceToolMode.Disc ? Disc(cellSize) : Box(cellSize);
            return Shape != null || before != null;
        }

        /// <summary>Le geste est fini ou abandonné : plus de forme.</summary>
        public void End()
        {
            IsDrawing = false;
            Shape = null;
            _points.Clear();
        }

        private bool IsBrush => _mode == PlaceToolMode.Brush || _mode == PlaceToolMode.Eraser;

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
