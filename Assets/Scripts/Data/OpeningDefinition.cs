using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le déroulé du départ (GDD, section 6) : ce qui se passe de soi-même au début d'une partie. La Chimère arrive en
    /// sachant déjà bâtir quelques constructions ; quand elle installe son campement, son premier logis, il reçoit le
    /// stock de départ.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Déroulé du départ", fileName = "Depart")]
    public sealed class OpeningDefinition : ScriptableObject
    {
        [Tooltip("Ce que la Chimère sait bâtir en arrivant, et rien d'autre (GDD, section 6) ; les idées s'y ajouteront.")]
        [SerializeField] private List<StructureDefinition> _knownStructures = new List<StructureDefinition>();

        [Tooltip("Le stock de départ, que reçoit le premier logis de la Chimère quand on l'installe.")]
        [SerializeField] private List<GoodAmount> _startingGoods = new List<GoodAmount>();

        public IReadOnlyList<StructureDefinition> KnownStructures => _knownStructures;
        public IReadOnlyList<GoodAmount> StartingGoods => _startingGoods;
    }
}
