using System;
using System.Collections.Generic;
using System.Globalization;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Le stock de la ville en fanions, en haut au centre de l'écran (GDD, section 16) : une tringle de bois et ses
    /// pommeaux, un fanion de lin par marchandise (son icône, son nombre), chacun avec son infobulle. C'est la vue (le bloc
    /// « stock » de Hud.uxml) ; son présentateur décide. Quand les nombres changent, les fanions se mettent à jour sur
    /// place : une infobulle ouverte le reste.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class StockPennants : MonoBehaviour, IStockPennantsView
    {
        private const string StockName = "stock";
        private const string PennantsName = "fanions";
        private const string HiddenClass = "bloc-cache";
        private const string PennantClass = "fanion";
        private const string EmptyClass = "fanion-vide";
        private const string IconClass = "fanion-icone";
        private const string CountClass = "nombre";

        private UIDocument _document;
        private VisualElement _stock;
        private VisualElement _row;
        private Tooltips _tooltips;
        private StockPennantsPresenter _presenter;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>Montre le stock de ce monde ; appelé par le démarrage quand la ville s'ouvre.</summary>
        public void Show(World world, Catalog catalog, TextTable texts, Tooltips tooltips)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            VisualElement stock = root.Q(StockName);
            VisualElement row = root.Q(PennantsName);
            if (stock == null || row == null || tooltips == null)
                throw new InvalidOperationException("Les fanions du stock ne trouvent pas leurs éléments (UXML).");

            _stock = stock;
            _row = row;
            _tooltips = tooltips;
            _presenter = new StockPennantsPresenter(this, world, catalog, texts);
        }

        /// <summary>Cache le stock et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            if (_stock == null)
                return;
            _row.Clear();
            ShowStock(false);
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowStock(bool shown)
        {
            _stock.EnableInClassList(HiddenClass, !shown);
        }

        public void ShowPennants(IReadOnlyList<StockPennant> pennants)
        {
            if (_row.childCount != pennants.Count)
            {
                _row.Clear();
                for (int i = 0; i < pennants.Count; i++)
                    _row.Add(NewPennant());
            }
            for (int i = 0; i < pennants.Count; i++)
            {
                VisualElement pennant = _row[i];
                pennant.userData = pennants[i].Tooltip;
                pennant.EnableInClassList(EmptyClass, pennants[i].Count == 0);
                VisualElement icon = pennant[0];
                if (pennants[i].Icon != null)
                    icon.style.backgroundImage = Background.FromVectorImage(pennants[i].Icon);
                ((Label)pennant[1]).text = pennants[i].Count.ToString(CultureInfo.InvariantCulture);
            }
        }

        private VisualElement NewPennant()
        {
            var pennant = new Pennant();
            pennant.AddToClassList(PennantClass);
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList(IconClass);
            pennant.Add(icon);
            var count = new Label { pickingMode = PickingMode.Ignore };
            count.AddToClassList(CountClass);
            pennant.Add(count);
            _tooltips.Attach(pennant, () => ((string)pennant.userData, ""));
            return pennant;
        }
    }
}
