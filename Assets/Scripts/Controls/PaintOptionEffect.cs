namespace Diciela.Controls
{
    /// <summary>
    /// Ce qu'une option changée demande à la surface de son outil de sol (décision 216) : abandonner le geste en cours
    /// (une autre forme), le redessiner (un autre style), poser l'épingle si elle ne l'est pas (une symétrie), ou se
    /// refaire (le nombre de l'étoile).
    /// </summary>
    internal enum PaintOptionEffect
    {
        Abandon,
        Restyle,
        PlacePin,
        Refresh
    }
}
