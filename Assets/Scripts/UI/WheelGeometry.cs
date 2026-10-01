using System.Collections.Generic;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Les mesures de la roue des outils (décisions 168 et 176), dans le repère du panneau : l'outil qu'on vise, par la
    /// direction où l'on a bougé le pointeur depuis l'ouverture, comme une roue d'armes ; et la place du centre, ramenée
    /// pour que la roue tienne dans l'écran. Rien n'y est réglé : tout se déduit de la place des outils, que donne la
    /// feuille de style. On vise à chaque image tant que la touche est tenue : les boucles vont par rang, sans rien
    /// allouer.
    /// </summary>
    public static class WheelGeometry
    {
        // Près du point d'ouverture, on ne vise rien : en deçà de cette part de la distance de l'outil le plus proche.
        private const float DeadZoneShare = 0.5f;

        /// <summary>
        /// Le rang de l'outil visé : celui dont la direction depuis le centre est la plus proche de celle du pointeur
        /// depuis le point où la roue s'est ouverte ; -1 si le pointeur est encore près de ce point, ou s'il n'y a pas
        /// d'outil. Ce point est le centre, sauf près d'un bord, où la roue est ramenée dans l'écran : relâcher sans
        /// bouger n'y choisit rien non plus (décision 176).
        /// </summary>
        public static int Aimed(Vector2 openedAt, Vector2 center, IReadOnlyList<Vector2> tools, Vector2 pointer)
        {
            if (tools == null || tools.Count == 0)
                return -1;

            float nearest = float.MaxValue;
            for (int i = 0; i < tools.Count; i++)
                nearest = Mathf.Min(nearest, Vector2.Distance(tools[i], center));
            Vector2 toPointer = pointer - openedAt;
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
