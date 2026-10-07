using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La Place (GDD, section 11 ; décision 204) : une zone que le joueur trace sur le sol, de la forme qu'il veut, sans
    /// rien coûter, où les charrettes de l'étal se garent sur des marques qu'il pose. Ce que permet chacun de ses
    /// niveaux : sa superficie au plus, et combien de places de charrette ; au départ, le niveau 0.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Place", fileName = "Place")]
    public sealed class PlazaDefinition : ScriptableObject
    {
        [Tooltip("Sa superficie au plus, au départ, en mètres carrés : environ un disque de 24 m.")]
        [SerializeField, Min(1f)] private float _startAreaSquareMeters = 450f;

        [Tooltip("Ce que chaque niveau ajoute à sa superficie au plus, en mètres carrés.")]
        [SerializeField, Min(0f)] private float _areaPerLevelSquareMeters = 150f;

        [Tooltip("Ses places de charrette au départ : une pour les habitants, une pour le troc.")]
        [SerializeField, Min(0)] private int _startCartSpots = 2;

        [Tooltip("Les places de charrette que chaque niveau ajoute.")]
        [SerializeField, Min(0)] private int _cartSpotsPerLevel = 1;

        [Tooltip("Ses niveaux, au-delà du départ ; chacun par un petit chantier (GDD, section 11).")]
        [SerializeField, Min(0)] private int _levels = 3;

        /// <summary>Sa superficie au plus à ce niveau (0 : au départ), en mètres carrés.</summary>
        public float MaxAreaAt(int level)
        {
            return _startAreaSquareMeters + _areaPerLevelSquareMeters * Mathf.Clamp(level, 0, _levels);
        }

        /// <summary>Combien de places de charrette elle a à ce niveau (0 : au départ).</summary>
        public int CartSpotsAt(int level)
        {
            return _startCartSpots + _cartSpotsPerLevel * Mathf.Clamp(level, 0, _levels);
        }
    }
}
