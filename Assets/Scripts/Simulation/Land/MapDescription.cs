using System;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La carte telle que la simulation la reçoit au lancement d'un monde, en données pures (ARCHITECTURE, section 10) :
    /// le relief et le point d'arrivée. L'eau, les arbres, les rochers et les champs sauvages s'y ajoutent avec les
    /// systèmes qui s'en servent.
    /// </summary>
    public sealed class MapDescription
    {
        /// <summary>La carte de ce relief ; sans point d'arrivée, il est au centre de la carte.</summary>
        public MapDescription(Relief relief, Vector2? arrivalPoint = null)
        {
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Vector2 arrival = arrivalPoint ?? new Vector2(relief.SizeX / 2f, relief.SizeZ / 2f);
            // Sur la carte, bords est et nord exclus : la dernière maille s'arrête juste avant eux.
            if (!(arrival.x >= 0f && arrival.x < relief.SizeX && arrival.y >= 0f && arrival.y < relief.SizeZ))
                throw new ArgumentException("Le point d'arrivée " + arrival + " est hors de la carte.",
                    nameof(arrivalPoint));

            Relief = relief;
            ArrivalPoint = arrival;
        }

        public Relief Relief { get; }

        /// <summary>
        /// Le point d'arrivée, en mètres sur le plan XZ : là où la Chimère apparaît (GDD, section 2). Le développeur le
        /// pose dans la carte ; sans lui, c'est le centre.
        /// </summary>
        public Vector2 ArrivalPoint { get; }
    }
}
