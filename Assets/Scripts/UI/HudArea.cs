using System;
using Diciela.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Là où est le HUD, pour les gestes (ARCHITECTURE, section 11) : un point de l'écran est sur l'interface s'il y a
    /// sous lui un élément du HUD qui se touche (la barre, la fiche, la journée…). Le conteneur plein écran ne capte pas
    /// le pointeur, ni un bloc caché. Le pointeur compte depuis le bas de l'écran, UI Toolkit depuis le haut : on
    /// retourne l'axe vertical avant de chercher l'élément (SAVOIR, section 3).
    /// </summary>
    public sealed class HudArea : IInterfaceArea
    {
        private readonly UIDocument _document;

        public HudArea(UIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _document = document;
            _document.rootVisualElement.pickingMode = PickingMode.Ignore;
        }

        public bool Contains(Vector2 screenPoint)
        {
            IPanel panel = _document != null ? _document.rootVisualElement?.panel : null;
            if (panel == null)
                return false;
            return panel.Pick(ToPanel(panel, screenPoint)) != null;
        }

        /// <summary>Un point de l'écran (en pixels, origine en bas) dans le repère de ce panneau (origine en haut).</summary>
        public static Vector2 ToPanel(IPanel panel, Vector2 screenPoint)
        {
            var fromTop = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            return RuntimePanelUtils.ScreenToPanel(panel, fromTop);
        }
    }
}
