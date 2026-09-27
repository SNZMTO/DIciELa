using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les calculs de la caméra de gestion, sans état (GDD, section 16) : l'inclinaison selon la distance, qui s'abaisse
    /// doucement en zoomant (chaque cran de molette l'incline d'autant, de près comme de loin), plus le décalage choisi au
    /// bouton du milieu, le tout borné : le haut de l'écran reste sous l'horizon, la vue d'oiseau a sa limite. Puis la
    /// place de la caméra autour du point visé.
    /// </summary>
    public static class CameraPose
    {
        /// <summary>L'inclinaison de la vue, en degrés sous l'horizon, à cette distance et avec ce décalage.</summary>
        public static float Pitch(CameraSettings settings, float distance, float tiltDegrees)
        {
            return Mathf.Clamp(CurvePitch(settings, distance) + tiltDegrees, LowestPitch(settings), HighestPitch(settings));
        }

        /// <summary>L'inclinaison de la courbe seule : le logarithme de la distance, pour qu'un cran vaille partout pareil.</summary>
        public static float CurvePitch(CameraSettings settings, float distance)
        {
            float farness = Mathf.InverseLerp(Mathf.Log(settings.NearDistance), Mathf.Log(settings.FarDistance),
                Mathf.Log(Mathf.Max(distance, 1e-3f)));
            return Mathf.Lerp(settings.NearPitch, settings.FarPitch, farness);
        }

        /// <summary>L'inclinaison la plus basse : le haut de l'écran reste à sa marge sous l'horizon.</summary>
        public static float LowestPitch(CameraSettings settings)
        {
            return settings.FieldOfView * 0.5f + settings.HorizonMarginDegrees;
        }

        /// <summary>Un déplacement dans le repère de la vue (x à droite, y devant), ramené sur le plan XZ.</summary>
        public static Vector2 ViewToGround(float yawDegrees, Vector2 viewMeters)
        {
            float yaw = yawDegrees * Mathf.Deg2Rad;
            var right = new Vector2(Mathf.Cos(yaw), -Mathf.Sin(yaw));
            var forward = new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw));
            return right * viewMeters.x + forward * viewMeters.y;
        }

        /// <summary>Pose la caméra à cette distance du point visé et à cet angle, jamais sous le sol plus sa garde.</summary>
        public static void Place(Transform camera, Vector3 target, float yawDegrees, float pitchDegrees, float distance,
            Relief relief, float groundClearance)
        {
            Quaternion rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
            Vector3 position = target - rotation * Vector3.forward * distance;
            float ground = relief.HeightAt(position.x, position.z) + groundClearance;
            if (position.y < ground)
                position.y = ground;
            camera.SetPositionAndRotation(position, Quaternion.LookRotation(target - position, Vector3.up));
        }

        /// <summary>L'inclinaison la plus forte, jamais sous la plus basse.</summary>
        public static float HighestPitch(CameraSettings settings)
        {
            return Mathf.Max(settings.MaxPitch, LowestPitch(settings));
        }
    }
}
