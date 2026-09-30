using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Un arbre ou un rocher pris par un défricheur (ARCHITECTURE, section 7) : le prendre l'a réservé pour lui seul, et a
    /// retenu à l'entrepôt la place de ce qu'il rendra. Abattu, ce qu'il rend est dans la charge du défricheur ; la tâche
    /// finit rangée à l'entrepôt, ou rendue.
    /// </summary>
    public sealed class Clearing
    {
        internal Clearing(NatureItem item, Structure warehouse, GoodDefinition good, int amount)
        {
            Item = item;
            Warehouse = warehouse;
            Good = good;
            Amount = amount;
        }

        /// <summary>L'arbre ou le rocher à retirer.</summary>
        public NatureItem Item { get; }

        /// <summary>L'entrepôt où ranger ce qu'il rend ; null s'il ne rend rien.</summary>
        public Structure Warehouse { get; }

        /// <summary>Ce qu'il rend ; null s'il ne rend rien.</summary>
        public GoodDefinition Good { get; }

        /// <summary>Combien il en rend ; 0 s'il ne rend rien.</summary>
        public int Amount { get; }

        /// <summary>Vrai une fois l'arbre ou le rocher retiré : ce qu'il rend est dans la charge du défricheur.</summary>
        public bool IsFelled { get; private set; }

        /// <summary>Vrai une fois rangé, ou rendu : la tâche ne réserve plus rien.</summary>
        public bool IsOver { get; private set; }

        internal void MarkFelled()
        {
            IsFelled = true;
        }

        internal void MarkOver()
        {
            IsOver = true;
        }
    }
}
