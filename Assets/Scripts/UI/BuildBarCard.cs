using Diciela.Data;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Une carte de la barre de construction, telle que la vue la montre : le nom traduit, la vignette (absente tant que
    /// la construction n'est pas photographiée), le pastel de sa famille, dont la carte prend la version pâle.
    /// </summary>
    public readonly struct BuildBarCard
    {
        public readonly string Name;
        public readonly Texture2D Picture;
        public readonly Pastel Pastel;

        public BuildBarCard(string name, Texture2D picture, Pastel pastel)
        {
            Name = name;
            Picture = picture;
            Pastel = pastel;
        }
    }
}
