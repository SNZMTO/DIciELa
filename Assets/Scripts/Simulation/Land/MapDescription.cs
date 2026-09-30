using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La carte telle que la simulation la reçoit au lancement d'un monde, en données pures (ARCHITECTURE, section 10) :
    /// le relief, le point d'arrivée, les arbres et les rochers. L'eau et les champs sauvages s'y ajoutent avec les
    /// systèmes qui s'en servent.
    /// </summary>
    public sealed class MapDescription
    {
        /// <summary>
        /// La carte de ce relief ; sans point d'arrivée, il est au centre de la carte ; sans arbres ni rochers, elle est
        /// nue.
        /// </summary>
        public MapDescription(Relief relief, Vector2? arrivalPoint = null, IReadOnlyList<NatureSpot> nature = null)
        {
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Vector2 arrival = arrivalPoint ?? new Vector2(relief.SizeX / 2f, relief.SizeZ / 2f);
            // Sur la carte, bords est et nord exclus : la dernière maille s'arrête juste avant eux.
            if (!(arrival.x >= 0f && arrival.x < relief.SizeX && arrival.y >= 0f && arrival.y < relief.SizeZ))
                throw new ArgumentException("Le point d'arrivée " + arrival + " est hors de la carte.",
                    nameof(arrivalPoint));

            var spots = new NatureSpot[nature?.Count ?? 0];
            for (int i = 0; i < spots.Length; i++)
            {
                Vector2 foot = nature[i].Placement.Center;
                if (!(foot.x >= 0f && foot.x < relief.SizeX && foot.y >= 0f && foot.y < relief.SizeZ))
                    throw new ArgumentException("L'arbre ou le rocher n° " + (i + 1) + " est hors de la carte, en " + foot +
                                                ".", nameof(nature));
                spots[i] = nature[i];
            }

            Relief = relief;
            ArrivalPoint = arrival;
            Nature = spots;
        }

        public Relief Relief { get; }

        /// <summary>
        /// Le point d'arrivée, en mètres sur le plan XZ : là où la Chimère apparaît (GDD, section 2). Le développeur le
        /// pose dans la carte ; sans lui, c'est le centre.
        /// </summary>
        public Vector2 ArrivalPoint { get; }

        /// <summary>Les arbres et les rochers peints sur la carte, dans l'ordre du Terrain.</summary>
        public IReadOnlyList<NatureSpot> Nature { get; }
    }
}
