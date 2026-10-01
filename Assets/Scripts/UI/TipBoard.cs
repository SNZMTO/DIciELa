using System;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'astuce, en bas à gauche de l'écran (GDD, section 16) : un cadre de bois, le portrait de la Chimère dans une tuile
    /// lavande (la photo de sa tête prise à la validation), et une phrase du jeu qui dit quoi faire ensuite. C'est la vue
    /// (le bloc « astuce » de Hud.uxml) ; son présentateur décide. Elle se cache quand il n'y a rien à dire.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TipBoard : MonoBehaviour, ITipView
    {
        private const string TipName = "astuce";
        private const string PortraitName = "astuce-portrait";
        private const string TextName = "astuce-texte";
        private const string HiddenClass = "bloc-cache";
        // Une astuce nouvelle part de là (décalée, transparente) puis glisse à sa place : la transition est dans Hud.uss.
        private const string ArrivingClass = "astuce-arrivee";
        private const long ArrivingMilliseconds = 30;

        private UIDocument _document;
        private VisualElement _tip;
        private VisualElement _portrait;
        private Label _text;
        private TipPresenter _presenter;
        private Action _arrived;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>
        /// Montre l'astuce de ce monde, avec ce portrait (null : la tuile reste vide) ; appelé par le démarrage quand la
        /// ville s'ouvre. Le portrait reste à celui qui l'a pris.
        /// </summary>
        public void Show(World world, TextTable texts, Texture2D portrait)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            VisualElement tip = root.Q(TipName);
            VisualElement picture = root.Q(PortraitName);
            var text = root.Q<Label>(TextName);
            if (tip == null || picture == null || text == null)
                throw new InvalidOperationException("L'astuce ne trouve pas ses éléments (UXML).");

            _tip = tip;
            _portrait = picture;
            _text = text;
            _portrait.style.backgroundImage = portrait != null ? new StyleBackground(portrait) : StyleKeyword.None;
            _presenter = new TipPresenter(this, world, texts);
        }

        /// <summary>Cache l'astuce et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_tip == null)
                return;
            ShowTip("");
            _portrait.style.backgroundImage = StyleKeyword.None;
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowTip(string text)
        {
            bool arrives = !string.IsNullOrEmpty(text) && text != _text.text;
            _text.text = text;
            _tip.EnableInClassList(HiddenClass, string.IsNullOrEmpty(text));
            if (!arrives)
                return;
            _arrived ??= () => _tip.RemoveFromClassList(ArrivingClass);
            _tip.AddToClassList(ArrivingClass);
            _tip.schedule.Execute(_arrived).StartingIn(ArrivingMilliseconds);
        }
    }
}
