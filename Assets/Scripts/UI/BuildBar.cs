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
    /// La barre de construction, en bas au centre de l'écran (GDD, section 16) : un cadre de bois, les familles en
    /// onglets de feutre, au-dessus les constructions de la famille ouverte en cartes, au bout gauche des onglets le
    /// bouton de l'outil « Retirer », et au-dessus du cadre la raison d'un refus de pose, dans une bulle. L'onglet ouvert,
    /// la carte en main et l'outil en main restent enfoncés. C'est la vue (le bloc « bar » de Hud.uxml) ; son
    /// présentateur décide.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class BuildBar : MonoBehaviour, IBuildBarView
    {
        private const string BarName = "bar";
        private const string FamiliesName = "families";
        private const string StructuresName = "structures";
        private const string ClearingName = "outil-retirer";
        private const string ReasonName = "reason";
        private const string ReasonTextName = "reason-text";
        private const string HiddenClass = "bloc-cache";
        private const string OpenClass = "ouvert";

        private UIDocument _document;
        private VisualElement _bar;
        private VisualElement _families;
        private VisualElement _structures;
        private FeltButton _clearing;
        private VisualElement _reason;
        private Label _reasonText;
        private BuildBarPresenter _presenter;
        private bool _clearingWired;

        public event Action<int> FamilyChosen;
        public event Action<int> StructureChosen;
        public event Action ClearingChosen;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>
        /// Montre la barre, remplie par son présentateur, avec les deux outils qu'elle met en main (poser, retirer) ;
        /// appelé par le démarrage quand la ville s'ouvre.
        /// </summary>
        public void Show(Catalog catalog, TextTable texts, PlacementTool placement, ClearingTool clearing, World world,
            Tooltips tooltips)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            // Tous les éléments d'abord, rangés seulement s'ils sont là : une barre à moitié trouvée ne se cache plus.
            VisualElement bar = root.Q(BarName);
            VisualElement families = root.Q(FamiliesName);
            VisualElement structures = root.Q(StructuresName);
            var clearingButton = root.Q<FeltButton>(ClearingName);
            VisualElement reason = root.Q(ReasonName);
            var reasonText = root.Q<Label>(ReasonTextName);
            if (bar == null || families == null || structures == null || clearingButton == null || reason == null ||
                reasonText == null || tooltips == null)
                throw new InvalidOperationException("La barre de construction ne trouve pas ses éléments (UXML).");

            _bar = bar;
            _families = families;
            _structures = structures;
            _clearing = clearingButton;
            _reason = reason;
            _reasonText = reasonText;
            // Le bouton de l'outil vit dans le document : branché une seule fois, il s'adresse au présentateur du moment.
            if (!_clearingWired)
            {
                _clearing.Clicked += () => ClearingChosen?.Invoke();
                tooltips.Attach(_clearing, () => _presenter != null ? _presenter.ClearingTooltip() : ("", ""), true);
                _clearingWired = true;
            }
            _presenter = new BuildBarPresenter(this, catalog, texts, placement, clearing, world);
            _bar.RemoveFromClassList(HiddenClass);
        }

        /// <summary>Cache la barre et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_bar == null)
                return;
            _bar.AddToClassList(HiddenClass);
            _families.Clear();
            _structures.Clear();
            ShowClearing(false, false);
            ShowReason("");
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowFamilies(IReadOnlyList<BuildBarFamily> families, int open)
        {
            _families.Clear();
            for (int i = 0; i < families.Count; i++)
            {
                int index = i;
                FeltButton tab = BuildBarElements.Tab(families[i], () => FamilyChosen?.Invoke(index));
                tab.EnableInClassList(OpenClass, i == open);
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
                _structures[i].EnableInClassList(OpenClass, i == index);
        }

        public void ShowClearing(bool shown, bool held)
        {
            _clearing.EnableInClassList(HiddenClass, !shown);
            _clearing.EnableInClassList(OpenClass, held);
        }

        public void ShowReason(string text)
        {
            _reasonText.text = text;
            _reason.EnableInClassList(HiddenClass, string.IsNullOrEmpty(text));
        }
    }
}
