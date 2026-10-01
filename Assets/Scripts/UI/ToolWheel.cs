using System;
using Diciela.Controls;
using Diciela.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La roue des outils (GDD, section 16 ; décision 168) : l'icône des outils, en bas à droite, et la roue qui s'ouvre
    /// autour du pointeur, quand on tient sa touche ou qu'on clique l'icône (son anneau, WheelRing). Chaque outil y a sa
    /// place fixe (Construire en haut, Retirer en bas) et sa touche ; la bulle du milieu nomme celui qu'on vise. Touche
    /// tenue, on choisit en relâchant ; au clic, on clique l'outil, et un appui à côté referme. Échap la ferme. C'est la
    /// vue (l'icône « outils » et la « roue » de Hud.uxml, ToolWheelElements) ; son présentateur décide.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ToolWheel : MonoBehaviour, IToolWheelView, IToolWheel, IEscapable
    {
        private const string HiddenClass = "bloc-cache";
        private const string OpenClass = "ouvert";

        private UIDocument _document;
        private ToolWheelElements _elements;
        private WheelRing _ring;
        private ToolWheelPresenter _presenter;

        public event Action<ToolChoice> Chosen;
        public event Action<ToolChoice?> Pointed;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>Montre l'icône des outils, la roue fermée ; appelé par le démarrage quand la ville s'ouvre.</summary>
        public void Show(TextTable texts, IToolKeys keys, ClearingTool clearing, IBuildMenu build, Tooltips tooltips)
        {
            Hide();
            ToolWheelElements elements = ToolWheelElements.Find(_document.rootVisualElement);
            if (elements == null || tooltips == null)
                throw new InvalidOperationException("La roue des outils ne trouve pas ses éléments (UXML).");

            _elements = elements;
            // Ces éléments vivent dans le document : branchés une seule fois, ils s'adressent au présentateur du moment.
            if (_ring == null)
            {
                _ring = new WheelRing(elements.Wheel, elements.Center, elements.Plate, elements.Tools);
                Wire(tooltips);
            }
            _presenter = new ToolWheelPresenter(this, texts, keys, clearing, build);
            _elements.Icon.RemoveFromClassList(HiddenClass);
        }

        /// <summary>Ferme la roue, cache l'icône et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_ring == null)
                return;
            Close();
            _elements.Icon.AddToClassList(HiddenClass);
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void OpenAt(Vector2 screenPoint)
        {
            if (_presenter != null)
                Open(ToPanel(screenPoint), true);
        }

        public void PointAt(Vector2 screenPoint)
        {
            if (_ring != null && _ring.IsOpen && _ring.IsHeld)
                _ring.Aim(_ring.ToolAt(ToPanel(screenPoint)));
        }

        public void ReleaseAt(Vector2 screenPoint)
        {
            if (_ring == null || !_ring.IsOpen || !_ring.IsHeld)
                return;
            int tool = _ring.ToolAt(ToPanel(screenPoint));
            Close();
            if (tool >= 0)
                Chosen?.Invoke((ToolChoice)tool);
        }

        public void Choose(ToolChoice choice)
        {
            if (_presenter == null)
                return;
            Close();
            Chosen?.Invoke(choice);
        }

        public bool Escape()
        {
            if (_ring == null || !_ring.IsOpen)
                return false;
            Close();
            return true;
        }

        public void ShowKeys(string wheel, string build, string clear)
        {
            _elements.IconKey.text = wheel;
            _elements.ToolKeys[(int)ToolChoice.Build].text = build;
            _elements.ToolKeys[(int)ToolChoice.Clear].text = clear;
        }

        public void ShowLabel(string name, string detail)
        {
            _elements.Label.text = name;
            _elements.Detail.text = detail;
            _elements.Detail.EnableInClassList(HiddenClass, string.IsNullOrEmpty(detail));
            _elements.Bubble.EnableInClassList(HiddenClass, string.IsNullOrEmpty(name));
        }

        public void ShowActive(bool building, bool clearing)
        {
            _elements.Tools[(int)ToolChoice.Build].EnableInClassList(OpenClass, building);
            _elements.Tools[(int)ToolChoice.Clear].EnableInClassList(OpenClass, clearing);
        }

        private void Wire(Tooltips tooltips)
        {
            _elements.Icon.Clicked += () =>
            {
                if (_ring.IsOpen)
                    Close();
                else if (_presenter != null)
                    Open(_elements.Icon.worldBound.center, false);
            };
            _ring.Dismissed += Close;
            _ring.AimChanged += tool => Pointed?.Invoke(tool >= 0 ? (ToolChoice)tool : (ToolChoice?)null);
            for (int i = 0; i < _elements.Tools.Length; i++)
            {
                var choice = (ToolChoice)i;
                _elements.Tools[i].Clicked += () => Choose(choice);
            }
            tooltips.Attach(_elements.Icon, () => _presenter != null ? _presenter.IconTooltip() : ("", ""), true);
        }

        private void Open(Vector2 at, bool held)
        {
            _ring.Open(at, held);
            _elements.Icon.AddToClassList(OpenClass);
        }

        private void Close()
        {
            if (!_ring.IsOpen)
                return;
            _ring.Close();
            _elements.Icon.RemoveFromClassList(OpenClass);
        }

        private Vector2 ToPanel(Vector2 screenPoint)
        {
            return HudArea.ToPanel(_document.rootVisualElement.panel, screenPoint);
        }
    }
}
