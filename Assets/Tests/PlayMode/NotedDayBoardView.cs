using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Une vue de la journée qui note ce que le présentateur lui montre.</summary>
    internal sealed class NotedDayBoardView : IDayBoardView
    {
        public string Day { get; private set; } = "";
        public string Phase { get; private set; } = "";
        public float WorkEnd { get; private set; } = -1f;
        public float EveningEnd { get; private set; } = -1f;
        public float Progress { get; private set; } = -1f;

        public void ShowDay(string day, string phase)
        {
            Day = day;
            Phase = phase;
        }

        public void ShowPhases(float workEnd, float eveningEnd)
        {
            WorkEnd = workEnd;
            EveningEnd = eveningEnd;
        }

        public void ShowProgress(float progress)
        {
            Progress = progress;
        }
    }
}
