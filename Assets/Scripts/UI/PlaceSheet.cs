using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La fiche du lieu touché, en haut à droite de l'écran (GDD, section 16) : un cadre de bois, le nom du lieu, ce qu'il
    /// tient pour la ville (chaque marchandise dans un creux de lin), son poste et qui le tient, un bouton de feutre menthe
    /// et une croix de feutre sable. C'est la vue (le bloc « fiche » de Hud.uxml) ; son présentateur décide. Échap la ferme
    /// avant tout le reste.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PlaceSheet : MonoBehaviour, IPlaceSheetView, IEscapable
    {
        private const string SheetName = "fiche";
        private const string TitleName = "fiche-titre";
        private const string CloseName = "fiche-fermer";
        private const string StockName = "fiche-stock";
        private const string SlotName = "fiche-poste";
        private const string SlotNameName = "fiche-poste-nom";
        private const string SlotHolderName = "fiche-poste-qui";
        private const string ButtonName = "fiche-bouton";
        private const string ButtonTextName = "fiche-bouton-texte";
        private const string HiddenClass = "bloc-cache";
        private static readonly string[] LineClasses = { "creux", "pan-4", "fiche-ligne" };
        private const string IconClass = "fiche-icone";
        private static readonly string[] LineNameClasses = { "libelle", "fiche-ligne-nom" };
        private const string CountClass = "nombre";

        private UIDocument _document;
        private VisualElement _sheet;
        private Label _title;
        private VisualElement _stock;
        private VisualElement _slot;
        private Label _slotName;
        private Label _slotHolder;
        private FeltButton _button;
        private Label _buttonText;
        private FeltButton _close;
        private PlaceSheetPresenter _presenter;

        public event Action ButtonClicked;
        public event Action CloseClicked;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>Relie la fiche à ce monde, fermée ; appelé par le démarrage quand la ville s'ouvre.</summary>
        public void Show(World world, TextTable texts)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            VisualElement sheet = root.Q(SheetName);
            var title = root.Q<Label>(TitleName);
            var close = root.Q<FeltButton>(CloseName);
            VisualElement stock = root.Q(StockName);
            VisualElement slot = root.Q(SlotName);
            var slotName = root.Q<Label>(SlotNameName);
            var slotHolder = root.Q<Label>(SlotHolderName);
            var button = root.Q<FeltButton>(ButtonName);
            var buttonText = root.Q<Label>(ButtonTextName);
            if (sheet == null || title == null || close == null || stock == null || slot == null || slotName == null ||
                slotHolder == null || button == null || buttonText == null)
                throw new InvalidOperationException("La fiche d'un lieu ne trouve pas ses éléments (UXML).");

            _sheet = sheet;
            _title = title;
            _close = close;
            _stock = stock;
            _slot = slot;
            _slotName = slotName;
            _slotHolder = slotHolder;
            _button = button;
            _buttonText = buttonText;
            _close.Clicked += OnClose;
            _button.Clicked += OnButton;
            _presenter = new PlaceSheetPresenter(this, world, texts);
        }

        /// <summary>On a touché cette construction (null : rien) : elle ouvre sa fiche, ou la ferme.</summary>
        public void Touch(Structure touched)
        {
            _presenter?.Touch(touched);
        }

        /// <summary>Échap ferme la fiche ouverte ; faux si elle l'était déjà.</summary>
        public bool Escape()
        {
            return _presenter != null && _presenter.Close();
        }

        /// <summary>Cache la fiche et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_sheet == null)
                return;
            _close.Clicked -= OnClose;
            _button.Clicked -= OnButton;
            ShowSheet(false);
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowSheet(bool shown)
        {
            _sheet.EnableInClassList(HiddenClass, !shown);
        }

        public void ShowTitle(string title)
        {
            _title.text = title;
        }

        public void ShowStock(IReadOnlyList<PlaceSheetLine> lines)
        {
            _stock.Clear();
            foreach (PlaceSheetLine line in lines)
            {
                var row = new Facet { pickingMode = PickingMode.Ignore };
                foreach (string style in LineClasses)
                    row.AddToClassList(style);
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList(IconClass);
                if (line.Icon != null)
                    icon.style.backgroundImage = Background.FromVectorImage(line.Icon);
                row.Add(icon);
                row.Add(Text(line.Name, LineNameClasses));
                row.Add(Text(line.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), CountClass));
                _stock.Add(row);
            }
            _stock.EnableInClassList(HiddenClass, lines.Count == 0);
        }

        public void ShowSlot(string name, string holder)
        {
            _slotName.text = name;
            _slotHolder.text = holder;
            _slot.EnableInClassList(HiddenClass, string.IsNullOrEmpty(name));
        }

        public void ShowButton(string text)
        {
            _buttonText.text = text;
            _button.EnableInClassList(HiddenClass, string.IsNullOrEmpty(text));
        }

        private void OnClose()
        {
            CloseClicked?.Invoke();
        }

        private void OnButton()
        {
            ButtonClicked?.Invoke();
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
