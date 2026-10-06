using System;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La bulle qui dit pourquoi un geste est refusé, en bas au centre, au-dessus de la barre (GDD, section 9) : celle de
    /// la pose (le panneau Construire) et celle de l'outil de la Place (décision 206). Une seule bulle, plusieurs voix :
    /// elle montre ce qu'a dit la dernière qui a parlé, et seule celle-ci peut la faire taire ; une voix qui se tait ne
    /// coupe pas l'autre.
    /// </summary>
    public sealed class ReasonBubble
    {
        private const string BubbleName = "reason";
        private const string TextName = "reason-text";

        private readonly VisualElement _bubble;
        private readonly Label _text;
        private object _speaker;

        public ReasonBubble(UIDocument document)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            _bubble = root?.Q(BubbleName);
            _text = root?.Q<Label>(TextName);
            if (_bubble == null || _text == null)
                throw new InvalidOperationException("La bulle des raisons ne trouve pas ses éléments (UXML).");
            Hide();
        }

        /// <summary>Ce que dit cette voix ; un texte vide la fait taire, si c'est elle qu'on entend.</summary>
        public void Say(object speaker, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (ReferenceEquals(speaker, _speaker))
                    Hide();
                return;
            }
            _speaker = speaker;
            _text.text = text;
            _bubble.EnableInClassList(StyleClasses.Hidden, false);
        }

        /// <summary>Le texte montré ; vide quand la bulle est cachée.</summary>
        public string Text => _speaker == null ? "" : _text.text;

        public void Hide()
        {
            _speaker = null;
            _text.text = "";
            _bubble.EnableInClassList(StyleClasses.Hidden, true);
        }
    }
}
