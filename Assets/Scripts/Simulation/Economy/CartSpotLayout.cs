using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Où se garent les charrettes sur une Place (GDD, section 11 ; décision 197) : sur un cercle autour de son milieu, à
    /// la part de son demi-côté que dit la fiche de la charrette, à intervalles égaux depuis un angle tiré au hasard ;
    /// chacune tournée vers le milieu ou vers le bord, au hasard, puis de travers d'un écart tiré. Le milieu reste libre.
    /// Le tirage suit le numéro de la Place : la même Place, les mêmes places.
    /// </summary>
    internal static class CartSpotLayout
    {
        // Le second entier du tirage, le premier étant le numéro de la Place : les places de charrette ont leur suite.
        private const int Draw = 197;

        /// <summary>Ajoute à la liste les places de charrette de cette Place, dans leur ordre.</summary>
        public static void Lay(Structure place, CartDefinition cart, List<CartSpot> spots)
        {
            int count = place.Definition.CartSpots;
            RotatedRect footprint = PlacementRules.Footprint(place.Definition, place.Placement);
            float radius = Mathf.Min(footprint.Size.x, footprint.Size.y) * 0.5f * cart.ParkRadius;
            var random = new HashRandom(place.Id, Draw);
            float start = (float)random.Unit() * 360f;
            for (int i = 0; i < count; i++)
            {
                float angle = start + i * 360f / count;
                float turn = random.Range(2) == 0 ? 0f : 180f;
                float slant = ((float)random.Unit() * 2f - 1f) * cart.ParkTurnDegrees;
                spots.Add(new CartSpot(place, i, footprint.Center + Heading.Direction(angle) * radius,
                    Mathf.Repeat(angle + turn + slant, 360f)));
            }
        }
    }
}
