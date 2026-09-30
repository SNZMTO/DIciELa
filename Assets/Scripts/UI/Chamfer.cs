using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La géométrie des coins coupés de l'interface (GDD, section 16) : un rectangle aux quatre coins coupés à 45°, et la
    /// règle qui garde des pans parallèles quand une forme en contient une autre.
    /// </summary>
    public static class Chamfer
    {
        // Rentrer de d dans un coin coupé à 45° raccourcit le pan de d × (2 − √2) : le pan intérieur reste parallèle.
        private static readonly float InsetFactor = 2f - Mathf.Sqrt(2f);

        /// <summary>Le pan d'une forme logée à inset pixels dans une forme au pan cut ; jamais négatif.</summary>
        public static float Inner(float cut, float inset)
        {
            return Mathf.Max(0f, cut - InsetFactor * inset);
        }

        /// <summary>Le rectangle rentré de chaque côté.</summary>
        public static Rect Inset(Rect box, float left, float top, float right, float bottom)
        {
            return new Rect(box.xMin + left, box.yMin + top, box.width - left - right, box.height - top - bottom);
        }

        /// <summary>Ajoute au tracé en cours le contour du rectangle aux coins coupés de cut pixels.</summary>
        public static void Trace(Painter2D painter, Rect box, float cut)
        {
            cut = Mathf.Clamp(cut, 0f, Mathf.Min(box.width, box.height) * 0.5f);
            painter.MoveTo(new Vector2(box.xMin + cut, box.yMin));
            painter.LineTo(new Vector2(box.xMax - cut, box.yMin));
            painter.LineTo(new Vector2(box.xMax, box.yMin + cut));
            painter.LineTo(new Vector2(box.xMax, box.yMax - cut));
            painter.LineTo(new Vector2(box.xMax - cut, box.yMax));
            painter.LineTo(new Vector2(box.xMin + cut, box.yMax));
            painter.LineTo(new Vector2(box.xMin, box.yMax - cut));
            painter.LineTo(new Vector2(box.xMin, box.yMin + cut));
            painter.ClosePath();
        }

        /// <summary>Remplit d'une couleur le rectangle aux coins coupés.</summary>
        public static void Fill(Painter2D painter, Rect box, float cut, Color color)
        {
            painter.fillColor = color;
            painter.BeginPath();
            Trace(painter, box, cut);
            painter.Fill();
        }
    }
}
