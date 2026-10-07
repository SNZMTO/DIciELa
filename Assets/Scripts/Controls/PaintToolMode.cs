namespace Diciela.Controls
{
    /// <summary>
    /// Ce que fait un outil de sol, en familles comme les outils de Photoshop (décisions 206 et 216), dans l'ordre de sa
    /// barre et de ses touches : peindre et gommer au pinceau, tracer point par point, tracer une forme (PaintShape),
    /// poser ou retirer une place de charrette. Le nom de chaque valeur fait la clé de son nom et de son usage dans la
    /// table des textes.
    /// </summary>
    public enum PaintToolMode
    {
        Brush = 0,
        Eraser = 1,
        Trace = 2,
        Shape = 3,
        CartSpot = 4
    }
}
