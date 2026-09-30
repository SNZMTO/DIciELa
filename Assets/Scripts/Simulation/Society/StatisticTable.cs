using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Les statistiques de la ville et le calcul de la valeur pour une tâche (GDD, sections 5 et 7) : chaque
    /// sous-statistique, sa place et sa statistique ; la valeur pour une tâche mêle la sous-statistique qu'elle lit et la
    /// statistique globale (la moyenne de ses sous-statistiques) ; l'apport au travail croît en ligne droite avec elle.
    /// Déduite des fiches à la naissance du monde, jamais sauvegardée.
    /// </summary>
    public sealed class StatisticTable
    {
        private readonly List<SubStatisticDefinition> _subStatistics = new List<SubStatisticDefinition>();
        private readonly List<StatisticDefinition> _parents = new List<StatisticDefinition>();

        public StatisticTable(IReadOnlyList<StatisticDefinition> statistics, SimulationSettings settings)
        {
            if (statistics == null)
                throw new ArgumentNullException(nameof(statistics));
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));

            foreach (StatisticDefinition statistic in statistics)
            {
                foreach (SubStatisticDefinition subStatistic in statistic.SubStatistics)
                {
                    if (_subStatistics.Contains(subStatistic))
                        throw new ArgumentException("La sous-statistique " + subStatistic.Id + " est dans deux " +
                                                    "statistiques.", nameof(statistics));
                    _subStatistics.Add(subStatistic);
                    _parents.Add(statistic);
                }
            }
            Scale = settings.StatisticScale;
            SubStatisticShare = settings.SubStatisticShare;
            ExpertWorkRate = settings.ExpertWorkRate;
        }

        /// <summary>Le nombre de sous-statistiques, toutes statistiques comprises.</summary>
        public int Count => _subStatistics.Count;

        /// <summary>La plus haute valeur d'une statistique.</summary>
        public float Scale { get; }

        /// <summary>La part de la sous-statistique dans la valeur pour une tâche.</summary>
        public float SubStatisticShare { get; }

        /// <summary>Ce qu'apporte un expert, en fois ce qu'apporte un débutant.</summary>
        public float ExpertWorkRate { get; }

        /// <summary>La place de cette sous-statistique ; une erreur si aucune statistique ne la liste.</summary>
        public int IndexOf(SubStatisticDefinition subStatistic)
        {
            int index = _subStatistics.IndexOf(subStatistic);
            if (index < 0)
                throw new ArgumentException("Aucune statistique du catalogue ne liste la sous-statistique " +
                                            (ReferenceEquals(subStatistic, null) ? "(aucune)" : subStatistic.Id) + ".");
            return index;
        }

        /// <summary>La statistique de la sous-statistique à cette place.</summary>
        public StatisticDefinition StatisticAt(int index)
        {
            return _parents[index];
        }

        /// <summary>L'apport au travail d'une valeur pour une tâche : ×1 à 0, l'apport d'un expert à la plus haute.</summary>
        public float WorkRateOf(float value)
        {
            return 1f + (ExpertWorkRate - 1f) * value / Scale;
        }
    }
}
