using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>Ce que le présentateur des fanions du stock attend de sa vue (MVP) : les fanions, et s'ils se voient.</summary>
    public interface IStockPennantsView
    {
        /// <summary>Montre la tringle et ses fanions, ou les cache.</summary>
        void ShowStock(bool shown);

        /// <summary>Un fanion par marchandise, dans l'ordre du catalogue.</summary>
        void ShowPennants(IReadOnlyList<StockPennant> pennants);
    }
}
