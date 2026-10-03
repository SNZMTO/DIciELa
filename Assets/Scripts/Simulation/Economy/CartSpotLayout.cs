using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Où se garent les charrettes sur une Place (GDD, section 11 ; décisions 197 et 198) : sur un cercle autour de son
    /// milieu, à la part de son demi-côté que dit la fiche de la charrette, à intervalles égaux depuis un angle tiré au
    /// hasard ; chacune le long du cercle, dans un sens ou dans l'autre, au hasard, puis de travers d'un écart tiré : la
    /// grande charrette du pack, longue de près de 6 m, tient ainsi sur la Place. Le milieu reste libre. Le tirage suit le
    /// numéro de la Place : la même Place, les mêmes places, autour de son milieu où qu'on la déplace.
    /// </summary>
    internal static class CartSpotLayout
    {
        // Le second entier du tirage, le premier étant le numéro de la Place : les places de charrette ont leur suite.
        private const int Draw = 197;

        /// <summary>Ajoute à la liste les places de charrette de cette Place, dans leur ordre.</summary>
        public static void Lay(Structure place, CartDefinition cart, List<CartSpot> spots)
        {
            for (int i = 0; i < place.Definition.CartSpots; i++)
                spots.Add(new CartSpot(place, i, place.Placement.Center, 0f));
            Relay(place, cart, spots);
        }

        /// <summary>
        /// Met les places de charrette de cette Place autour de son milieu, là où elle est maintenant : celles de la
        /// liste, qui s'y suivent dans leur ordre.
        /// </summary>
        public static void Relay(Structure place, CartDefinition cart, IReadOnlyList<CartSpot> spots)
        {
            int count = place.Definition.CartSpots;
            RotatedRect footprint = PlacementRules.Footprint(place.Definition, place.Placement);
            float radius = Mathf.Min(footprint.Size.x, footprint.Size.y) * 0.5f * cart.ParkRadius;
            var random = new HashRandom(place.Id, Draw);
            float start = (float)random.Unit() * 360f;
            for (int i = 0; i < spots.Count; i++)
            {
                CartSpot spot = spots[i];
                if (!ReferenceEquals(spot.Place, place))
                    continue;
                float angle = start + spot.Index * 360f / count;
                float turn = random.Range(2) == 0 ? 90f : -90f;
                float slant = ((float)random.Unit() * 2f - 1f) * cart.ParkTurnDegrees;
                spot.MoveTo(footprint.Center + Heading.Direction(angle) * radius,
                    Mathf.Repeat(angle + turn + slant, 360f));
            }
        }
    }
}
