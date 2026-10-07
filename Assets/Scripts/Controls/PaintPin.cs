using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// L'épingle de la symétrie d'un outil de sol (décision 216) : le centre des copies, qu'on prend au pointeur et qu'on
    /// glisse où l'on veut, tant qu'une symétrie est prise. La première symétrie la pose au milieu de la zone de la Place
    /// (l'outil de la Place, la zone tracée), sinon au sol sous le milieu de l'écran ; ses axes suivent alors les côtés de
    /// la caméra, au cran de rotation.
    /// </summary>
    internal sealed class PaintPin
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly PaintHandles _handles;
        private readonly PlacementSettings _settings;
        private readonly Vector2[] _one = new Vector2[1];
        private readonly List<Vector2Int> _cells = new List<Vector2Int>();
        private bool _held;

        public PaintPin(World world, Camera camera, PaintHandles handles, PlacementSettings settings)
        {
            _world = world;
            _camera = camera;
            _handles = handles;
            _settings = settings;
        }

        /// <summary>Pose l'épingle où il faut, si l'outil n'en a pas encore.</summary>
        public void PlaceByDefault(PaintTool tool)
        {
            if (tool.Options.HasPin)
                return;
            Vector2 center = Vector2.zero;
            if (tool.Entry.LaysTheZone && _world.Plaza.CellCount > 0)
            {
                _world.Plaza.CopyCells(_cells);
                foreach (Vector2Int cell in _cells)
                    center += _world.Grid.CellCenter(cell);
                center /= _cells.Count;
            }
            else
            {
                var middle = new Vector2(_camera.pixelWidth * 0.5f, _camera.pixelHeight * 0.5f);
                GroundAim.TryGround(_camera, _world.Map.Relief, middle, out center);
            }
            tool.Options.PlacePin(center, GroundAim.SideDegrees(_camera, _settings.RotationStepDegrees));
        }

        /// <summary>L'appui à ce point de l'écran : vrai s'il prend l'épingle d'une symétrie.</summary>
        public bool Press(Vector2 screen, PaintOptions options)
        {
            _one[0] = options.PinCenter;
            _held = options.Symmetry != PaintSymmetry.None && options.HasPin && _handles.At(_one, screen) == 0;
            return _held;
        }

        /// <summary>Le pointeur est à ce point du sol : l'épingle tenue le suit ; vrai si elle était tenue.</summary>
        public bool Drag(Vector2 ground, PaintOptions options)
        {
            if (!_held)
                return false;
            options.MovePin(ground);
            return true;
        }

        /// <summary>Le relâchement, ou l'abandon : vrai si l'épingle était tenue.</summary>
        public bool Release()
        {
            bool held = _held;
            _held = false;
            return held;
        }
    }
}
