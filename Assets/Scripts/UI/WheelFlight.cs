using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'icône qui saute de la roue au curseur (décision 225) : une copie du visage pris, posée sur lui, qui file vers le
    /// pointeur en rapetissant et s'efface. Un seul élément (« roue-vol » de Hud.uxml), hors de la roue, pour voler
    /// encore quand elle se ferme ; un vol neuf reprend l'ancien. Le pointeur le traverse.
    /// </summary>
    internal sealed class WheelFlight
    {
        private const string StartClass = "roue-vol-depart";

        private readonly VisualElement _flight;

        public WheelFlight(VisualElement flight)
        {
            _flight = flight;
        }

        /// <summary>Lance la copie de ce visage, de son milieu jusqu'à ce point du panneau.</summary>
        public void Launch(VisualElement face, Vector2 to)
        {
            Vector2 origin = _flight.parent != null ? _flight.parent.worldBound.position : Vector2.zero;
            Vector2 from = face.worldBound.center - origin;
            Vector2 end = to - origin;
            _flight.style.backgroundImage = new StyleBackground(face.resolvedStyle.backgroundImage);
            HudMotion.Enter(_flight, StartClass, 0, () => _flight.style.translate = new Translate(from.x, from.y),
                () => _flight.style.translate = new Translate(end.x, end.y));
        }
    }
}
