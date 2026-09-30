using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur des fanions du stock (GDD, section 16 ; décision 153) : le stock de la ville, un fanion par
    /// marchandise du catalogue (celles de l'étape en cours : le Bois et la Pierre), qui paraît avec le premier entrepôt
    /// (le tas de départ). Un fanion à zéro reste, en douce. Il écoute les entrepôts : les fanions ne photographient
    /// jamais.
    /// </summary>
    public sealed class StockPennantsPresenter : IDisposable
    {
        private readonly IStockPennantsView _view;
        private readonly World _world;
        private readonly Catalog _catalog;
        private readonly TextTable _texts;
        private readonly List<StockPennant> _pennants = new List<StockPennant>();

        public StockPennantsPresenter(IStockPennantsView view, World world, Catalog catalog, TextTable texts)
        {
            if (view == null || ReferenceEquals(world, null) || catalog == null || ReferenceEquals(texts, null))
                throw new ArgumentException("Les fanions du stock ont besoin de leur vue, du monde, du catalogue et des " +
                                            "textes.");

            _view = view;
            _world = world;
            _catalog = catalog;
            _texts = texts;
            _world.Warehouses.StockChanged += OnStockChanged;
            Refresh();
        }

        public void Dispose()
        {
            _world.Warehouses.StockChanged -= OnStockChanged;
        }

        private void OnStockChanged(Structure warehouse)
        {
            Refresh();
        }

        private void Refresh()
        {
            bool shown = _world.Warehouses.Count > 0;
            _view.ShowStock(shown);
            if (!shown)
                return;
            _pennants.Clear();
            foreach (GoodDefinition good in _catalog.Goods)
            {
                string tooltip = _texts.Get(TextKeys.StockTooltip)
                    .Replace(TextKeys.GoodParameter, _texts.Get(good.NameKey));
                _pennants.Add(new StockPennant(good.Icon, _world.Warehouses.CityStockOf(good), tooltip));
            }
            _view.ShowPennants(_pennants);
        }
    }
}
