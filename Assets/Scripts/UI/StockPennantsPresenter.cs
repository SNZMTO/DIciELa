using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur des fanions du stock (GDD, section 16 ; décision 153) : le stock de la ville, un fanion par
    /// marchandise du catalogue (celles de l'étape en cours : le Bois et la Pierre), qui paraît avec le premier entrepôt
    /// (le campement). Un fanion à zéro reste, en douce. Il écoute le stock de la ville, une fois par pas où il change,
    /// quel que soit le nombre d'entrepôts qui ont bougé : les fanions ne photographient jamais. Leurs infobulles se font
    /// une fois.
    /// </summary>
    public sealed class StockPennantsPresenter : IDisposable
    {
        private readonly IStockPennantsView _view;
        private readonly World _world;
        private readonly Catalog _catalog;
        private readonly List<StockPennant> _pennants = new List<StockPennant>();
        private readonly List<string> _tooltips = new List<string>();

        public StockPennantsPresenter(IStockPennantsView view, World world, Catalog catalog, TextTable texts)
        {
            if (view == null || ReferenceEquals(world, null) || catalog == null || ReferenceEquals(texts, null))
                throw new ArgumentException("Les fanions du stock ont besoin de leur vue, du monde, du catalogue et des " +
                                            "textes.");

            _view = view;
            _world = world;
            _catalog = catalog;
            foreach (GoodDefinition good in catalog.Goods)
                _tooltips.Add(texts.Get(TextKeys.StockTooltip).Replace(TextKeys.GoodParameter, texts.Get(good.NameKey)));
            _world.Warehouses.CityStockChanged += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            _world.Warehouses.CityStockChanged -= Refresh;
        }

        private void Refresh()
        {
            bool shown = _world.Warehouses.Count > 0;
            _view.ShowStock(shown);
            if (!shown)
                return;
            _pennants.Clear();
            IReadOnlyList<GoodDefinition> goods = _catalog.Goods;
            for (int i = 0; i < goods.Count; i++)
                _pennants.Add(new StockPennant(goods[i].Icon, _world.Warehouses.CityStockOf(goods[i]), _tooltips[i]));
            _view.ShowPennants(_pennants);
        }
    }
}
