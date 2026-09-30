namespace Diciela.UI
{
    /// <summary>Ce que le présentateur de l'astuce attend de sa vue (MVP) : une phrase, ou rien, et la vue se cache.</summary>
    public interface ITipView
    {
        /// <summary>Montre cette phrase ; un texte vide cache l'astuce.</summary>
        void ShowTip(string text);
    }
}
