using System;
using Diciela.Data;
using Diciela.View;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Le bouton de la prévision (GDD, section 16 ; décision 228), au-dessus de l'icône des outils : un feutre lavande,
    /// sa touche sur une étiquette, qui bascule la prévision au clic et reste enfoncé tant qu'elle dure ; son infobulle
    /// dit ce qu'elle montre. Montré à l'ouverture de la ville, caché et débranché à sa sortie.
    /// </summary>
    public sealed class ForesightButton
    {
        private const string ButtonName = "prevision";
        private const string KeyName = "prevision-touche";

        private readonly FeltButton _button;
        private readonly Label _key;
        private Foresight _foresight;
        private IDisposable _tooltip;

        /// <summary>Le bouton de ce document ; il doit le porter (Hud.uxml).</summary>
        public ForesightButton(UIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            VisualElement root = document.rootVisualElement;
            _button = root.Q<FeltButton>(ButtonName);
            _key = root.Q<Label>(KeyName);
            if (_button == null || _key == null)
                throw new InvalidOperationException("Le bouton de la prévision ne trouve pas ses éléments (UXML).");
        }

        /// <summary>Montre le bouton de cette prévision, cette touche sur son étiquette et dans l'infobulle.</summary>
        public void Show(Foresight foresight, TextTable texts, Tooltips tooltips, string key)
        {
            Hide();
            if (foresight == null || texts == null || tooltips == null)
                throw new ArgumentNullException(foresight == null ? nameof(foresight) :
                    texts == null ? nameof(texts) : nameof(tooltips));
            _foresight = foresight;
            _key.text = key;
            _tooltip = tooltips.Attach(_button, () => (texts.Get(TextKeys.ForesightTooltip),
                texts.Get(TextKeys.ForesightTooltipDetail).Replace(TextKeys.KeyParameter, key)), true);
            _button.Clicked += OnClicked;
            _foresight.Changed += OnChanged;
            OnChanged();
            _button.RemoveFromClassList(StyleClasses.Hidden);
        }

        /// <summary>Cache le bouton et le débranche ; accepte de n'avoir rien à cacher.</summary>
        public void Hide()
        {
            if (_foresight == null)
                return;
            _button.Clicked -= OnClicked;
            _foresight.Changed -= OnChanged;
            _foresight = null;
            _tooltip.Dispose();
            _tooltip = null;
            _button.RemoveFromClassList(StyleClasses.Open);
            _button.AddToClassList(StyleClasses.Hidden);
        }

        private void OnClicked()
        {
            _foresight.Toggle();
        }

        private void OnChanged()
        {
            _button.EnableInClassList(StyleClasses.Open, _foresight.IsOn);
        }
    }
}
