using Diciela.Data;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// Le déroulé du départ (GDD, section 6 ; décision 172) : quand la Chimère installe son premier logis, le campement,
    /// qui tient les marchandises de la ville, il reçoit le stock de départ, né du départ. Une seule fois par partie : le
    /// bilan des marchandises le dit, rien d'autre ne le retient. Un catalogue sans déroulé du départ n'en donne pas.
    /// </summary>
    internal sealed class Opening
    {
        private readonly OpeningDefinition _definition;

        public Opening(OpeningDefinition definition)
        {
            _definition = definition;
        }

        /// <summary>
        /// Cette construction vient d'être posée : si c'est un logis de la Chimère qui tient des marchandises, et que le
        /// stock de départ n'a pas encore été donné, elle le reçoit.
        /// </summary>
        public void Welcome(Structure placed, Warehouses warehouses, GoodsLedger ledger)
        {
            if (ReferenceEquals(_definition, null) || !placed.Definition.IsChimeraHome ||
                warehouses.StockOf(placed) == null || ledger.CreatedBy(GoodsCause.StartingStock) > 0)
                return;
            foreach (GoodAmount goods in _definition.StartingGoods)
                warehouses.Create(placed, goods.Good, goods.Amount, GoodsCause.StartingStock);
        }
    }
}
