using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Le bilan des marchandises (ARCHITECTURE, section 3) : combien en sont nées et combien ont disparu, par
    /// marchandise et par cause. Rien ne naît ni ne disparaît sans y être écrit : ce qui est dans les stocks, les
    /// charges et les chantiers égale ce qui est né moins ce qui a disparu.
    /// </summary>
    public sealed class GoodsLedger
    {
        private readonly List<GoodDefinition> _goods = new List<GoodDefinition>();
        private readonly List<GoodsCause> _causes = new List<GoodsCause>();
        private readonly List<int> _created = new List<int>();
        private readonly List<int> _destroyed = new List<int>();

        /// <summary>Combien de cette marchandise sont nées, toutes causes comprises.</summary>
        public int CreatedOf(GoodDefinition good)
        {
            return SumOf(good, _created);
        }

        /// <summary>Combien de cette marchandise ont disparu, toutes causes comprises.</summary>
        public int DestroyedOf(GoodDefinition good)
        {
            return SumOf(good, _destroyed);
        }

        internal void Created(GoodDefinition good, int amount, GoodsCause cause)
        {
            int line = LineOf(good, cause);
            _created[line] += amount;
        }

        internal void Destroyed(GoodDefinition good, int amount, GoodsCause cause)
        {
            int line = LineOf(good, cause);
            _destroyed[line] += amount;
        }

        private int SumOf(GoodDefinition good, List<int> column)
        {
            int sum = 0;
            for (int i = 0; i < _goods.Count; i++)
            {
                if (ReferenceEquals(_goods[i], good))
                    sum += column[i];
            }
            return sum;
        }

        private int LineOf(GoodDefinition good, GoodsCause cause)
        {
            if (ReferenceEquals(good, null))
                throw new ArgumentNullException(nameof(good));
            for (int i = 0; i < _goods.Count; i++)
            {
                if (ReferenceEquals(_goods[i], good) && _causes[i] == cause)
                    return i;
            }
            _goods.Add(good);
            _causes.Add(cause);
            _created.Add(0);
            _destroyed.Add(0);
            return _goods.Count - 1;
        }
    }
}
