using System.Collections.Generic;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Les mesures de la roue des outils (décision 168), dans le repère du panneau : l'outil qu'on vise, par la direction
    /// du pointeur depuis le centre, comme une roue d'armes ; et la place du centre, ramenée pour que la roue tienne dans
    /// l'écran. Rien n'y est réglé : tout se déduit de la place des outils, que donne la feuille de style.
    /// </summary>
    public static class WheelGeometry
    {
        // Près du centre, on ne vise rien : en deçà de cette part de la distance de l'outil le plus proche.
        private const float DeadZoneShare = 0.5f;

        /// <summary>
        /// Le rang de l'outil visé : celui dont la direction depuis le centre est la plus proche de celle du pointeur ;
        /// -1 si le pointeur est trop près du centre, ou s'il n'y a pas d'outil.
        /// </summary>
        public static int Aimed(Vector2 center, IReadOnlyList<Vector2> tools, Vector2 pointer)
        {
            if (tools == null || tools.Count == 0)
                return -1;

            float nearest = float.MaxValue;
            foreach (Vector2 tool in tools)
                nearest = Mathf.Min(nearest, Vector2.Distance(tool, center));
            Vector2 toPointer = pointer - center;
            if (toPointer.magnitude < nearest * DeadZoneShare)
                return -1;

            int aimed = -1;
            float smallest = float.MaxValue;
            for (int i = 0; i < tools.Count; i++)
            {
                float angle = Vector2.Angle(toPointer, tools[i] - center);
                if (angle < smallest)
                {
                    smallest = angle;
                    aimed = i;
                }
            }
            return aimed;
        }

        /// <summary>
        /// Le centre déplacé au plus court pour que la boîte de la roue tienne dans l'écran ; extent est cette boîte
        /// autour du centre, room l'écran.
        /// </summary>
        public static Vector2 Clamped(Vector2 center, Rect extent, Rect room)
        {
            float x = Mathf.Clamp(center.x, room.xMin - extent.xMin, room.xMax - extent.xMax);
            float y = Mathf.Clamp(center.y, room.yMin - extent.yMin, room.yMax - extent.yMax);
            return new Vector2(x, y);
        }
    }
}
