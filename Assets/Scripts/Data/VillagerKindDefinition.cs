using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche d'une sorte d'habitant : ordinaire, maître ou la Chimère (ARCHITECTURE, section 9). Ce qui distingue
    /// une sorte d'une autre est ici, jamais dans le code par son nom : sa marche, sa flânerie, la vitesse à laquelle il
    /// se tourne à l'écran. Son allure est à chaque habitant : son apparence (corps, couleur, pièces).
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Sorte d'habitant", fileName = "SorteHabitant")]
    public sealed class VillagerKindDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("Vitesse de marche, en mètres par seconde de temps réel : le pas naturel à l'écran (GDD, section 3).")]
        [SerializeField, Min(0.1f)] private float _walkMetersPerSecond = 1.5f;

        [Tooltip("Sans poste, on flâne : on va à un point libre tiré à ce rayon au plus, en mètres, de l'endroit où l'on " +
                 "s'est mis à flâner (GDD, section 7).")]
        [SerializeField, Min(1f)] private float _wanderRadiusMeters = 6f;

        [Tooltip("Un point de flânerie est à au moins cette distance de soi, en mètres : plus près, le saut ne se verrait " +
                 "pas, comme un tremblement (GDD, section 7).")]
        [SerializeField, Min(0f)] private float _wanderMinHopMeters = 2f;

        [Tooltip("La pause la plus courte entre deux points de flânerie, en heures de jeu.")]
        [SerializeField, Min(0f)] private float _wanderPauseMinHours = 0.1f;

        [Tooltip("La pause la plus longue entre deux points de flânerie, en heures de jeu.")]
        [SerializeField, Min(0f)] private float _wanderPauseMaxHours = 0.3f;

        [Tooltip("La vitesse à laquelle le personnage se tourne vers où il va, à l'écran, en degrés par seconde.")]
        [SerializeField, Min(1f)] private float _turnDegreesPerSecond = 540f;

        [Tooltip("La valeur de départ de chacune de ses sous-statistiques (GDD, sections 5 et 6).")]
        [SerializeField, Min(0f)] private float _startingStatistic = 10f;

        [Tooltip("Les marchandises qu'il porte à pied en un voyage (GDD, section 6).")]
        [SerializeField, Min(1)] private int _carryCapacity = 5;

        public string Id => _id;
        public float WalkMetersPerSecond => _walkMetersPerSecond;
        public float WanderRadiusMeters => _wanderRadiusMeters;
        public float WanderMinHopMeters => _wanderMinHopMeters;
        public float WanderPauseMinHours => _wanderPauseMinHours;
        public float WanderPauseMaxHours => _wanderPauseMaxHours;
        public float TurnDegreesPerSecond => _turnDegreesPerSecond;
        public float StartingStatistic => _startingStatistic;
        public int CarryCapacity => _carryCapacity;
    }
}
