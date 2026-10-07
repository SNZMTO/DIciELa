using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Une option d'une case de la roue (décision 221), un jeton de son éventail : son icône (une classe de la feuille de
    /// style) ou sa vignette, son nom et son usage, si elle est prise, et une pastille (le nombre de branches de
    /// l'étoile ; vide sans pastille).
    /// </summary>
    public sealed class WheelOption
    {
        public WheelOption(string icon, Texture2D picture, string name, string detail, bool isOn, string badge = "")
        {
            Icon = icon ?? "";
            Picture = picture;
            Name = name ?? "";
            Detail = detail ?? "";
            IsOn = isOn;
            Badge = badge ?? "";
        }

        public string Icon { get; }
        public Texture2D Picture { get; }
        public string Name { get; }
        public string Detail { get; }
        public bool IsOn { get; }
        public string Badge { get; }
    }
}
