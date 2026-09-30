using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La journée, en haut à gauche de l'écran (GDD, section 16) : le jour, sa phase et la jauge de la journée. C'est la
    /// vue (UI Toolkit, le bloc « journee » de Hud.uxml) ; son présentateur décide. Le démarrage lui donne, à chaque
    /// image, la fraction du pas en cours, pour que le repère avance sans à-coups.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DayBoard : MonoBehaviour, IDayBoardView
    {
        private const string BoardName = "journee";
        private const string DayName = "jour";
        private const string PhaseName = "phase";
        private const string GaugeName = "jauge";
        private const string HiddenClass = "bloc-cache";

        private UIDocument _document;
        private VisualElement _board;
        private Label _day;
        private Label _phase;
        private DayGauge _gauge;
        private DayBoardPresenter _presenter;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        /// <summary>Montre la journée de cette horloge ; appelé par le démarrage quand la ville s'ouvre.</summary>
        public void Show(Clock clock, TextTable texts)
        {
            Hide();
            VisualElement root = _document.rootVisualElement;
            VisualElement board = root.Q(BoardName);
            var day = root.Q<Label>(DayName);
            var phase = root.Q<Label>(PhaseName);
            var gauge = root.Q<DayGauge>(GaugeName);
            if (board == null || day == null || phase == null || gauge == null)
                throw new InvalidOperationException("La journée ne trouve pas ses éléments (UXML).");

            _board = board;
            _day = day;
            _phase = phase;
            _gauge = gauge;
            _presenter = new DayBoardPresenter(this, clock, texts);
            _board.RemoveFromClassList(HiddenClass);
        }

        /// <summary>Place le repère de l'heure entre deux pas.</summary>
        public void Follow(float stepFraction)
        {
            _presenter?.Follow(stepFraction);
        }

        /// <summary>Cache la journée et oublie l'horloge.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            _board?.AddToClassList(HiddenClass);
        }

        private void OnDestroy()
        {
            Hide();
        }

        public void ShowDay(string day, string phase)
        {
            _day.text = day;
            _phase.text = phase;
        }

        public void ShowPhases(float workEnd, float eveningEnd)
        {
            _gauge.SetPhases(workEnd, eveningEnd);
        }

        public void ShowProgress(float progress)
        {
            _gauge.SetProgress(progress);
        }
    }
}
