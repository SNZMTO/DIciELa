using Diciela.Data;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que pose un outil de sol, tel que sa barre le montre (décision 210) : la zone de la Place, ou un sol ; son nom et
    /// son usage traduits ; sa vignette, la texture du calque d'un sol (un remplaçant), la zone ayant son icône.
    /// </summary>
    public readonly struct PlaceBarInk
    {
        /// <summary>Le sol ; null pour la zone de la Place.</summary>
        public readonly FloorDefinition Floor;
        public readonly string Name;
        public readonly string Detail;
        public readonly Texture2D Picture;

        public PlaceBarInk(FloorDefinition floor, string name, string detail, Texture2D picture)
        {
            Floor = floor;
            Name = name;
            Detail = detail;
            Picture = picture;
        }
    }
}
