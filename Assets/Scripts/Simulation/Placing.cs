using System;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// Poser une construction, une opération qui touche plusieurs couches (ARCHITECTURE, section 3), par des appels
    /// vers le bas, dans cet ordre : le terrain la pose (ou la refuse) ; l'économie ouvre son chantier si elle attend
    /// d'être bâtie ; si elle se dresse dès la pose, elle est mise en service tout de suite (Commissioning : son stock,
    /// ses postes, le stock de départ).
    /// </summary>
    internal sealed class Placing
    {
        private readonly World _world;
        private readonly Commissioning _commissioning;

        public Placing(World world, Commissioning commissioning)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (ReferenceEquals(commissioning, null))
                throw new ArgumentNullException(nameof(commissioning));

            _world = world;
            _commissioning = commissioning;
        }

        /// <summary>
        /// Pose la construction si les règles le permettent, et la rend ; sinon, null (le terrain dit pourquoi).
        /// </summary>
        public Structure Place(StructureDefinition definition, Placement placement)
        {
            Structure placed = _world.Structures.Place(definition, placement);
            if (placed == null)
                return null;
            _world.ConstructionSites.Track(placed);
            if (placed.State == StructureState.Built)
                _commissioning.Commission(placed);
            return placed;
        }
    }
}
