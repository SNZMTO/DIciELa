using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.Data
{
    /// <summary>
    /// Une famille de constructions, un onglet de la barre de construction (GDD, section 16) : son nom, son pastel et son
    /// icône, et ses constructions dans l'ordre où la barre les montre. Une construction n'est que dans une famille.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Famille de constructions", fileName = "Famille")]
    public sealed class StructureFamilyDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son pastel : la couleur de son onglet, et de ses cartes en plus pâle.")]
        [SerializeField] private Pastel _pastel = Pastel.Rose;

        [Tooltip("L'icône de son onglet (Assets/UI/Icones).")]
        [SerializeField] private VectorImage _icon;

        [Tooltip("Ses constructions, dans l'ordre de la barre.")]
        [SerializeField] private List<StructureDefinition> _structures = new List<StructureDefinition>();

        public string Id => _id;
        public string NameKey => _nameKey;
        public Pastel Pastel => _pastel;
        public VectorImage Icon => _icon;
        public IReadOnlyList<StructureDefinition> Structures => _structures;
    }
}
