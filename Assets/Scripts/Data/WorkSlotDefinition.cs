using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Un poste (GDD, section 7) : les sortes de tâches qu'on y fait, dans l'ordre où celui qui le tient les choisit. Un
    /// lieu de travail ouvre les siens : sa fiche les liste.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Poste", fileName = "Poste")]
    public sealed class WorkSlotDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Ses sortes de tâches, la plus urgente d'abord : on prend la première qui a quelque chose à faire.")]
        [SerializeField] private List<TaskKindDefinition> _tasks = new List<TaskKindDefinition>();

        public string Id => _id;
        public string NameKey => _nameKey;
        public IReadOnlyList<TaskKindDefinition> Tasks => _tasks;
    }
}
