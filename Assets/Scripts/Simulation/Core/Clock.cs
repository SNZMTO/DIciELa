using System;
using Diciela.Data;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Le temps de jeu. La vérité est le nombre de pas joués, un entier : une journée compte un nombre entier de pas,
    /// si bien que le jour et la phase ne dérivent jamais, même après des heures de jeu.
    /// </summary>
    public sealed class Clock
    {
        private const double HoursPerDay = 24.0;

        private readonly long _stepsPerDay;
        private readonly long _workEndStep;
        private readonly long _eveningEndStep;
        private readonly double _gameHoursPerRealSecond;

        /// <summary>Levé quand une nouvelle journée commence, avec son numéro (la première est la 1).</summary>
        public event Action<int> DayStarted;

        /// <summary>Levé quand la journée change de phase, après <see cref="DayStarted"/> si les deux arrivent ensemble.</summary>
        public event Action<DayPhase> PhaseChanged;

        public Clock(SimulationSettings settings)
        {
            if (ReferenceEquals(settings, null))
                throw new ArgumentNullException(nameof(settings));

            _stepsPerDay = (long)Math.Round(settings.DayLengthSeconds / settings.StepSeconds, MidpointRounding.AwayFromZero);
            if (_stepsPerDay < 1)
                throw new ArgumentException("Une journée doit durer au moins un pas de simulation.", nameof(settings));

            double workAndEvening = (double)settings.WorkFraction + settings.EveningFraction;
            if (settings.WorkFraction < 0f || settings.EveningFraction < 0f || workAndEvening > 1.0)
                throw new ArgumentException("Le travail et la soirée doivent tenir dans la journée.", nameof(settings));

            _workEndStep = (long)Math.Round(_stepsPerDay * (double)settings.WorkFraction, MidpointRounding.AwayFromZero);
            _eveningEndStep = (long)Math.Round(_stepsPerDay * workAndEvening, MidpointRounding.AwayFromZero);
            _gameHoursPerRealSecond = HoursPerDay / (_stepsPerDay * (double)settings.StepSeconds);
            Phase = PhaseAt(0);
        }

        /// <summary>Le nombre de pas joués depuis le début de la partie.</summary>
        public long StepCount { get; private set; }

        public long StepsPerDay => _stepsPerDay;

        /// <summary>Le numéro de la journée en cours ; la première est la 1.</summary>
        public int Day => (int)(StepCount / _stepsPerDay) + 1;

        /// <summary>L'heure de la journée, de 0 à 24, comptée depuis le début du travail.</summary>
        public double HourOfDay => StepCount % _stepsPerDay * HoursPerDay / _stepsPerDay;

        public DayPhase Phase { get; private set; }

        /// <summary>La seule conversion du temps réel en heures de jeu (ARCHITECTURE, section 5).</summary>
        public double RealSecondsToGameHours(double realSeconds)
        {
            return realSeconds * _gameHoursPerRealSecond;
        }

        /// <summary>
        /// Joue un pas. Les événements partent une fois l'état à jour, jamais au milieu. Seul le monde l'appelle, dans
        /// son pas (<c>internal</c>, ouvert au seul monde) : rien au-dessus de la simulation ne fait avancer le temps.
        /// </summary>
        internal void Advance()
        {
            int dayBefore = Day;
            DayPhase phaseBefore = Phase;

            StepCount++;
            Phase = PhaseAt(StepCount % _stepsPerDay);

            if (Day != dayBefore)
                DayStarted?.Invoke(Day);
            if (Phase != phaseBefore)
                PhaseChanged?.Invoke(Phase);
        }

        private DayPhase PhaseAt(long stepInDay)
        {
            if (stepInDay < _workEndStep)
                return DayPhase.Work;
            if (stepInDay < _eveningEndStep)
                return DayPhase.Evening;
            return DayPhase.Night;
        }
    }
}
