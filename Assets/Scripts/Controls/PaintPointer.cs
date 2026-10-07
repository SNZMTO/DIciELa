using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Ce que vise le pointeur d'un outil de sol (décisions 206 et 220) : le point du sol sous lui, son point à l'écran,
    /// et si l'aimant des places de charrette joue (Ctrl tenu le coupe). Hors du sol, il ne trouve rien à viser ; c'est la
    /// surface de l'outil qui décide s'il cesse de viser (un geste en cours attend qu'il revienne).
    /// </summary>
    internal sealed class PaintPointer
    {
        private readonly Camera _camera;
        private readonly Relief _relief;

        public PaintPointer(Camera camera, Relief relief)
        {
            _camera = camera;
            _relief = relief;
        }

        /// <summary>Vrai tant qu'il vise le sol.</summary>
        public bool IsAiming { get; private set; }

        /// <summary>Le point du sol visé, en mètres.</summary>
        public Vector2 Ground { get; private set; }

        /// <summary>Le point de l'écran d'où il le vise, en pixels.</summary>
        public Vector2 Screen { get; private set; }

        /// <summary>Vrai si une place de charrette s'aimante à côté d'une place posée ; faux Ctrl tenu.</summary>
        public bool Magnet { get; private set; } = true;

        /// <summary>Vise le sol sous ce point de l'écran, aimanté ou non ; faux s'il n'y a pas de sol là.</summary>
        public bool Aim(Vector2 screenPoint, bool magnet)
        {
            Magnet = magnet;
            if (!TryGround(screenPoint, out Vector2 ground))
                return false;
            IsAiming = true;
            Ground = ground;
            Screen = screenPoint;
            return true;
        }

        /// <summary>Ne vise plus rien.</summary>
        public void Lose()
        {
            IsAiming = false;
        }

        /// <summary>Le point du sol sous ce point de l'écran ; faux s'il n'y en a pas.</summary>
        public bool TryGround(Vector2 screenPoint, out Vector2 ground)
        {
            return GroundAim.TryGround(_camera, _relief, screenPoint, out ground);
        }
    }
}
