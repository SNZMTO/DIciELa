using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La roue (GDD, section 16 ; décisions 168 et 221) : elle s'ouvre autour du pointeur, touche tenue, là où l'on en
    /// est (son anneau, WheelRing), ou au clic sur l'une de ses deux portes, en bas à droite (WheelDoors) : l'icône des
    /// outils, sur les outils, et la carte de l'outil en main, sur ses réglages. Touche tenue, on choisit en
    /// relâchant ; au clic, on clique, et un appui à côté referme ; la case Outils la retourne sur les outils, ouverte.
    /// Échap la ferme. C'est la vue (Hud.uxml, ToolWheelElements) ; son présentateur décide. Elle se branche quand la
    /// ville s'ouvre et se débranche quand elle se ferme, infobulle comprise.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ToolWheel : MonoBehaviour, IToolWheelView, IToolWheel, IRingKeys, IEscapable
    {
        private UIDocument _document;
        private ToolWheelElements _elements;
        private WheelRing _ring;
        private WheelDoors _doors;
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
            _doors = new WheelDoors(elements, tooltips, () => _presenter != null ? _presenter.IconTooltip() : ("", ""));
            _doors.IconClicked += OnIconClicked;
            _doors.CardClicked += OnCardClicked;
            _ring = new WheelRing(elements);
            _ring.Dismissed += Close;
            _ring.Picked += Pick;
            _presenter = new ToolWheelPresenter(this, texts, keys, clearing, build, move, paths, plaza);
            _ring.AimChanged += _presenter.Aim;
            _ring.Scrolled += _presenter.Scroll;
        }

        /// <summary>Ferme la roue, cache l'icône, la débranche et oublie le monde.</summary>
        public void Hide()
        {
            if (_ring == null)
                return;
            Close();
            _doors.IconClicked -= OnIconClicked;
            _doors.CardClicked -= OnCardClicked;
            _doors.Dispose();
            _doors = null;
            _ring.Dismissed -= Close;
            _ring.Picked -= Pick;
            if (_presenter != null)
            {
                _ring.AimChanged -= _presenter.Aim;
                _ring.Scrolled -= _presenter.Scroll;
            }
            _ring.Dispose();
            _ring = null;
            _presenter = null;
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
            if (!_ring.PickAimed())
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
                OpenByClick(true);
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
            _doors.ShowKey(key);
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
                OpenByClick(true);
        }

        private void OnCardClicked()
        {
            OpenByClick(false);
        }

        // Au clic : la roue des outils au-dessus de leur icône, ou celle des réglages de l'outil en main au-dessus de
        // sa carte (sur les outils pour un outil sans réglages).
        private void OpenByClick(bool onTools)
        {
            if (_presenter == null)
                return;
            _presenter.Open(onTools);
            Open(onTools ? _doors.IconCenter : _doors.CardCenter, false);
        }

        // Une case ou une option prise : son icône saute au curseur, la roue se ferme ; la case Outils la retourne, ouverte.
        private void Pick(int slot, int option)
        {
            if (_presenter == null)
                return;
            if (_presenter.Pick(slot, option))
                _ring.IsHeld = false;
            else
            {
                _ring.Fly(slot, option);
                Close();
            }
        }

        private void Open(Vector2 at, bool held)
        {
            _ring.Open(at, held);
            _doors.ShowOpen(true);
        }

        private void Close()
        {
            if (_ring == null || !_ring.IsOpen)
                return;
            _ring.Close();
            _doors.ShowOpen(false);
        }

        private Vector2 ToPanel(Vector2 screenPoint)
        {
            return HudArea.ToPanel(_document.rootVisualElement.panel, screenPoint);
        }
    }
}
