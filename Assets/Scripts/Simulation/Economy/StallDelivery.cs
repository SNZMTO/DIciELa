using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Ce qu'un porteur prend pour l'étal (ARCHITECTURE, section 7 ; décision 202) : garnir une charrette garée, d'un
    /// entrepôt jusqu'à elle, ou en rapporter le surplus, d'elle jusqu'à un entrepôt. Le prendre a réservé des deux côtés :
    /// les marchandises à la source, la place à l'arrivée ; elle finit livrée, ou rendue.
    /// </summary>
    public sealed class StallDelivery
    {
        internal StallDelivery(Cart cart, Structure warehouse, GoodDefinition good, int amount, bool toCart,
            Vector2Int cartCell)
        {
            Cart = cart;
            Warehouse = warehouse;
            Good = good;
            Amount = amount;
            ToCart = toCart;
            CartCell = cartCell;
        }

        public Cart Cart { get; }

        /// <summary>L'entrepôt d'où l'on garnit la charrette, ou où l'on rapporte son surplus.</summary>
        public Structure Warehouse { get; }

        public GoodDefinition Good { get; }
        public int Amount { get; }

        /// <summary>Vrai pour garnir la charrette ; faux pour en rapporter le surplus.</summary>
        public bool ToCart { get; }

        /// <summary>La maille devant la charrette, au bout de son timon : on y prend, on y dépose.</summary>
        public Vector2Int CartCell { get; }

        /// <summary>Vrai une fois les marchandises prises : le porteur les a dans sa charge.</summary>
        public bool IsPickedUp { get; private set; }

        /// <summary>Vrai une fois livrée ou rendue : elle ne réserve plus rien.</summary>
        public bool IsOver { get; private set; }

        internal void MarkPickedUp()
        {
            IsPickedUp = true;
        }

        internal void MarkOver()
        {
            IsOver = true;
        }
    }
}
