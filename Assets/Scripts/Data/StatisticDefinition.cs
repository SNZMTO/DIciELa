using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Une statistique (GDD, section 5) : Terre, Bâtisse, Artisanat, Échange, et ses sous-statistiques, une par
    /// discipline. La statistique globale d'un habitant est la moyenne de ses sous-statistiques.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Statistique", fileName = "Statistique")]
    public sealed class StatisticDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Ses sous-statistiques, dans l'ordre où la fiche d'un habitant les montrera ; chacune n'est que dans " +
                 "une statistique.")]
        [SerializeField] private List<SubStatisticDefinition> _subStatistics = new List<SubStatisticDefinition>();

        public string Id => _id;
        public string NameKey => _nameKey;
        public IReadOnlyList<SubStatisticDefinition> SubStatistics => _subStatistics;
    }
}
