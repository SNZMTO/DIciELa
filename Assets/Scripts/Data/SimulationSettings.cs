using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de la simulation : la durée d'un pas, la durée d'une journée et ses phases, le maillage, la pose, et
    /// le calcul du travail (la valeur pour une tâche, l'apport d'un expert, les ouvriers d'un chantier).
    /// La fiche fait foi (ARCHITECTURE, section 9) ; les valeurs écrites ici ne servent qu'à une fiche neuve.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages de la simulation", fileName = "SimulationSettings")]
    public sealed class SimulationSettings : ScriptableObject
    {
        [Tooltip("Durée d'un pas de simulation, en secondes de temps réel.")]
        [SerializeField, Min(0.01f)] private float _stepSeconds = 0.1f;

        [Tooltip("Nombre maximal de pas joués dans une même image, pour qu'une image lente ne fige pas le jeu.")]
        [SerializeField, Min(1)] private int _maxStepsPerFrame = 10;

        [Tooltip("Durée d'une journée de jeu, en secondes de temps réel (GDD, section 3).")]
        [SerializeField, Min(1f)] private float _dayLengthSeconds = 720f;

        [Tooltip("Part de la journée passée au travail ; la journée commence avec lui.")]
        [SerializeField, Range(0f, 1f)] private float _workFraction = 2f / 3f;

        [Tooltip("Part de la journée passée en soirée, après le travail ; la nuit est le reste.")]
        [SerializeField, Range(0f, 1f)] private float _eveningFraction = 1f / 12f;

        [Tooltip("Mailles du maillage par mètre : le maillage marque ce qui occupe le sol (SAVOIR, 2.1).")]
        [SerializeField, Min(1)] private int _cellsPerMeter = 4;

        [Tooltip("Pente maximale du sol sous un bâtiment, en degrés (GDD, section 2).")]
        [SerializeField, Range(0f, 89f)] private float _maxBuildSlopeDegrees = 15f;

        [Tooltip("Profondeur laissée libre devant une porte, en mètres (GDD, section 9).")]
        [SerializeField, Min(0f)] private float _doorClearanceMeters = 1.5f;

        [Tooltip("La plus haute valeur d'une statistique : elles vont de 0 à elle (GDD, section 5).")]
        [SerializeField, Min(1f)] private float _statisticScale = 100f;

        [Tooltip("La part de la sous-statistique dans la valeur pour une tâche ; le reste vient de la statistique " +
                 "globale (GDD, section 5).")]
        [SerializeField, Range(0f, 1f)] private float _subStatisticShare = 0.8f;

        [Tooltip("Ce qu'apporte au travail un habitant à la plus haute valeur, en fois ce qu'apporte un débutant (à 0) ; " +
                 "l'apport croît en ligne droite entre les deux (GDD, section 7).")]
        [SerializeField, Min(1f)] private float _expertWorkRate = 3f;

        [Tooltip("Le nombre d'ouvriers au plus sur un chantier (GDD, section 9).")]
        [SerializeField, Min(1)] private int _maxBuildersPerSite = 3;

        [Tooltip("Les heures de travail d'un débutant pour défaire un bâtiment qu'on déplace (GDD, section 9).")]
        [SerializeField, Min(0.01f)] private float _takeDownHours = 0.1f;

        [Tooltip("Les heures de travail d'un débutant pour le remonter à sa nouvelle place (GDD, section 9).")]
        [SerializeField, Min(0.01f)] private float _putBackHours = 0.1f;

        [Tooltip("Les heures de travail d'un débutant pour tracer un mètre carré de sol (GDD, section 9) : un chemin de " +
                 "2 × 15 m, 0,3 h.")]
        [SerializeField, Min(0.0001f)] private float _floorHoursPerSquareMeter = 0.01f;

        [Tooltip("Pour effacer un sol bâti, la part du travail qu'il a demandé à tracer (GDD, section 9).")]
        [SerializeField, Range(0.01f, 1f)] private float _floorEraseShare = 0.5f;

        [Tooltip("Les essais d'un habitant qui flâne pour trouver un point libre, à chaque départ ; tous ratés, c'est " +
                 "une pause de plus (une borne de boucle, GDD, section 7).")]
        [SerializeField, Min(1)] private int _wanderDraws = 8;

        public float StepSeconds => _stepSeconds;
        public int CellsPerMeter => _cellsPerMeter;
        public float MaxBuildSlopeDegrees => _maxBuildSlopeDegrees;
        public float DoorClearanceMeters => _doorClearanceMeters;
        public int MaxStepsPerFrame => _maxStepsPerFrame;
        public float DayLengthSeconds => _dayLengthSeconds;
        public float WorkFraction => _workFraction;
        public float EveningFraction => _eveningFraction;
        public float StatisticScale => _statisticScale;
        public float SubStatisticShare => _subStatisticShare;
        public float ExpertWorkRate => _expertWorkRate;
        public int MaxBuildersPerSite => _maxBuildersPerSite;
        public float TakeDownHours => _takeDownHours;
        public float PutBackHours => _putBackHours;
        public float FloorHoursPerSquareMeter => _floorHoursPerSquareMeter;
        public float FloorEraseShare => _floorEraseShare;
        public int WanderDraws => _wanderDraws;
    }
}
