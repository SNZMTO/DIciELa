namespace Diciela.UI
{
    /// <summary>
    /// Un poste du lieu, tel que sa fiche le montre (GDD, section 16) : son nom et qui le tient (« Libre » s'il est
    /// libre), déjà traduits. La fiche n'écrit le nom qu'une fois pour des postes pareils qui se suivent.
    /// </summary>
    public readonly struct LocationSheetSlot
    {
        public LocationSheetSlot(string name, string holder)
        {
            Name = name;
            Holder = holder;
        }

        public string Name { get; }
        public string Holder { get; }
    }
}
