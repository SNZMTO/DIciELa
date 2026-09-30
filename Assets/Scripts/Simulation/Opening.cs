using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation
{
    /// <summary>
    /// Le déroulé du départ (GDD, section 6) : quand la Chimère plante son premier logis, le tas de départ se pose de
    /// lui-même, avec son contenu. Il se pose à droite de la porte, vu de face (décision 147), du même angle que le logis,
    /// devant sa façade ; si la place est prise, au plus près, par anneaux d'une maille autour d'elle. Un catalogue sans
    /// tas de départ n'en pose pas. Qu'il soit posé se lit sur les constructions : rien d'autre ne le retient.
    /// </summary>
    internal sealed class Opening
    {
        private readonly OpeningDefinition _definition;

        public Opening(OpeningDefinition definition)
        {
            _definition = definition;
        }

        /// <summary>Une construction vient d'être posée : si c'est le premier logis de la Chimère, le tas se pose.</summary>
        public void Follow(Structure placed, Placing placing, World world)
        {
            if (ReferenceEquals(_definition, null) || ReferenceEquals(_definition.StartingPile, null))
                return;
            if (!placed.Definition.IsChimeraHome || placed.State != StructureState.Built)
                return;
            StructureDefinition pileDefinition = _definition.StartingPile;
            if (world.Structures.IsPlaced(pileDefinition) || !TryFindSpot(world, placed, pileDefinition, out Placement spot))
                return;

            Structure pile = placing.Place(pileDefinition, spot);
            if (pile == null)
                return;
            foreach (GoodAmount goods in _definition.StartingGoods)
                world.Warehouses.Create(pile, goods.Good, goods.Amount, GoodsCause.StartingPile);
        }

        /// <summary>
        /// La place du tas : à droite de la porte vue de face (à l'angle 0, la porte regarde le nord, et qui lui fait
        /// face a l'ouest à sa droite), collé à la façade ; sinon, le plus proche de là où les règles de pose le
        /// permettent, dans le premier anneau qui en a une. Faux si la carte n'en a aucune.
        /// </summary>
        private static bool TryFindSpot(World world, Structure home, StructureDefinition pile, out Placement spot)
        {
            StructureDefinition definition = home.Definition;
            RotatedRect footprint = PlacementRules.Footprint(definition, home.Placement);
            var local = new Vector2(definition.DoorOffsetMeters - (definition.DoorWidthMeters + pile.WidthMeters) * 0.5f,
                (definition.DepthMeters + pile.DepthMeters) * 0.5f);
            Vector2 ideal = footprint.ToWorld(local);
            float rotation = home.Placement.RotationDegrees;
            float step = world.Grid.Metrics.CellSize;
            int maxRings = Math.Max(world.Grid.Width, world.Grid.Depth);

            for (int ring = 0; ring <= maxRings; ring++)
            {
                bool found = false;
                float best = float.MaxValue;
                spot = default;
                for (int dy = -ring; dy <= ring; dy++)
                {
                    // Sur les rangées du bas et du haut de l'anneau, toutes les places ; entre elles, les deux des bords.
                    int stride = dy == -ring || dy == ring ? 1 : 2 * ring;
                    for (int dx = -ring; dx <= ring; dx += Math.Max(stride, 1))
                    {
                        Vector2 offset = new Vector2(dx, dy) * step;
                        var candidate = new Placement(ideal + offset, rotation);
                        if (offset.magnitude < best &&
                            world.PlacementRules.Check(pile, candidate) == PlacementResult.Ok)
                        {
                            found = true;
                            best = offset.magnitude;
                            spot = candidate;
                        }
                    }
                }
                if (found)
                    return true;
            }
            spot = default;
            return false;
        }
    }
}
