using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les deux portes de la roue, en bas à droite (décisions 168 et 221) : l'icône des outils, qui l'ouvre sur les
    /// outils, avec son infobulle et la touche de la roue sur son étiquette ; et la carte de l'outil en main, qui
    /// l'ouvre sur ses réglages. Tant que la roue est ouverte, l'icône reste enfoncée et la carte s'efface. Branchées à
    /// l'ouverture de la ville, débranchées à sa sortie, l'icône cachée.
    /// </summary>
    internal sealed class WheelDoors : IDisposable
    {
        private const string FadedClass = "carte-outil-effacee";

        private readonly FeltButton _icon;
        private readonly Label _iconKey;
        private readonly FeltButton _card;
        private readonly IDisposable _tooltip;

        public WheelDoors(ToolWheelElements elements, Tooltips tooltips, Func<(string, string)> iconTooltip)
        {
            _icon = elements.Icon;
            _iconKey = elements.IconKey;
            _card = elements.Card;
            _icon.Clicked += OnIconClicked;
            _card.Clicked += OnCardClicked;
            _tooltip = tooltips.Attach(_icon, iconTooltip, true);
            _icon.RemoveFromClassList(StyleClasses.Hidden);
        }

        /// <summary>Levé quand on clique l'icône des outils.</summary>
        public event Action IconClicked;

        /// <summary>Levé quand on clique la carte de l'outil en main.</summary>
        public event Action CardClicked;

        /// <summary>Le milieu de l'icône, dans le repère du panneau : la roue des outils s'ouvre là, au clic.</summary>
        public Vector2 IconCenter => _icon.worldBound.center;

        /// <summary>Le milieu de la carte, dans le repère du panneau : la roue des réglages s'y ouvre.</summary>
        public Vector2 CardCenter => _card.worldBound.center;

        /// <summary>Écrit la touche de la roue sur l'étiquette de l'icône.</summary>
        public void ShowKey(string key)
        {
            _iconKey.text = key;
        }

        /// <summary>La roue s'ouvre ou se ferme : l'icône s'enfonce ou remonte, la carte s'efface ou revient.</summary>
        public void ShowOpen(bool open)
        {
            _icon.EnableInClassList(StyleClasses.Open, open);
            _card.EnableInClassList(FadedClass, open);
        }

        public void Dispose()
        {
            ShowOpen(false);
            _icon.Clicked -= OnIconClicked;
            _card.Clicked -= OnCardClicked;
            _tooltip.Dispose();
            _icon.AddToClassList(StyleClasses.Hidden);
        }

        private void OnIconClicked()
        {
            IconClicked?.Invoke();
        }

        private void OnCardClicked()
        {
            CardClicked?.Invoke();
        }
    }
}
