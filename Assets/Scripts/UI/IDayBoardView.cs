namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la journée attend de sa vue (MVP) : montrer le jour et la phase, déjà traduits, et la
    /// jauge de la journée.
    /// </summary>
    public interface IDayBoardView
    {
        /// <summary>Écrit le jour (« Jour 3 ») et sa phase (« Soirée »).</summary>
        void ShowDay(string day, string phase);

        /// <summary>Donne à la jauge ses phases : la fin du travail et celle de la soirée, en part de la journée.</summary>
        void ShowPhases(float workEnd, float eveningEnd);

        /// <summary>Place le repère de l'heure : la part de la journée passée.</summary>
        void ShowProgress(float progress);
    }
}
