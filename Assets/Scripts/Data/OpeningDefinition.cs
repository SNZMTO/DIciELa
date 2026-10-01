using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le déroulé du départ (GDD, section 6) : ce qui se passe de soi-même au début d'une partie. Quand la Chimère installe
    /// son campement, son premier logis, il reçoit le stock de départ.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Déroulé du départ", fileName = "Depart")]
    public sealed class OpeningDefinition : ScriptableObject
    {
        [Tooltip("Le stock de départ, que reçoit le premier logis de la Chimère quand on l'installe.")]
        [SerializeField] private List<GoodAmount> _startingGoods = new List<GoodAmount>();

        public IReadOnlyList<GoodAmount> StartingGoods => _startingGoods;
    }
}
