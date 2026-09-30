using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Une livraison prise par un porteur (ARCHITECTURE, section 7) : des marchandises d'un entrepôt, promises à un
    /// chantier. La prendre les a réservées des deux côtés ; elle finit livrée, ou rendue.
    /// </summary>
    public sealed class Delivery
    {
        internal Delivery(Structure source, ConstructionSite site, GoodDefinition good, int amount)
        {
            Source = source;
            Site = site;
            Good = good;
            Amount = amount;
        }

        /// <summary>L'entrepôt où prendre les marchandises.</summary>
        public Structure Source { get; }

        /// <summary>Le chantier où les livrer.</summary>
        public ConstructionSite Site { get; }

        public GoodDefinition Good { get; }
        public int Amount { get; }

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
