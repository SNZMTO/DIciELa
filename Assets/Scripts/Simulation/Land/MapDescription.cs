using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La carte telle que la simulation la reçoit au lancement d'un monde, en données pures (ARCHITECTURE, section 10).
    /// Pour l'instant, le relief ; l'eau, les arbres, les rochers, les champs sauvages et le point d'arrivée s'y
    /// ajoutent avec les systèmes qui s'en servent.
    /// </summary>
    public sealed class MapDescription
    {
        public MapDescription(Relief relief)
        {
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Relief = relief;
        }

        public Relief Relief { get; }
    }
}
