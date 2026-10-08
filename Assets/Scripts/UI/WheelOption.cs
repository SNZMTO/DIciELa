namespace Diciela.UI
{
    /// <summary>
    /// Une option d'une case de la roue (décision 221), un jeton de son éventail : son visage (une icône ou une
    /// vignette), son nom et son usage, si elle est prise, et une pastille (le nombre de branches de l'étoile ; vide
    /// sans pastille).
    /// </summary>
    public sealed class WheelOption
    {
        public WheelOption(HudFace face, string name, string detail, bool isOn, string badge = "")
        {
            Face = face;
            Name = name ?? "";
            Detail = detail ?? "";
            IsOn = isOn;
            Badge = badge ?? "";
        }

        public HudFace Face { get; }
        public string Name { get; }
        public string Detail { get; }
        public bool IsOn { get; }
        public string Badge { get; }
    }
}
