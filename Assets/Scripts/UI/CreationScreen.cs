using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'écran de création de la Chimère (GDD, section 6), dans la langue du HUD (section 16) : un cadre de bois sur le
    /// côté, l'atelier visible derrière ; son titre, une ligne par partie (sa pièce et sa couleur, chacune dans un creux
    /// entre deux flèches de feutre), et le grand bouton qui valide. C'est la vue (UI Toolkit, Creation.uxml) ; son
    /// présentateur décide. Elle dit aussi aux gestes si le pointeur est sur elle.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CreationScreen : MonoBehaviour, ICreationView, IInterfaceArea
    {
        private const string PanelName = "panel";
        private const string TitleName = "title";
        private const string RowsName = "rows";
        private const string ConfirmName = "confirm";
        private const string ConfirmTextName = "confirm-text";
        private const string HiddenPanelClass = "bloc-cache";
        private const string RowClass = "row";
        private const string PaletteClass = "palette";
        private static readonly string[] LabelClasses = { "row-label", "libelle" };
        private const string ChoiceClass = "choice";
        private const string ColorClass = "color";
        private const string HiddenClass = "hidden";
        private static readonly string[] ArrowClasses = { "feutre", "sable", "pan-6", "arrow" };
        private static readonly string[] SlotClasses = { "creux", "pan-6" };
        private static readonly string[] ValueClasses = { "libelle", "valeur" };
        private const string PreviousClass = "fleche-precedent";
        private const string NextClass = "fleche-suivant";

        private readonly List<Label> _choices = new List<Label>();
        private readonly List<Label> _colors = new List<Label>();
        private readonly List<VisualElement> _palettes = new List<VisualElement>();
        private UIDocument _document;
        private VisualElement _panel;
        private Label _title;
        private VisualElement _rows;
        private FeltButton _confirm;
        private Label _confirmText;
        private CreationPresenter _presenter;

        public event Action<int, int> Stepped;
        public event Action<int, int> Recolored;
        public event Action Confirmed;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>
        /// Montre l'écran, rempli par son présentateur ; created est appelé quand l'ordre « créer la Chimère » est parti.
        /// </summary>
        public void Show(Catalog catalog, TextTable texts, AppearancePreview preview, World world, Action created)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            // Tous les éléments d'abord, rangés seulement s'ils sont là : un écran à moitié trouvé ne se cache plus.
            VisualElement panel = root.Q(PanelName);
            var title = root.Q<Label>(TitleName);
            VisualElement rows = root.Q(RowsName);
            var confirm = root.Q<FeltButton>(ConfirmName);
            var confirmText = root.Q<Label>(ConfirmTextName);
            if (panel == null || title == null || rows == null || confirm == null || confirmText == null)
                throw new InvalidOperationException("L'écran de création ne trouve pas ses éléments (UXML).");

            _panel = panel;
            _title = title;
            _rows = rows;
            _confirm = confirm;
            _confirmText = confirmText;
            _confirm.Clicked += Confirm;
            _presenter = new CreationPresenter(this, catalog, texts, preview, world, created);
            _panel.RemoveFromClassList(HiddenPanelClass);
        }

        /// <summary>Cache l'écran et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_panel == null)
                return;
            _confirm.Clicked -= Confirm;
            _panel.AddToClassList(HiddenPanelClass);
            _rows.Clear();
            _choices.Clear();
            _colors.Clear();
            _palettes.Clear();
        }

        /// <summary>Valide le personnage : le bouton, ou Entrée par le geste « valider ».</summary>
        public void Confirm()
        {
            Confirmed?.Invoke();
        }

        public void ShowRows(string title, string confirm, IReadOnlyList<string> labels)
        {
            _title.text = title;
            _confirmText.text = confirm;
            _rows.Clear();
            _choices.Clear();
            _colors.Clear();
            _palettes.Clear();
            for (int i = 0; i < labels.Count; i++)
            {
                int row = i;
                var line = Styled(new VisualElement(), RowClass);
                line.Add(Styled(new Label(labels[i]), LabelClasses));
                _choices.Add(Selector(line, ChoiceClass, direction => Stepped?.Invoke(row, direction)));
                var palette = Styled(new VisualElement(), PaletteClass);
                _colors.Add(Selector(palette, ColorClass, direction => Recolored?.Invoke(row, direction)));
                _palettes.Add(palette);
                line.Add(palette);
                _rows.Add(line);
            }
        }

        public void ShowChoice(int row, string choice, string color)
        {
            if (row < 0 || row >= _choices.Count)
                return;
            _choices[row].text = choice;
            _colors[row].text = color;
            _palettes[row].EnableInClassList(HiddenClass, color.Length == 0);
        }

        /// <summary>
        /// Vrai si le pointeur est sur le panneau. Le pointeur compte depuis le bas de l'écran, UI Toolkit depuis le
        /// haut : on retourne l'axe vertical avant de chercher l'élément (SAVOIR, section 3).
        /// </summary>
        public bool Contains(Vector2 screenPoint)
        {
            IPanel panel = _document != null ? _document.rootVisualElement?.panel : null;
            if (panel == null || _panel == null || _panel.ClassListContains(HiddenPanelClass))
                return false;
            var fromTop = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            VisualElement picked = panel.Pick(RuntimePanelUtils.ScreenToPanel(panel, fromTop));
            return picked != null && (picked == _panel || _panel.Contains(picked));
        }

        private void OnDestroy()
        {
            Hide();
        }

        /// <summary>Une flèche, un creux où s'écrit le choix, une flèche, à la suite dans cette ligne ; rend le texte.</summary>
        private static Label Selector(VisualElement line, string style, Action<int> stepped)
        {
            line.Add(Arrow(PreviousClass, () => stepped(-1)));
            var slot = Styled(new Facet(), SlotClasses);
            slot.AddToClassList(style);
            var shown = Styled(new Label { pickingMode = PickingMode.Ignore }, ValueClasses);
            slot.Add(shown);
            line.Add(slot);
            line.Add(Arrow(NextClass, () => stepped(1)));
            return shown;
        }

        private static FeltButton Arrow(string direction, Action clicked)
        {
            var arrow = Styled(new FeltButton(), ArrowClasses);
            arrow.Add(Styled(new VisualElement { pickingMode = PickingMode.Ignore }, direction));
            arrow.Clicked += clicked;
            return arrow;
        }

        private static T Styled<T>(T element, params string[] styles) where T : VisualElement
        {
            foreach (string style in styles)
                element.AddToClassList(style);
            return element;
        }
    }
}
