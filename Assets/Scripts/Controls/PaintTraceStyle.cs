namespace Diciela.Controls
{
    /// <summary>
    /// Comment un tracé et un polygone relient leurs points (décision 216), dans l'ordre de leurs options : en segments
    /// droits, ou en une courbe douce qui passe par tous (GroundSpline). Le nom de chaque valeur fait la clé de son nom et
    /// de son usage dans la table des textes.
    /// </summary>
    public enum PaintTraceStyle
    {
        Straight = 0,
        Curved = 1
    }
}
