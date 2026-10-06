namespace Diciela.Controls
{
    /// <summary>
    /// Ce que fait l'outil de la Place (décision 206), dans l'ordre de sa barre et de ses touches (1 à 5) : peindre et
    /// gommer au pinceau, tracer un disque ou un rectangle, poser ou retirer une place de charrette. Le nom de chaque
    /// valeur fait la clé de son nom et de son usage dans la table des textes ; les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum PlaceToolMode
    {
        Brush = 0,
        Eraser = 1,
        Disc = 2,
        Box = 3,
        CartSpot = 4
    }
}
