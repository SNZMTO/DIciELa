using System;
using Diciela.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Là où est le HUD, pour les gestes (ARCHITECTURE, section 11) : un point de l'écran est sur l'interface s'il y a
    /// sous lui un élément du HUD qui se touche (la barre, la fiche, la journée…), ou si un élément du HUD a pris l'appui
    /// de cette image. Un appui appartient à l'interface dès l'enfoncement, même si ce qui l'a reçu disparaît aussitôt (la
    /// roue qu'on congédie d'un appui à côté) : UI Toolkit distribue l'appui avant les LateUpdate des gestes (SAVOIR,
    /// 2.8), qui sans cette mémoire trouveraient la place vide et le prendraient pour la ville. Le conteneur plein écran
    /// ne capte pas le pointeur, ni un bloc caché. Le pointeur compte depuis le bas de l'écran, UI Toolkit depuis le
    /// haut : on retourne l'axe vertical avant de chercher l'élément (SAVOIR, section 3).
    /// </summary>
    public sealed class HudArea : IInterfaceArea
    {
        private readonly UIDocument _document;
        private readonly EventCallback<PointerDownEvent> _onPress;
        private VisualElement _watched;
        private int _pressFrame = -1;

        public HudArea(UIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _document = document;
            _onPress = _ => _pressFrame = Time.frameCount;
            Watch();
        }

        public bool Contains(Vector2 screenPoint)
        {
            Watch();
            if (_pressFrame == Time.frameCount)
                return true;
            IPanel panel = _watched?.panel;
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

        // L'appui pris par un élément du HUD passe par la racine en descendant vers lui ; la racine elle-même ne se touche
        // pas. Un document rechargé a une racine neuve : on la suit.
        private void Watch()
        {
            VisualElement root = _document != null ? _document.rootVisualElement : null;
            if (root == _watched)
                return;
            _watched?.UnregisterCallback(_onPress, TrickleDown.TrickleDown);
            if (root != null)
            {
                root.pickingMode = PickingMode.Ignore;
                root.RegisterCallback(_onPress, TrickleDown.TrickleDown);
            }
            _watched = root;
        }
    }
}
