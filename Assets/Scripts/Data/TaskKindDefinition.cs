using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.Data
{
    /// <summary>
    /// Une sorte de tâche (GDD, section 7) : porter, bâtir… Ce qu'elle fait, et la sous-statistique qu'elle lit, d'où
    /// vient la valeur d'un habitant pour elle.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Sorte de tâche", fileName = "Tache")]
    public sealed class TaskKindDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son icône (Assets/UI/Icones).")]
        [SerializeField] private VectorImage _icon;

        [Tooltip("Ce qu'elle fait : le geste que le code joue.")]
        [SerializeField] private TaskWork _work = TaskWork.Carry;

        [Tooltip("La sous-statistique qu'elle lit (GDD, section 5).")]
        [SerializeField] private SubStatisticDefinition _subStatistic;

        public string Id => _id;
        public string NameKey => _nameKey;
        public VectorImage Icon => _icon;
        public TaskWork Work => _work;
        public SubStatisticDefinition SubStatistic => _subStatistic;
    }
}
