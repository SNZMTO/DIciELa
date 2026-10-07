using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Viser le sol (SAVOIR, 2.7) : le point du relief sous un point de l'écran, par un seul rayon de la caméra, jusqu'à
    /// son plan lointain. La pose, l'outil Déplacer et l'outil de la Place visent ainsi ; tirer le sol avec la caméra est
    /// une autre question, qui a sa réponse (GroundGrip).
    /// </summary>
    internal static class GroundAim
    {
        /// <summary>Le point du sol, en mètres sur le plan XZ, sous ce point de l'écran ; faux si le rayon le manque.</summary>
        public static bool TryGround(Camera camera, Relief relief, Vector2 screenPoint, out Vector2 ground)
        {
            Ray ray = camera.ScreenPointToRay(screenPoint);
            bool hit = relief.TryRaycast(ray.origin, ray.direction, camera.farClipPlane, out Vector3 point);
            ground = new Vector2(point.x, point.z);
            return hit;
        }

        /// <summary>
        /// Le cap des côtés du sol vus de cette caméra, au cran de rotation : des rectangles tracés de la même vue
        /// s'alignent (décision 206).
        /// </summary>
        public static float SideDegrees(Camera camera, float stepDegrees)
        {
            return Mathf.Repeat(Mathf.Round(camera.transform.eulerAngles.y / stepDegrees) * stepDegrees, 360f);
        }
    }
}
