using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le déroulé du départ (GDD, section 6) : ce qui se passe de soi-même au début d'une partie. La Chimère arrive en
    /// sachant déjà bâtir quelques constructions ; quand elle installe son campement, son premier logis, il reçoit le
    /// stock de départ ; puis les premiers habitants arrivent, un par un, à mesure que les lits le permettent.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Déroulé du départ", fileName = "Depart")]
    public sealed class OpeningDefinition : ScriptableObject
    {
        [Tooltip("Ce que la Chimère sait bâtir en arrivant, et rien d'autre (GDD, section 6) ; les idées s'y ajouteront.")]
        [SerializeField] private List<StructureDefinition> _knownStructures = new List<StructureDefinition>();

        [Tooltip("Le stock de départ, que reçoit le premier logis de la Chimère quand on l'installe.")]
        [SerializeField] private List<GoodAmount> _startingGoods = new List<GoodAmount>();

        [Tooltip("Les premiers habitants, dans l'ordre de leur arrivée (GDD, section 6) : un par statistique.")]
        [SerializeField] private List<FirstVillagerDefinition> _firstVillagers = new List<FirstVillagerDefinition>();

        [Tooltip("La valeur de départ d'un premier habitant dans la sous-statistique de son métier (GDD, section 6).")]
        [SerializeField, Min(0f)] private float _firstVillagerStrength = 40f;

        [Tooltip("Les heures de jeu au moins entre deux premières arrivées (GDD, section 6).")]
        [SerializeField, Min(0f)] private float _firstArrivalHours = 1f;

        public IReadOnlyList<StructureDefinition> KnownStructures => _knownStructures;
        public IReadOnlyList<GoodAmount> StartingGoods => _startingGoods;
        public IReadOnlyList<FirstVillagerDefinition> FirstVillagers => _firstVillagers;
        public float FirstVillagerStrength => _firstVillagerStrength;
        public float FirstArrivalHours => _firstArrivalHours;
    }
}
