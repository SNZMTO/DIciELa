using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le sol que tient une construction (ARCHITECTURE, section 4) : ses mailles sur le maillage et le devant de sa porte,
    /// déduits de sa place ; pris à la pose, rendus et repris quand elle change de place. Les mailles que deux places
    /// partagent se reprennent après avoir été rendues : rendre d'abord, prendre ensuite.
    /// </summary>
    internal sealed class StructureGround
    {
        private readonly NavGrid _grid;
        private readonly Doorways _doorways;
        private readonly PlacementRules _rules;

        public StructureGround(NavGrid grid, Doorways doorways, PlacementRules rules)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _doorways = doorways ?? throw new ArgumentNullException(nameof(doorways));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>La construction prend son sol à cette place : ses mailles, et le devant de sa porte.</summary>
        public void Take(Structure structure, Placement placement)
        {
            _grid.SetOccupant(PlacementRules.Footprint(structure.Definition, placement), structure.Id,
                structure.Definition.IsWalkable);
            if (structure.Definition.HasDoor)
                _doorways.Add(_rules.Doorway(structure.Definition, placement));
        }

        /// <summary>La construction rend son sol à cette place : les mailles qu'elle y tient, et le devant de sa porte.</summary>
        public void Release(Structure structure, Placement placement)
        {
            _grid.ReleaseOccupant(PlacementRules.Footprint(structure.Definition, placement), structure.Id);
            if (structure.Definition.HasDoor)
                _doorways.Remove(_rules.Doorway(structure.Definition, placement));
        }
    }
}
