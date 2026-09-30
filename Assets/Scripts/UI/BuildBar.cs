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
    /// onglets de feutre (leur pastel, leur icône), au-dessus les constructions de la famille ouverte en cartes (leur
    /// vignette, leur nom), et au-dessus du cadre la raison d'un refus de pose, dans une bulle. L'onglet ouvert et la
    /// carte en main restent enfoncés. C'est la vue (le bloc « bar » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class BuildBar : MonoBehaviour, IBuildBarView
    {
        private const string BarName = "bar";
        private const string FamiliesName = "families";
        private const string StructuresName = "structures";
        private const string ReasonName = "reason";
        private const string ReasonTextName = "reason-text";
        private const string HiddenClass = "bloc-cache";
        private const string OpenClass = "ouvert";
        private const string PaleSuffix = "-pale";
        private static readonly string[] TabClasses = { "feutre", "pan-10", "couture", "onglet" };
        private static readonly string[] CardClasses = { "feutre", "pan-10", "carte" };
        private const string TabIconClass = "onglet-icone";
        private const string TabNameClass = "action";
        private const string CardPictureClass = "carte-vignette";
        private static readonly string[] CardNameClasses = { "t2", "carte-nom" };
        private static readonly string[] CostClasses = { "cout", "carte-cout" };
        private const string CostLineClass = "carte-cout-ligne";
        private const string CostIconClass = "carte-cout-icone";

        private UIDocument _document;
        private VisualElement _bar;
        private VisualElement _families;
        private VisualElement _structures;
        private VisualElement _reason;
        private Label _reasonText;
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
            // Tous les éléments d'abord, rangés seulement s'ils sont là : une barre à moitié trouvée ne se cache plus.
            VisualElement bar = root.Q(BarName);
            VisualElement families = root.Q(FamiliesName);
            VisualElement structures = root.Q(StructuresName);
            VisualElement reason = root.Q(ReasonName);
            var reasonText = root.Q<Label>(ReasonTextName);
            if (bar == null || families == null || structures == null || reason == null || reasonText == null)
                throw new InvalidOperationException("La barre de construction ne trouve pas ses éléments (UXML).");

            _bar = bar;
            _families = families;
            _structures = structures;
            _reason = reason;
            _reasonText = reasonText;
            _presenter = new BuildBarPresenter(this, catalog, texts, tool, world);
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
                FeltButton tab = Button(TabClasses, Pastels.ClassOf(families[i].Pastel),
                    () => FamilyChosen?.Invoke(index));
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList(TabIconClass);
                if (families[i].Icon != null)
                    icon.style.backgroundImage = Background.FromVectorImage(families[i].Icon);
                tab.Add(icon);
                tab.Add(Text(families[i].Name, TabNameClass));
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
                FeltButton card = Button(CardClasses, Pastels.ClassOf(cards[i].Pastel) + PaleSuffix,
                    () => StructureChosen?.Invoke(index));
                if (cards[i].Picture != null)
                {
                    var picture = new VisualElement { pickingMode = PickingMode.Ignore };
                    picture.AddToClassList(CardPictureClass);
                    picture.style.backgroundImage = new StyleBackground(cards[i].Picture);
                    card.Add(picture);
                }
                card.Add(Text(cards[i].Name, CardNameClasses));
                if (cards[i].Cost != null && cards[i].Cost.Count > 0)
                    card.Add(Cost(cards[i].Cost));
                _structures.Add(card);
            }
        }

        public void SelectStructure(int index)
        {
            for (int i = 0; i < _structures.childCount; i++)
                _structures[i].EnableInClassList(OpenClass, i == index);
        }

        public void ShowReason(string text)
        {
            _reasonText.text = text;
            _reason.EnableInClassList(HiddenClass, string.IsNullOrEmpty(text));
        }

        /// <summary>Le coût d'une carte : chaque marchandise, son icône et combien.</summary>
        private static VisualElement Cost(IReadOnlyList<GoodAmount> cost)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            foreach (string style in CostClasses)
                row.AddToClassList(style);
            foreach (GoodAmount goods in cost)
            {
                var line = new VisualElement { pickingMode = PickingMode.Ignore };
                line.AddToClassList(CostLineClass);
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList(CostIconClass);
                if (goods.Good != null && goods.Good.Icon != null)
                    icon.style.backgroundImage = Background.FromVectorImage(goods.Good.Icon);
                line.Add(icon);
                line.Add(Text(goods.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                row.Add(line);
            }
            return row;
        }

        private static FeltButton Button(string[] classes, string pastel, Action clicked)
        {
            var button = new FeltButton();
            foreach (string style in classes)
                button.AddToClassList(style);
            button.AddToClassList(pastel);
            button.Clicked += clicked;
            return button;
        }

        private static Label Text(string text, params string[] classes)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            foreach (string style in classes)
                label.AddToClassList(style);
            return label;
        }
    }
}
