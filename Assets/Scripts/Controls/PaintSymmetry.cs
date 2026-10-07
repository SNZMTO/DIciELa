namespace Diciela.Controls
{
    /// <summary>
    /// La symétrie d'un outil de sol (décision 216), dans l'ordre de ses options : aucune ; en miroir, par l'axe de
    /// l'épingle ; en double miroir, par ses deux axes ; en étoile, tournée autour de l'épingle à parts égales. Le nom de
    /// chaque valeur fait la clé de son nom et de son usage dans la table des textes.
    /// </summary>
    public enum PaintSymmetry
    {
        None = 0,
        Mirror = 1,
        DoubleMirror = 2,
        Star = 3
    }
}
