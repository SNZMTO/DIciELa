using System;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>Une quantité d'une marchandise : un coût, un contenu de départ.</summary>
    [Serializable]
    public struct GoodAmount
    {
        [Tooltip("La marchandise.")]
        [SerializeField] private GoodDefinition _good;

        [Tooltip("Combien.")]
        [SerializeField, Min(1)] private int _amount;

        public GoodAmount(GoodDefinition good, int amount)
        {
            _good = good;
            _amount = amount;
        }

        public GoodDefinition Good => _good;
        public int Amount => _amount;
    }
}
