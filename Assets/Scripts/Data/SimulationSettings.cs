using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de la simulation : la durée d'un pas, la durée d'une journée et ses phases.
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

        public float StepSeconds => _stepSeconds;
        public int MaxStepsPerFrame => _maxStepsPerFrame;
        public float DayLengthSeconds => _dayLengthSeconds;
        public float WorkFraction => _workFraction;
        public float EveningFraction => _eveningFraction;
    }
}
