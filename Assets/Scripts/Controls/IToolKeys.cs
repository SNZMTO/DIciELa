namespace Diciela.Controls
{
    /// <summary>
    /// Les touches des outils telles que le joueur les lit (décisions 168 et 221) : celle de la roue, celle de chaque
    /// outil, celle de chaque case d'une roue (son chiffre). Elles se lisent sur les liaisons, jamais dans la table des
    /// textes : une touche refaite se lit aussitôt.
    /// </summary>
    public interface IToolKeys
    {
        string WheelKey { get; }

        string KeyOf(ToolChoice choice);

        /// <summary>La touche de la case de ce rang (0 : la première) ; vide si elle n'en a pas.</summary>
        string KeyOfCard(int index);
    }
}
