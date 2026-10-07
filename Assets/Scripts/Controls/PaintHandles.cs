using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Les poignées d'un outil de sol (décision 216) : des points posés au sol qu'on prend au pointeur, à l'écran, à
    /// quelques pixels près (les points d'un tracé ou d'un polygone) ; le plus proche gagne.
    /// </summary>
    internal sealed class PaintHandles
    {
        private readonly Camera _camera;
        private readonly Relief _relief;
        private readonly PlacementSettings _settings;

        public PaintHandles(Camera camera, Relief relief, PlacementSettings settings)
        {
            _camera = camera;
            _relief = relief;
            _settings = settings;
        }

        /// <summary>Le point de cette liste sous ce point de l'écran, le plus proche ; -1 s'il n'y en a pas.</summary>
        public int At(IReadOnlyList<Vector2> points, Vector2 screen)
        {
            float best = _settings.HandlePickPixels * _settings.HandlePickPixels;
            int found = -1;
            for (int i = 0; i < points.Count; i++)
            {
                float squared = SquaredPixels(points[i], screen);
                if (squared > best)
                    continue;
                best = squared;
                found = i;
            }
            return found;
        }

        // Le carré de l'écart, en pixels, entre ce point du sol vu à l'écran et ce point de l'écran ; derrière la caméra,
        // l'infini.
        private float SquaredPixels(Vector2 ground, Vector2 screen)
        {
            Vector3 seen = _camera.WorldToScreenPoint(_relief.GroundAt(ground));
            return seen.z > 0f ? ((Vector2)seen - screen).sqrMagnitude : float.PositiveInfinity;
        }
    }
}
