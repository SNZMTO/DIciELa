using System;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La bulle qui dit pourquoi une pose est refusée, en bas au centre, au-dessus du panneau Construire (GDD, section
    /// 9), et qui reste quand le panneau est fermé. Les autres outils le disent sur leur carte (décision 221).
    /// </summary>
    public sealed class ReasonBubble
    {
        private const string BubbleName = "reason";
        private const string TextName = "reason-text";

        private readonly VisualElement _bubble;
        private readonly Label _text;

        public ReasonBubble(UIDocument document)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            _bubble = root?.Q(BubbleName);
            _text = root?.Q<Label>(TextName);
            if (_bubble == null || _text == null)
                throw new InvalidOperationException("La bulle des raisons ne trouve pas ses éléments (UXML).");
            Say("");
        }

        /// <summary>Écrit cette raison ; un texte vide cache la bulle.</summary>
        public void Say(string text)
        {
            _text.text = text ?? "";
            _bubble.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(text));
        }
    }
}
