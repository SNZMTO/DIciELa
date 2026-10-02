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
    /// place, et seul un nombre qui a changé se réécrit : une infobulle ouverte le reste.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class StockPennants : MonoBehaviour, IStockPennantsView
    {
        private const string StockName = "stock";
        private const string PennantsName = "fanions";
        private const string PennantClass = "fanion";
        private const string EmptyClass = "fanion-vide";
        private const string IconClass = "fanion-icone";
        private const string CountClass = "nombre";

        // Le nombre que montre chaque fanion.
        private readonly List<int> _counts = new List<int>();
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
            _counts.Clear();
            ShowStock(false);
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowStock(bool shown)
        {
            _stock.EnableInClassList(StyleClasses.Hidden, !shown);
        }

        public void ShowPennants(IReadOnlyList<StockPennant> pennants)
        {
            if (_row.childCount != pennants.Count)
            {
                _row.Clear();
                _counts.Clear();
                for (int i = 0; i < pennants.Count; i++)
                {
                    _row.Add(NewPennant(pennants[i].Icon));
                    _counts.Add(-1);
                }
            }
            for (int i = 0; i < pennants.Count; i++)
            {
                VisualElement pennant = _row[i];
                pennant.userData = pennants[i].Tooltip;
                int count = pennants[i].Count;
                if (count == _counts[i])
                    continue;
                _counts[i] = count;
                pennant.EnableInClassList(EmptyClass, count == 0);
                ((Label)pennant[1]).text = count.ToString(CultureInfo.InvariantCulture);
            }
        }

        private VisualElement NewPennant(VectorImage image)
        {
            var pennant = new Pennant();
            pennant.AddToClassList(PennantClass);
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList(IconClass);
            if (image != null)
                icon.style.backgroundImage = Background.FromVectorImage(image);
            pennant.Add(icon);
            var count = new Label { pickingMode = PickingMode.Ignore };
            count.AddToClassList(CountClass);
            pennant.Add(count);
            // Un fanion qu'on jette emporte son infobulle : rien à lui retirer.
            _tooltips.Attach(pennant, () => ((string)pennant.userData, ""));
            return pennant;
        }
    }
}
