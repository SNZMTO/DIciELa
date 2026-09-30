using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Une sous-statistique (GDD, section 5) : une discipline d'une statistique (la construction, le transport…). C'est
    /// la sorte de tâche qui dit laquelle elle lit ; sa statistique est celle qui la liste.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Sous-statistique", fileName = "SousStatistique")]
    public sealed class SubStatisticDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        public string Id => _id;
        public string NameKey => _nameKey;
    }
}
