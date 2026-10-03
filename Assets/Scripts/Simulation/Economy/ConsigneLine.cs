using Diciela.Data;

namespace Diciela.Simulation.Economy
{
    /// <summary>Comment une ligne de consigne compte ce qu'elle demande à sa charrette (GDD, section 11).</summary>
    public enum ConsigneMode
    {
        /// <summary>Tant de cette marchandise sur la charrette.</summary>
        Amount = 0,

        /// <summary>Tout ce qui dépasse tant de cette marchandise au stock de la ville.</summary>
        KeepAtLeast = 1
    }

    /// <summary>
    /// Une ligne de la consigne d'une place de charrette (GDD, section 11 ; décision 202) : une marchandise, et ce qu'on en
    /// veut sur la charrette, une quantité, ou ce qui dépasse « garder au moins N » au stock de la ville.
    /// </summary>
    public readonly struct ConsigneLine
    {
        public ConsigneLine(GoodDefinition good, ConsigneMode mode, int number)
        {
            Good = good;
            Mode = mode;
            Number = number;
        }

        public GoodDefinition Good { get; }
        public ConsigneMode Mode { get; }

        /// <summary>La quantité voulue, ou ce qu'on garde au moins au stock de la ville.</summary>
        public int Number { get; }
    }
}
