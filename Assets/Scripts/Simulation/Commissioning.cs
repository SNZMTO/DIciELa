using System;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// La mise en service d'une construction qui devient bâtie (ARCHITECTURE, section 3 ; décision 179), une opération
    /// qui touche plusieurs couches, par des appels vers le bas, dans cet ordre : l'économie ouvre son stock si elle
    /// tient les marchandises de la ville, la société ses postes, l'économie ses charrettes ou ses places de
    /// charrette ; puis le déroulé du départ la regarde (le premier logis de la Chimère reçoit le stock de départ, son
    /// stock déjà ouvert ; le premier habitant dont c'est le lieu de travail y prend un poste, ses postes déjà
    /// ouverts). La même, que la construction se dresse dès la pose ou finisse son chantier : la pose l'appelle tout de
    /// suite, le monde à la fin du pas où le chantier a fini.
    /// </summary>
    internal sealed class Commissioning
    {
        private readonly World _world;
        private readonly Opening _opening;

        public Commissioning(World world, Opening opening)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (ReferenceEquals(opening, null))
                throw new ArgumentNullException(nameof(opening));

            _world = world;
            _opening = opening;
        }

        /// <summary>Met en service cette construction, qui vient d'être bâtie.</summary>
        public void Commission(Structure built)
        {
            _world.Warehouses.Track(built);
            _world.WorkSlots.Track(built);
            _world.Carts.Track(built);
            _opening.Welcome(built, _world.Warehouses, _world.Ledger);
            _opening.Settle(built, _world);
        }
    }
}
