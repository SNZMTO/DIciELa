using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Une carte de la barre de construction, telle que la vue la montre : le nom traduit, la vignette (absente tant que
    /// la construction n'est pas photographiée), le pastel de sa famille, dont la carte prend la version pâle, son coût
    /// (vide pour ce qui se dresse d'un coup) et la touche qui la prend, écrite sur elle (vide : aucune).
    /// </summary>
    public readonly struct BuildBarCard
    {
        public readonly string Name;
        public readonly Texture2D Picture;
        public readonly Pastel Pastel;
        public readonly IReadOnlyList<GoodAmount> Cost;
        public readonly string Key;

        public BuildBarCard(string name, Texture2D picture, Pastel pastel, IReadOnlyList<GoodAmount> cost, string key)
        {
            Name = name;
            Picture = picture;
            Pastel = pastel;
            Cost = cost;
            Key = key;
        }
    }
}
