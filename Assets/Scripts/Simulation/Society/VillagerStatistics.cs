using System;
using System.Collections.Generic;
using Diciela.Data;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Les statistiques d'un habitant (GDD, section 5) : une valeur par sous-statistique, de 0 à la plus haute. La
    /// statistique globale est la moyenne de ses sous-statistiques ; la valeur pour une tâche mêle les deux. Elles ne
    /// progressent pas encore (décision 138).
    /// </summary>
    public sealed class VillagerStatistics
    {
        private readonly StatisticTable _table;
        private readonly float[] _values;

        internal VillagerStatistics(StatisticTable table, float startingValue)
        {
            if (ReferenceEquals(table, null))
                throw new ArgumentNullException(nameof(table));

            _table = table;
            _values = new float[table.Count];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = startingValue;
        }

        /// <summary>Sa valeur dans cette sous-statistique.</summary>
        public float ValueOf(SubStatisticDefinition subStatistic)
        {
            return _values[_table.IndexOf(subStatistic)];
        }

        /// <summary>Sa statistique globale : la moyenne des sous-statistiques de cette statistique.</summary>
        public float GlobalOf(StatisticDefinition statistic)
        {
            // Une boucle for : chaque ouvrier la lit à chaque pas de travail, et un foreach sur la liste en lecture seule
            // créerait un objet à chaque fois.
            IReadOnlyList<SubStatisticDefinition> subStatistics = statistic.SubStatistics;
            float sum = 0f;
            for (int i = 0; i < subStatistics.Count; i++)
                sum += ValueOf(subStatistics[i]);
            return subStatistics.Count == 0 ? 0f : sum / subStatistics.Count;
        }

        /// <summary>Sa valeur pour cette sorte de tâche : sa sous-statistique, mêlée de sa statistique globale.</summary>
        public float ValueFor(TaskKindDefinition task)
        {
            int index = _table.IndexOf(task.SubStatistic);
            float global = GlobalOf(_table.StatisticAt(index));
            return _table.SubStatisticShare * _values[index] + (1f - _table.SubStatisticShare) * global;
        }

        /// <summary>Ce qu'il apporte au travail de cette tâche, en fois ce qu'apporte un débutant.</summary>
        public float WorkRateFor(TaskKindDefinition task)
        {
            return _table.WorkRateOf(ValueFor(task));
        }
    }
}
