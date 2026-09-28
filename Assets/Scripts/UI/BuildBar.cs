using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La barre de construction, en bas de l'écran (GDD, section 16) : les familles en onglets, les constructions de la
    /// famille ouverte, et au-dessus la raison d'un refus de pose. C'est la vue (UI Toolkit, BarreConstruction.uxml) ;
    /// son présentateur décide. Elle dit aussi aux gestes si le pointeur est sur elle.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class BuildBar : MonoBehaviour, IBuildBarView, IInterfaceArea
    {
        private const string BarName = "bar";
        private const string FamiliesName = "families";
        private const string StructuresName = "structures";
        private const string ReasonName = "reason";
        private const string HiddenBarClass = "bar--hidden";
        private const string HiddenReasonClass = "reason--hidden";
        private const string ButtonClass = "button";
        private const string SelectedClass = "button--selected";

        private UIDocument _document;
        private VisualElement _bar;
        private VisualElement _families;
        private VisualElement _structures;
        private Label _reason;
        private BuildBarPresenter _presenter;

        public event Action<int> FamilyChosen;
        public event Action<int> StructureChosen;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>Montre la barre, remplie par son présentateur ; appelé par le démarrage quand le monde naît.</summary>
        public void Show(Catalog catalog, TextTable texts, PlacementTool tool, World world)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            // Tous les éléments d'abord, rangés seulement s'ils sont là : une barre à moitié trouvée ne se cache plus.
            VisualElement bar = root.Q(BarName);
            VisualElement families = root.Q(FamiliesName);
            VisualElement structures = root.Q(StructuresName);
            var reason = root.Q<Label>(ReasonName);
            if (bar == null || families == null || structures == null || reason == null)
                throw new InvalidOperationException("La barre de construction ne trouve pas ses éléments (UXML).");

            _bar = bar;
            _families = families;
            _structures = structures;
            _reason = reason;
            _presenter = new BuildBarPresenter(this, catalog, texts, tool, world);
            _bar.RemoveFromClassList(HiddenBarClass);
        }

        /// <summary>Cache la barre et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_bar == null)
                return;
            _bar.AddToClassList(HiddenBarClass);
            _families.Clear();
            _structures.Clear();
            ShowReason("");
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowFamilies(IReadOnlyList<string> names, int open)
        {
            Fill(_families, names, index => FamilyChosen?.Invoke(index));
            Select(_families, open);
        }

        public void ShowStructures(IReadOnlyList<string> names)
        {
            Fill(_structures, names, index => StructureChosen?.Invoke(index));
        }

        public void SelectStructure(int index)
        {
            Select(_structures, index);
        }

        public void ShowReason(string text)
        {
            _reason.text = text;
            _reason.EnableInClassList(HiddenReasonClass, string.IsNullOrEmpty(text));
        }

        /// <summary>
        /// Vrai si le pointeur est sur un élément de la barre. Le pointeur compte depuis le bas de l'écran, UI Toolkit
        /// depuis le haut : on retourne l'axe vertical avant de chercher l'élément (SAVOIR, section 3).
        /// </summary>
        public bool Contains(Vector2 screenPoint)
        {
            IPanel panel = _document != null ? _document.rootVisualElement?.panel : null;
            if (panel == null || _bar == null || _bar.ClassListContains(HiddenBarClass))
                return false;
            var fromTop = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            VisualElement picked = panel.Pick(RuntimePanelUtils.ScreenToPanel(panel, fromTop));
            return picked != null && (picked == _bar || _bar.Contains(picked));
        }

        private static void Fill(VisualElement row, IReadOnlyList<string> names, Action<int> chosen)
        {
            row.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                int index = i;
                var button = new Button(() => chosen(index)) { text = names[i] };
                button.AddToClassList(ButtonClass);
                row.Add(button);
            }
        }

        private static void Select(VisualElement row, int selected)
        {
            for (int i = 0; i < row.childCount; i++)
                row[i].EnableInClassList(SelectedClass, i == selected);
        }
    }
}
