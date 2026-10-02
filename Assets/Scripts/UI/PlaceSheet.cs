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
    /// tient pour la ville (chaque marchandise dans un creux de lin), ses postes et qui les tient, ses boutons de feutre
    /// (chacun dans son pastel) et une croix de feutre sable. C'est la vue (le bloc « fiche » de Hud.uxml) ; son
    /// présentateur décide. Échap la ferme avant tout le reste. Ses lignes et ses boutons se refont seulement quand leur
    /// nombre change, sinon ils se remplissent sur place (le stock bouge à chaque charge prise) : un clic pendant que le
    /// stock bouge tombe sur un bouton vivant.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PlaceSheet : MonoBehaviour, IPlaceSheetView, IEscapable
    {
        private const string SheetName = "fiche";
        private const string TitleName = "fiche-titre";
        private const string CloseName = "fiche-fermer";
        private const string StockName = "fiche-stock";
        private const string SlotsName = "fiche-poste";
        private const string ActionsName = "fiche-actions";

        private readonly List<Label> _actionTexts = new List<Label>();
        private readonly List<string> _actionPastels = new List<string>();
        private UIDocument _document;
        private VisualElement _sheet;
        private Label _title;
        private VisualElement _stock;
        private VisualElement _slots;
        private VisualElement _actions;
        private FeltButton _close;
        private PlaceSheetPresenter _presenter;

        public event Action<int> ActionClicked;
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
            VisualElement slots = root.Q(SlotsName);
            VisualElement actions = root.Q(ActionsName);
            if (sheet == null || title == null || close == null || stock == null || slots == null || actions == null)
                throw new InvalidOperationException("La fiche d'un lieu ne trouve pas ses éléments (UXML).");

            _sheet = sheet;
            _title = title;
            _close = close;
            _stock = stock;
            _slots = slots;
            _actions = actions;
            _close.Clicked += OnClose;
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

        /// <summary>Cache la fiche, jette ses boutons (et leurs rappels) et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_sheet == null)
                return;
            _close.Clicked -= OnClose;
            _actions.Clear();
            _actionTexts.Clear();
            _actionPastels.Clear();
            ShowSheet(false);
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowSheet(bool shown)
        {
            _sheet.EnableInClassList(StyleClasses.Hidden, !shown);
        }

        public void ShowTitle(string title)
        {
            _title.text = title;
        }

        public void ShowStock(IReadOnlyList<PlaceSheetLine> lines)
        {
            if (_stock.childCount != lines.Count)
            {
                _stock.Clear();
                foreach (PlaceSheetLine line in lines)
                    _stock.Add(PlaceSheetElements.Line(line));
            }
            else
            {
                for (int i = 0; i < lines.Count; i++)
                    PlaceSheetElements.Refill(_stock[i], lines[i]);
            }
            _stock.EnableInClassList(StyleClasses.Hidden, lines.Count == 0);
        }

        public void ShowSlots(IReadOnlyList<PlaceSheetSlot> slots)
        {
            PlaceSheetElements.ShowSlots(_slots, slots);
            _slots.EnableInClassList(StyleClasses.Hidden, slots.Count == 0);
        }

        public void ShowActions(IReadOnlyList<PlaceSheetAction> actions)
        {
            if (_actionTexts.Count != actions.Count)
            {
                _actions.Clear();
                _actionTexts.Clear();
                _actionPastels.Clear();
                for (int i = 0; i < actions.Count; i++)
                {
                    int index = i;
                    // Un bouton qu'on jette emporte son rappel.
                    _actions.Add(PlaceSheetElements.Button(() => ActionClicked?.Invoke(index), out Label text));
                    _actionTexts.Add(text);
                    _actionPastels.Add("");
                }
            }
            for (int i = 0; i < actions.Count; i++)
            {
                string pastel = Pastels.ClassOf(actions[i].Pastel);
                if (pastel != _actionPastels[i])
                {
                    _actions[i].RemoveFromClassList(_actionPastels[i]);
                    _actions[i].AddToClassList(pastel);
                    _actionPastels[i] = pastel;
                }
                _actionTexts[i].text = actions[i].Text;
            }
            _actions.EnableInClassList(StyleClasses.Hidden, actions.Count == 0);
        }

        private void OnClose()
        {
            CloseClicked?.Invoke();
        }
    }
}
