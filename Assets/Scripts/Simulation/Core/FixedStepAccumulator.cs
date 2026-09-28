using System;
using Diciela.Data;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Transforme le temps réel écoulé en nombre de pas à jouer, au plus un nombre borné par image (ARCHITECTURE,
    /// section 5). Quand la borne est atteinte et qu'il reste du retard, ce retard est abandonné : une image lente
    /// ralentit le jeu au lieu de le faire courir après lui-même.
    /// </summary>
    public sealed class FixedStepAccumulator
    {
        private readonly double _stepSeconds;
        private readonly int _maxStepsPerFrame;
        private double _pendingSeconds;

        public FixedStepAccumulator(SimulationSettings settings)
        {
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));
            if (!(settings.StepSeconds > 0f))
                throw new ArgumentException("Un pas doit durer plus de zéro seconde.", nameof(settings));
            if (settings.MaxStepsPerFrame < 1)
                throw new ArgumentException("Il faut pouvoir jouer au moins un pas par image.", nameof(settings));

            _stepSeconds = settings.StepSeconds;
            _maxStepsPerFrame = settings.MaxStepsPerFrame;
        }

        /// <summary>
        /// La part du pas suivant déjà écoulée, de 0 à 1 : la vue montre ce qui bouge à cette fraction du chemin entre sa
        /// place au pas d'avant et celle du dernier pas joué (ARCHITECTURE, section 5).
        /// </summary>
        public float StepFraction => (float)Math.Min(1.0, _pendingSeconds / _stepSeconds);

        /// <summary>
        /// Ajoute le temps réel écoulé depuis l'image précédente et rend le nombre de pas à jouer maintenant. Un temps
        /// négatif ou indéfini ne compte pas.
        /// </summary>
        public int Accumulate(double elapsedRealSeconds)
        {
            if (elapsedRealSeconds > 0.0)
                _pendingSeconds += elapsedRealSeconds;

            int steps = 0;
            while (steps < _maxStepsPerFrame && _pendingSeconds >= _stepSeconds)
            {
                _pendingSeconds -= _stepSeconds;
                steps++;
            }

            if (steps == _maxStepsPerFrame && _pendingSeconds >= _stepSeconds)
                _pendingSeconds = 0.0;

            return steps;
        }
    }
}
