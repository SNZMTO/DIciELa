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
    /// vue (l'icône « outils » et la « roue » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ToolWheel : MonoBehaviour, IToolWheelView, IToolWheel, IEscapable
    {
        private const string IconName = "outils";
        private const string WheelName = "roue";
        private const string CenterName = "roue-centre";
        private const string BubbleName = "roue-bulle";
        private const string LabelName = "roue-nom";
        private const string DetailName = "roue-suite";
        private const string KeySuffix = "-touche";
        private const string HiddenClass = "bloc-cache";
        private const string OpenClass = "ouvert";
        // Les outils, au rang de leur choix (ToolChoice).
        private static readonly string[] ToolNames = { "roue-construire", "roue-retirer" };

        private UIDocument _document;
        private FeltButton _icon;
        private VisualElement _bubble;
        private Label _label;
        private Label _detail;
        private FeltButton[] _tools;
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
            VisualElement root = _document.rootVisualElement;
            var icon = root.Q<FeltButton>(IconName);
            VisualElement wheel = root.Q(WheelName);
            VisualElement center = root.Q(CenterName);
            VisualElement bubble = root.Q(BubbleName);
            var label = root.Q<Label>(LabelName);
            var detail = root.Q<Label>(DetailName);
            var tools = new FeltButton[ToolNames.Length];
            for (int i = 0; i < tools.Length; i++)
                tools[i] = root.Q<FeltButton>(ToolNames[i]);
            if (icon == null || wheel == null || center == null || bubble == null || label == null || detail == null ||
                Array.IndexOf(tools, null) >= 0 || tooltips == null)
                throw new InvalidOperationException("La roue des outils ne trouve pas ses éléments (UXML).");

            _icon = icon;
            _bubble = bubble;
            _label = label;
            _detail = detail;
            _tools = tools;
            // Ces éléments vivent dans le document : branchés une seule fois, ils s'adressent au présentateur du moment.
            if (_ring == null)
            {
                _ring = new WheelRing(wheel, center, bubble, tools);
                Wire(tooltips);
            }
            _presenter = new ToolWheelPresenter(this, texts, keys, clearing, build);
            _icon.RemoveFromClassList(HiddenClass);
        }

        /// <summary>Ferme la roue, cache l'icône et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_ring == null)
                return;
            Close();
            _icon.AddToClassList(HiddenClass);
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
            _icon.Q<Label>(IconName + KeySuffix).text = wheel;
            KeyOf(ToolChoice.Build).text = build;
            KeyOf(ToolChoice.Clear).text = clear;
        }

        public void ShowLabel(string name, string detail)
        {
            _label.text = name;
            _detail.text = detail;
            _detail.EnableInClassList(HiddenClass, string.IsNullOrEmpty(detail));
            _bubble.EnableInClassList(HiddenClass, string.IsNullOrEmpty(name));
        }

        public void ShowActive(bool building, bool clearing)
        {
            _tools[(int)ToolChoice.Build].EnableInClassList(OpenClass, building);
            _tools[(int)ToolChoice.Clear].EnableInClassList(OpenClass, clearing);
        }

        private void Wire(Tooltips tooltips)
        {
            _icon.Clicked += () =>
            {
                if (_ring.IsOpen)
                    Close();
                else if (_presenter != null)
                    Open(_icon.worldBound.center, false);
            };
            _ring.Dismissed += Close;
            _ring.AimChanged += tool => Pointed?.Invoke(tool >= 0 ? (ToolChoice)tool : (ToolChoice?)null);
            for (int i = 0; i < _tools.Length; i++)
            {
                var choice = (ToolChoice)i;
                _tools[i].Clicked += () => Choose(choice);
            }
            tooltips.Attach(_icon, () => _presenter != null ? _presenter.IconTooltip() : ("", ""), true);
        }

        private void Open(Vector2 at, bool held)
        {
            _ring.Open(at, held);
            _icon.AddToClassList(OpenClass);
        }

        private void Close()
        {
            if (!_ring.IsOpen)
                return;
            _ring.Close();
            _icon.RemoveFromClassList(OpenClass);
        }

        private Label KeyOf(ToolChoice choice)
        {
            return _tools[(int)choice].Q<Label>(ToolNames[(int)choice] + KeySuffix);
        }

        private Vector2 ToPanel(Vector2 screenPoint)
        {
            return HudArea.ToPanel(_document.rootVisualElement.panel, screenPoint);
        }
    }
}
