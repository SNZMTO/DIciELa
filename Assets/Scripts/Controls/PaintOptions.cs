using Diciela.Data;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Les réglages d'un outil de sol qui ne tiennent à aucun geste (décision 216), ceux de la rangée d'options de sa
    /// barre : la forme du mode Forme, le style d'un tracé ou d'un polygone (droit ou en courbe), la taille du pinceau
    /// (dans ses bornes, par crans), et le cap de la place de charrette, qu'on tourne comme une construction à la pose.
    /// Seul son outil les change, qui le dit.
    /// </summary>
    public sealed class PaintOptions
    {
        private readonly PlacementSettings _settings;

        internal PaintOptions(PlacementSettings settings, float startRadius)
        {
            _settings = settings;
            Vector2 range = settings.BrushRadiusRangeMeters;
            BrushRadius = Mathf.Clamp(startRadius, range.x, range.y);
        }

        /// <summary>La forme du mode Forme.</summary>
        public PaintShape Shape { get; private set; }

        /// <summary>Comment un tracé et un polygone relient leurs points.</summary>
        public PaintTraceStyle Style { get; private set; }

        /// <summary>Le rayon du pinceau, de la gomme et du tracé, en mètres.</summary>
        public float BrushRadius { get; private set; }

        /// <summary>Le cap de la place de charrette en main, en degrés, vers l'est depuis le nord.</summary>
        public float HeadingDegrees { get; private set; }

        /// <summary>Prend cette forme ; vrai si elle change.</summary>
        internal bool Choose(PaintShape shape)
        {
            if (shape == Shape)
                return false;
            Shape = shape;
            return true;
        }

        /// <summary>Prend ce style ; vrai s'il change.</summary>
        internal bool Choose(PaintTraceStyle style)
        {
            if (style == Style)
                return false;
            Style = style;
            return true;
        }

        /// <summary>Grandit (positif) ou rapetisse le pinceau d'autant de crans, dans ses bornes ; vrai s'il change.</summary>
        internal bool Resize(int notches)
        {
            Vector2 range = _settings.BrushRadiusRangeMeters;
            float radius = Mathf.Clamp(BrushRadius + notches * _settings.BrushRadiusStepMeters, range.x, range.y);
            if (Mathf.Approximately(radius, BrushRadius))
                return false;
            BrushRadius = radius;
            return true;
        }

        /// <summary>Tourne la place de charrette de cet angle, en degrés.</summary>
        internal void Rotate(float degrees)
        {
            HeadingDegrees = Mathf.Repeat(HeadingDegrees + degrees, 360f);
        }
    }
}
