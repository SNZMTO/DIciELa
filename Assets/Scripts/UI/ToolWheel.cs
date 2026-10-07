using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La roue (GDD, section 16 ; décisions 168 et 221) : l'icône des outils, en bas à droite, et la roue qui s'ouvre autour
    /// du pointeur, touche tenue, là où l'on en est, ou sur les outils au clic sur l'icône (son anneau, WheelRing). Touche
    /// tenue, on choisit en relâchant ; au clic, on clique, et un appui à côté referme ; la case Outils la retourne sur les
    /// outils, ouverte. Échap la ferme. C'est la vue (Hud.uxml, ToolWheelElements) ; son présentateur décide. Elle se
    /// branche quand la ville s'ouvre et se débranche quand elle se ferme, infobulle comprise.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ToolWheel : MonoBehaviour, IToolWheelView, IToolWheel, IRingKeys, IEscapable
    {
        private UIDocument _document;
        private ToolWheelElements _elements;
        private WheelRing _ring;
        private IDisposable _iconTooltip;
        private ToolWheelPresenter _presenter;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>Montre l'icône des outils, la roue fermée ; appelé par le démarrage quand la ville s'ouvre.</summary>
        public void Show(TextTable texts, IToolKeys keys, ClearingTool clearing, IBuildMenu build, MoveTool move,
            PaintTool paths, PaintTool plaza, Tooltips tooltips)
        {
            Hide();
            ToolWheelElements elements = ToolWheelElements.Find(_document.rootVisualElement);
            if (elements == null || tooltips == null)
                throw new InvalidOperationException("La roue des outils ne trouve pas ses éléments (UXML).");

            _elements = elements;
            _ring = new WheelRing(elements.Wheel, elements.Center, elements.Slots, elements.Fan);
            _ring.Dismissed += Close;
            _ring.Picked += Pick;
            _ring.AimChanged += OnAimChanged;
            _ring.Scrolled += OnScrolled;
            _elements.Icon.Clicked += OnIconClicked;
            _iconTooltip = tooltips.Attach(_elements.Icon,
                () => _presenter != null ? _presenter.IconTooltip() : ("", ""), true);
            _presenter = new ToolWheelPresenter(this, texts, keys, clearing, build, move, paths, plaza);
            _elements.Icon.RemoveFromClassList(StyleClasses.Hidden);
        }

        /// <summary>Ferme la roue, cache l'icône, la débranche et oublie le monde.</summary>
        public void Hide()
        {
            _presenter = null;
            if (_ring == null)
                return;
            Close();
            _elements.Icon.Clicked -= OnIconClicked;
            _iconTooltip.Dispose();
            _iconTooltip = null;
            _ring.Dismissed -= Close;
            _ring.Picked -= Pick;
            _ring.AimChanged -= OnAimChanged;
            _ring.Scrolled -= OnScrolled;
            _ring.Dispose();
            _ring = null;
            _elements.Icon.AddToClassList(StyleClasses.Hidden);
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void OpenAt(Vector2 screenPoint)
        {
            if (_presenter == null)
                return;
            _presenter.Open(false);
            Open(ToPanel(screenPoint), true);
        }

        public void PointAt(Vector2 screenPoint)
        {
            if (_ring != null && _ring.IsOpen && _ring.IsHeld)
                _ring.AimAt(ToPanel(screenPoint));
        }

        public void ReleaseAt(Vector2 screenPoint)
        {
            if (_ring == null || !_ring.IsOpen || !_ring.IsHeld)
                return;
            _ring.AimAt(ToPanel(screenPoint));
            if (_ring.AimedSlot >= 0)
                Pick(_ring.AimedSlot, _ring.AimedOption);
            else
                Close();
        }

        public void Choose(ToolChoice choice)
        {
            if (_presenter == null)
                return;
            Close();
            _presenter.Choose(choice);
        }

        /// <summary>Le chiffre d'une case de l'outil de sol en main ; la case Outils ouvre la roue des outils.</summary>
        public void TakeSlot(int index)
        {
            if (_presenter != null && _presenter.TakeSlot(index))
                OpenTools();
        }

        public bool Escape()
        {
            if (_ring == null || !_ring.IsOpen)
                return false;
            Close();
            return true;
        }

        public void ShowWheelKey(string key)
        {
            _elements.IconKey.text = key;
        }

        public void ShowRing(IReadOnlyList<WheelSlot> slots)
        {
            _ring.Show(slots);
        }

        public void ShowLabel(string name, string detail)
        {
            _elements.Label.text = name;
            _elements.Detail.text = detail;
            _elements.Detail.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(detail));
            _elements.Bubble.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(name));
        }

        private void OnIconClicked()
        {
            if (_ring.IsOpen)
                Close();
            else
                OpenTools();
        }

        // La roue des outils, ouverte au clic au-dessus de leur icône.
        private void OpenTools()
        {
            if (_presenter == null)
                return;
            _presenter.Open(true);
            Open(_elements.Icon.worldBound.center, false);
        }

        // Une case ou une option prise : la roue se ferme, sauf la case Outils, qui la retourne et la garde ouverte.
        private void Pick(int slot, int option)
        {
            if (_presenter == null)
                return;
            if (_presenter.Pick(slot, option))
                _ring.IsHeld = false;
            else
                Close();
        }

        private void OnAimChanged(int slot, int option)
        {
            _presenter?.Aim(slot, option);
        }

        private void OnScrolled(int notches)
        {
            _presenter?.Scroll(notches);
        }

        private void Open(Vector2 at, bool held)
        {
            _ring.Open(at, held);
            _elements.Icon.AddToClassList(StyleClasses.Open);
        }

        private void Close()
        {
            if (_ring == null || !_ring.IsOpen)
                return;
            _ring.Close();
            _elements.Icon.RemoveFromClassList(StyleClasses.Open);
        }

        private Vector2 ToPanel(Vector2 screenPoint)
        {
            return HudArea.ToPanel(_document.rootVisualElement.panel, screenPoint);
        }
    }
}
