using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le déroulé du départ (GDD, section 6) : ce qui se passe de soi-même au début d'une partie. Quand la Chimère plante
    /// son premier logis, le tas de départ se pose à droite de sa porte, avec son contenu.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Déroulé du départ", fileName = "Depart")]
    public sealed class OpeningDefinition : ScriptableObject
    {
        [Tooltip("Le tas de départ : la construction qui se pose d'elle-même près du premier logis de la Chimère.")]
        [SerializeField] private StructureDefinition _startingPile;

        [Tooltip("Ce que tient le tas de départ quand il se pose.")]
        [SerializeField] private List<GoodAmount> _startingGoods = new List<GoodAmount>();

        [Tooltip("Si sa place est prise, le tas se pose au plus près, mais pas plus loin que ce rayon, en mètres ; " +
                 "au-delà, il ne se pose pas.")]
        [SerializeField, Min(0f)] private float _pileSearchMeters = 16f;

        public StructureDefinition StartingPile => _startingPile;
        public IReadOnlyList<GoodAmount> StartingGoods => _startingGoods;
        public float PileSearchMeters => _pileSearchMeters;
    }
}
