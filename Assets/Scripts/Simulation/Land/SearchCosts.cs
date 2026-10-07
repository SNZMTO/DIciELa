using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Ce que coûte un trajet à qui se déplace d'une façon donnée (SAVOIR, 2.1) : le prix d'entrer dans une maille, et le
    /// coût restant estimé, qui ne dépasse jamais le vrai. Un piéton : 1 partout, et sa distance à la boîte des arrivées ;
    /// une charrette : l'inverse de son pas (CartPace), et une estimation qui sait où sont les sols (CartEstimate). La
    /// recherche des trajets les lit, pour une façon à la fois, choisie au début de chaque recherche.
    /// </summary>
    internal sealed class SearchCosts
    {
        private readonly NavGrid _grid;
        private readonly PathGoals _goals;
        private readonly CartPace _pace;
        private readonly CartEstimate _estimate;
        private Mover _mover;

        public SearchCosts(NavGrid grid, PathGoals goals, CartPace pace)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _goals = goals ?? throw new ArgumentNullException(nameof(goals));
            _pace = pace ?? throw new ArgumentNullException(nameof(pace));
            _estimate = new CartEstimate(pace);
        }

        /// <summary>Le pas de la charrette dont on cherche le trajet, pour tendre sa ficelle ; null pour un piéton.</summary>
        public CartPace CartPace => _mover == Mover.Cart ? _pace : null;

        /// <summary>
        /// Une recherche commence, pour qui se déplace ainsi, vers ces arrivées : une charrette lit les prix d'aujourd'hui.
        /// </summary>
        public void Begin(Mover mover, IReadOnlyList<Vector2Int> goals)
        {
            _mover = mover;
            if (mover == Mover.Cart)
                _estimate.Begin(_grid, goals);
        }

        /// <summary>Le prix d'entrer dans la maille à cette place du maillage.</summary>
        public int PriceAt(int index)
        {
            return _mover == Mover.Cart ? _pace.CostAt(index) : 1;
        }

        /// <summary>Le coût restant estimé depuis cette maille (à cette place du maillage) jusqu'à une arrivée.</summary>
        public int Estimate(Vector2Int cell, int index)
        {
            int toGoals = _goals.Estimate(cell);
            return _mover == Mover.Cart ? _estimate.Estimate(index, toGoals) : toGoals;
        }
    }
}
