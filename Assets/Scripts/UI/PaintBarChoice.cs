namespace Diciela.UI
{
    /// <summary>
    /// Un choix d'un outil de sol, tel que sa barre le montre (décision 216) : un mode, ou une forme du mode Forme ; son nom
    /// et son usage traduits ; sa touche, s'il en a une (vide sinon).
    /// </summary>
    public readonly struct PaintBarChoice<T>
    {
        public readonly T Value;
        public readonly string Name;
        public readonly string Detail;
        public readonly string Key;

        public PaintBarChoice(T value, string name, string detail, string key)
        {
            Value = value;
            Name = name;
            Detail = detail;
            Key = key;
        }
    }
}
