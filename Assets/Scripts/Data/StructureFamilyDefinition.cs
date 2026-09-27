using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Une famille de constructions, un onglet de la barre de construction (GDD, section 16) : son nom, et ses
    /// constructions dans l'ordre où la barre les montre. Une construction n'est que dans une famille.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Famille de constructions", fileName = "Famille")]
    public sealed class StructureFamilyDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Ses constructions, dans l'ordre de la barre.")]
        [SerializeField] private List<StructureDefinition> _structures = new List<StructureDefinition>();

        public string Id => _id;
        public string NameKey => _nameKey;
        public IReadOnlyList<StructureDefinition> Structures => _structures;
    }
}
