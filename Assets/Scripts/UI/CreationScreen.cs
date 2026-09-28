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
    /// L'écran de création de la Chimère (GDD, section 6), un panneau sur le côté, la ville visible derrière : son titre,
    /// une ligne par choix avec ses flèches, et le bouton qui valide. C'est la vue (UI Toolkit, Creation.uxml) ; son
    /// présentateur décide. Elle dit aussi aux gestes si le pointeur est sur elle.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CreationScreen : MonoBehaviour, ICreationView, IInterfaceArea
    {
        private const string PanelName = "panel";
        private const string TitleName = "title";
        private const string RowsName = "rows";
        private const string ConfirmName = "confirm";
        private const string HiddenPanelClass = "panel--hidden";
        private const string RowClass = "row";
        private const string LabelClass = "row-label";
        private const string ArrowClass = "arrow";
        private const string ChoiceClass = "choice";

        private readonly List<Label> _choices = new List<Label>();
        private UIDocument _document;
        private VisualElement _panel;
        private Label _title;
        private VisualElement _rows;
        private Button _confirm;
        private CreationPresenter _presenter;

        public event Action<int, int> Stepped;
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
            var confirm = root.Q<Button>(ConfirmName);
            if (panel == null || title == null || rows == null || confirm == null)
                throw new InvalidOperationException("L'écran de création ne trouve pas ses éléments (UXML).");

            _panel = panel;
            _title = title;
            _rows = rows;
            _confirm = confirm;
            _confirm.clicked += Confirm;
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
            _confirm.clicked -= Confirm;
            _panel.AddToClassList(HiddenPanelClass);
            _rows.Clear();
            _choices.Clear();
        }

        /// <summary>Valide le personnage : le bouton, ou Entrée par le geste « valider ».</summary>
        public void Confirm()
        {
            Confirmed?.Invoke();
        }

        public void ShowRows(string title, string confirm, string previous, string next,
            IReadOnlyList<string> labels)
        {
            _title.text = title;
            _confirm.text = confirm;
            _rows.Clear();
            _choices.Clear();
            for (int i = 0; i < labels.Count; i++)
            {
                int row = i;
                var line = new VisualElement();
                line.AddToClassList(RowClass);
                line.Add(Styled(new Label(labels[i]), LabelClass));
                line.Add(Styled(new Button(() => Stepped?.Invoke(row, -1)) { text = previous }, ArrowClass));
                var choice = Styled(new Label(), ChoiceClass);
                line.Add(choice);
                line.Add(Styled(new Button(() => Stepped?.Invoke(row, 1)) { text = next }, ArrowClass));
                _choices.Add(choice);
                _rows.Add(line);
            }
        }

        public void ShowChoice(int row, string choice)
        {
            if (row >= 0 && row < _choices.Count)
                _choices[row].text = choice;
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

        private static T Styled<T>(T element, string style) where T : VisualElement
        {
            element.AddToClassList(style);
            return element;
        }
    }
}
