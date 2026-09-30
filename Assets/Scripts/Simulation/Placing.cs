using System;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// Poser une construction, une opération qui touche plusieurs couches (ARCHITECTURE, section 3), par des appels
    /// vers le bas, dans cet ordre : le terrain la pose (ou la refuse) ; l'économie ouvre son stock si elle tient les
    /// marchandises de la ville, ou son chantier si elle attend d'être bâtie ; la société ouvre ses postes si elle est
    /// bâtie. Puis le déroulé du départ la regarde : si c'est le premier logis de la Chimère, le tas de départ se pose
    /// de même à la place qu'il dit, et il le remplit.
    /// </summary>
    internal sealed class Placing
    {
        private readonly World _world;
        private readonly Opening _opening;

        public Placing(World world, Opening opening)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (ReferenceEquals(opening, null))
                throw new ArgumentNullException(nameof(opening));

            _world = world;
            _opening = opening;
        }

        /// <summary>
        /// Pose la construction si les règles le permettent, et la rend ; sinon, null (le terrain dit pourquoi).
        /// </summary>
        public Structure Place(StructureDefinition definition, Placement placement)
        {
            Structure placed = PlaceOne(definition, placement);
            if (placed != null &&
                _opening.TryFindPileSpot(placed, _world, out StructureDefinition pile, out Placement spot))
            {
                Structure laid = PlaceOne(pile, spot);
                if (laid != null)
                    _opening.Fill(laid, _world.Warehouses);
            }
            return placed;
        }

        private Structure PlaceOne(StructureDefinition definition, Placement placement)
        {
            Structure placed = _world.Structures.Place(definition, placement);
            if (placed == null)
                return null;
            _world.Warehouses.Track(placed);
            _world.ConstructionSites.Track(placed);
            _world.WorkSlots.Track(placed);
            return placed;
        }
    }
}
