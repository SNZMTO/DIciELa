using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Les statistiques d'un habitant (GDD, section 5) : une valeur par sous-statistique, de 0 à la plus haute. À son
    /// arrivée, chacune se tire entre les bornes de sa sorte, et son point fort commence à sa valeur. La statistique
    /// globale est la moyenne de ses sous-statistiques ; la valeur pour une tâche mêle les deux. Elles ne progressent pas
    /// encore (décision 138).
    /// </summary>
    public sealed class VillagerStatistics
    {
        private readonly StatisticTable _table;
        private readonly float[] _values;

        internal VillagerStatistics(StatisticTable table, VillagerKindDefinition kind, int villagerId, Strength strength)
        {
            if (ReferenceEquals(table, null))
                throw new ArgumentNullException(nameof(table));
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));

            _table = table;
            _values = new float[table.Count];
            float lowest = kind.StartingStatisticMin;
            float spread = Math.Max(0f, kind.StartingStatisticMax - lowest);
            for (int i = 0; i < _values.Length; i++)
            {
                // Un tirage par sous-statistique, du numéro de l'habitant et de sa place, comptée en négatif : jamais la
                // suite d'un pas, qui part de son numéro et d'un numéro de pas positif.
                var random = new HashRandom(villagerId, ~i);
                _values[i] = lowest + (float)random.Unit() * spread;
            }
            if (!ReferenceEquals(strength.SubStatistic, null))
                _values[table.IndexOf(strength.SubStatistic)] = strength.Value;
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
