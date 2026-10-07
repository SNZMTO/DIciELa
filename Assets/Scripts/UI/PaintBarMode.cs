using Diciela.Controls;

namespace Diciela.UI
{
    /// <summary>Un mode de l'outil de la Place, tel que sa barre le montre : son nom et son usage traduits, sa touche.</summary>
    public readonly struct PaintBarMode
    {
        public readonly PaintToolMode Mode;
        public readonly string Name;
        public readonly string Detail;
        public readonly string Key;

        public PaintBarMode(PaintToolMode mode, string name, string detail, string key)
        {
            Mode = mode;
            Name = name;
            Detail = detail;
            Key = key;
        }
    }
}
