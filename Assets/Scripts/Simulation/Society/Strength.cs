using Diciela.Data;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Le point fort d'un arrivant (GDD, sections 5 et 6) : une sous-statistique et la valeur où elle commence. Sans point
    /// fort, toutes ses sous-statistiques se tirent entre les bornes de sa sorte.
    /// </summary>
    public readonly struct Strength
    {
        public Strength(SubStatisticDefinition subStatistic, float value)
        {
            SubStatistic = subStatistic;
            Value = value;
        }

        /// <summary>Aucun point fort : la Chimère, qui commence partout pareil.</summary>
        public static Strength None => default;

        /// <summary>La sous-statistique où il est fort ; null s'il n'en a pas.</summary>
        public SubStatisticDefinition SubStatistic { get; }

        /// <summary>La valeur où elle commence.</summary>
        public float Value { get; }
    }
}
