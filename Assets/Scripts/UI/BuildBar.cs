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
    /// Le panneau Construire, en bas au centre de l'écran (GDD, section 16) : un cadre de bois, les familles en onglets de
    /// feutre, au-dessus les constructions de la famille ouverte en cartes ; au-dessus du cadre, la raison d'un refus de
    /// pose, dans une bulle, qui reste quand le panneau est fermé. L'onglet ouvert et la carte en main restent enfoncés.
    /// La roue des outils l'ouvre et le ferme ; Échap le ferme ; ouvert, les touches 1 à 9 prennent ses cartes. C'est la
    /// vue (le bloc « bar » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class BuildBar : MonoBehaviour, IBuildBarView, IBuildMenu, IEscapable, ICardPicker
    {
        private const string BarName = "bar";
        private const string FamiliesName = "families";
        private const string StructuresName = "structures";
        private const string ReasonName = "reason";
        private const string ReasonTextName = "reason-text";

        private UIDocument _document;
        private VisualElement _bar;
        private VisualElement _families;
        private VisualElement _structures;
        private VisualElement _reason;
        private Label _reasonText;
        private BuildBarPresenter _presenter;

        public event Action<int> FamilyChosen;
        public event Action<int> StructureChosen;
        public event Action OpenChanged;

        /// <summary>Vrai tant que le panneau est ouvert.</summary>
        public bool IsOpen => _presenter != null && _presenter.IsOpen;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>Remplit le panneau, fermé, par son présentateur ; appelé par le démarrage quand la ville s'ouvre.</summary>
        public void Show(Catalog catalog, TextTable texts, PlacementTool placement, World world, IToolKeys keys)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            // Tous les éléments d'abord, rangés seulement s'ils sont là : un panneau à moitié trouvé ne se cache plus.
            VisualElement bar = root.Q(BarName);
            VisualElement families = root.Q(FamiliesName);
            VisualElement structures = root.Q(StructuresName);
            VisualElement reason = root.Q(ReasonName);
            var reasonText = root.Q<Label>(ReasonTextName);
            if (bar == null || families == null || structures == null || reason == null || reasonText == null)
                throw new InvalidOperationException("Le panneau Construire ne trouve pas ses éléments (UXML).");

            _bar = bar;
            _families = families;
            _structures = structures;
            _reason = reason;
            _reasonText = reasonText;
            _presenter = new BuildBarPresenter(this, catalog, texts, placement, world, keys);
        }

        /// <summary>Cache le panneau et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_bar == null)
                return;
            ShowOpen(false);
            _families.Clear();
            _structures.Clear();
            ShowReason("");
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void Open()
        {
            _presenter?.Open();
        }

        public void Close()
        {
            _presenter?.Close();
        }

        public bool Escape()
        {
            return _presenter != null && _presenter.Escape();
        }

        public void TakeCard(int index)
        {
            _presenter?.TakeCard(index);
        }

        public void ShowOpen(bool open)
        {
            _bar.EnableInClassList(StyleClasses.Hidden, !open);
            OpenChanged?.Invoke();
        }

        public void ShowFamilies(IReadOnlyList<BuildBarFamily> families, int open)
        {
            _families.Clear();
            for (int i = 0; i < families.Count; i++)
            {
                int index = i;
                FeltButton tab = BuildBarElements.Tab(families[i], () => FamilyChosen?.Invoke(index));
                tab.EnableInClassList(StyleClasses.Open, i == open);
                _families.Add(tab);
            }
        }

        public void ShowStructures(IReadOnlyList<BuildBarCard> cards)
        {
            _structures.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                int index = i;
                _structures.Add(BuildBarElements.Card(cards[i], () => StructureChosen?.Invoke(index)));
            }
        }

        public void SelectStructure(int index)
        {
            for (int i = 0; i < _structures.childCount; i++)
                _structures[i].EnableInClassList(StyleClasses.Open, i == index);
        }

        public void ShowReason(string text)
        {
            _reasonText.text = text;
            _reason.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(text));
        }
    }
}
