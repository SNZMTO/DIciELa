using System;
using System.Globalization;
using Diciela.Data;
using Diciela.Simulation.Core;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la journée (GDD, sections 3 et 16) : il écoute l'horloge, écrit le jour et la phase quand ils
    /// changent, et place le repère de l'heure à chaque image, entre deux pas, sans jamais faire avancer le temps.
    /// </summary>
    public sealed class DayBoardPresenter : IDisposable
    {
        private readonly IDayBoardView _view;
        private readonly Clock _clock;
        private readonly TextTable _texts;

        public DayBoardPresenter(IDayBoardView view, Clock clock, TextTable texts)
        {
            if (view == null || ReferenceEquals(clock, null) || ReferenceEquals(texts, null))
                throw new ArgumentException("La journée a besoin de sa vue, de l'horloge et des textes.");

            _view = view;
            _clock = clock;
            _texts = texts;
            _clock.DayStarted += OnDayStarted;
            _clock.PhaseChanged += OnPhaseChanged;
            _view.ShowPhases((float)_clock.WorkEndsAt, (float)_clock.EveningEndsAt);
            ShowDay();
            Follow(0f);
        }

        public void Dispose()
        {
            _clock.DayStarted -= OnDayStarted;
            _clock.PhaseChanged -= OnPhaseChanged;
        }

        /// <summary>Place le repère : la part de la journée passée, plus la fraction du pas en cours.</summary>
        public void Follow(float stepFraction)
        {
            double progress = _clock.DayProgress + stepFraction / (double)_clock.StepsPerDay;
            _view.ShowProgress((float)Math.Min(progress, 1.0));
        }

        private void OnDayStarted(int day)
        {
            ShowDay();
        }

        private void OnPhaseChanged(DayPhase phase)
        {
            ShowDay();
        }

        private void ShowDay()
        {
            string day = _texts.Get(TextKeys.DayNumber)
                .Replace(TextKeys.DayNumberParameter, _clock.Day.ToString(CultureInfo.InvariantCulture));
            _view.ShowDay(day, _texts.Get(TextKeys.DayPhase(_clock.Phase.ToString())));
        }
    }
}
