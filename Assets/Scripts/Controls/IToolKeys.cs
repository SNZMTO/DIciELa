namespace Diciela.Controls
{
    /// <summary>
    /// Les touches des outils telles que le joueur les lit (décisions 168 et 184) : celle de la roue, celle de chaque outil,
    /// celle de chaque carte du panneau Construire. Elles se lisent sur les liaisons, jamais dans la table des textes : une
    /// touche refaite se lit aussitôt.
    /// </summary>
    public interface IToolKeys
    {
        string WheelKey { get; }

        string KeyOf(ToolChoice choice);

        /// <summary>La touche de la carte de ce rang (0 : la première) ; vide si elle n'en a pas.</summary>
        string KeyOfCard(int index);
    }
}
