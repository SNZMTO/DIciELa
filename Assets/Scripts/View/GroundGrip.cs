using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La prise du sol par la caméra (SAVOIR, 2.8) : le point du sol qu'on tire au clic droit, et celui que vise un cran
    /// de molette. Chacun est pris sur le vrai relief sous le pointeur, puis gardé sous lui sur un plan à sa hauteur, pour
    /// que la prise ne saute pas pendant le geste. Elle dit de combien déplacer le point visé ; la caméra le déplace.
    /// </summary>
    internal sealed class GroundGrip
    {
        private bool _dragging;
        private Vector3 _dragged;
        private bool _zooming;
        private Vector3 _zoomed;
        private Vector2 _zoomPointer;

        /// <summary>Vrai tant qu'un cran de molette garde son sol sous le pointeur.</summary>
        public bool IsZooming => _zooming;

        public void Clear()
        {
            _dragging = false;
            _zooming = false;
        }

        /// <summary>Un autre geste prend la main : le zoom cesse de garder son sol.</summary>
        public void ReleaseZoom()
        {
            _zooming = false;
        }

        /// <summary>Saisit le sol sous ce point de l'écran ; faux s'il ne vise pas le sol.</summary>
        public bool BeginDrag(Camera camera, Relief relief, Vector2 screenPoint, float fallbackHeight)
        {
            _zooming = false;
            _dragging = TryGround(camera, relief, screenPoint, fallbackHeight, out _dragged);
            return _dragging;
        }

        /// <summary>
        /// Le déplacement du point visé qui ramène le sol saisi sous ce point de l'écran. Un zoom en cours suit la prise :
        /// il garde désormais le sol saisi sous le pointeur, sans le contredire.
        /// </summary>
        public bool TryDrag(Camera camera, Vector2 screenPoint, out Vector2 pivotShift)
        {
            pivotShift = default;
            if (!_dragging || !TryPlane(camera, screenPoint, _dragged.y, out Vector3 underPointer))
                return false;
            pivotShift = new Vector2(_dragged.x - underPointer.x, _dragged.z - underPointer.z);
            if (_zooming)
            {
                _zoomed = _dragged;
                _zoomPointer = screenPoint;
            }
            return true;
        }

        /// <summary>Un cran de molette : le sol sous ce point de l'écran restera sous lui pendant tout le zoom.</summary>
        public void BeginZoom(Camera camera, Relief relief, Vector2 screenPoint, float fallbackHeight)
        {
            _zooming = TryGround(camera, relief, screenPoint, fallbackHeight, out _zoomed);
            _zoomPointer = screenPoint;
        }

        /// <summary>Le déplacement du point visé qui garde le sol du zoom sous son point de l'écran.</summary>
        public bool TryKeepZoom(Camera camera, out Vector2 pivotShift)
        {
            pivotShift = default;
            if (!_zooming || !TryPlane(camera, _zoomPointer, _zoomed.y, out Vector3 underPointer))
                return false;
            pivotShift = new Vector2(_zoomed.x - underPointer.x, _zoomed.z - underPointer.z);
            return true;
        }

        /// <summary>
        /// Le sol sous ce point de l'écran : le vrai relief d'abord ; s'il manque (au bord de la carte), le plan à la
        /// hauteur donnée. Faux au-dessus de l'horizon ou au-delà de ce que la caméra affiche.
        /// </summary>
        private static bool TryGround(Camera camera, Relief relief, Vector2 screenPoint, float fallbackHeight,
            out Vector3 point)
        {
            Ray ray = camera.ScreenPointToRay(screenPoint);
            return relief.TryRaycast(ray.origin, ray.direction, camera.farClipPlane, out point) ||
                   TryPlane(camera, screenPoint, fallbackHeight, out point);
        }

        private static bool TryPlane(Camera camera, Vector2 screenPoint, float height, out Vector3 point)
        {
            Ray ray = camera.ScreenPointToRay(screenPoint);
            var ground = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            if (ground.Raycast(ray, out float along) && along <= camera.farClipPlane)
            {
                point = ray.GetPoint(along);
                return true;
            }
            point = default;
            return false;
        }
    }
}
