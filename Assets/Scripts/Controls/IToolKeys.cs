namespace Diciela.Controls
{
    /// <summary>
    /// Les touches des outils telles que le joueur les lit (décision 168) : celle de la roue, celle de chaque outil. Elles
    /// se lisent sur les liaisons, jamais dans la table des textes : une touche refaite se lit aussitôt.
    /// </summary>
    public interface IToolKeys
    {
        string WheelKey { get; }

        string KeyOf(ToolChoice choice);
    }
}
