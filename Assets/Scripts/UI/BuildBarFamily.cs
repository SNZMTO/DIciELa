using Diciela.Data;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>Un onglet de la barre de construction, tel que la vue le montre : le nom traduit, l'icône, le pastel.</summary>
    public readonly struct BuildBarFamily
    {
        public readonly string Name;
        public readonly VectorImage Icon;
        public readonly Pastel Pastel;

        public BuildBarFamily(string name, VectorImage icon, Pastel pastel)
        {
            Name = name;
            Icon = icon;
            Pastel = pastel;
        }
    }
}
