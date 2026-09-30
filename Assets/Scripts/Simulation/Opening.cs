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
    /// lui-même, avec son contenu. Il dit où le poser : à droite de la porte, vu de face (décision 147), du même angle
    /// que le logis, devant sa façade ; si la place est prise, au plus près, par pas d'une maille, dans le rayon que dit
    /// sa fiche. Il le remplit une fois posé. Un catalogue sans tas de départ n'en pose pas. Qu'il soit posé se lit
    /// sur les constructions : rien d'autre ne le retient.
    /// </summary>
    internal sealed class Opening
    {
        private readonly OpeningDefinition _definition;

        public Opening(OpeningDefinition definition)
        {
            _definition = definition;
        }

        /// <summary>
        /// Cette construction vient d'être posée : si c'est le premier logis de la Chimère, le tas de départ et sa place.
        /// Faux s'il n'y a pas de tas à poser, ou pas de place pour lui.
        /// </summary>
        public bool TryFindPileSpot(Structure placed, World world, out StructureDefinition pile, out Placement spot)
        {
            pile = null;
            spot = default;
            if (ReferenceEquals(_definition, null) || ReferenceEquals(_definition.StartingPile, null))
                return false;
            if (!placed.Definition.IsChimeraHome || placed.State != StructureState.Built)
                return false;
            pile = _definition.StartingPile;
            return !world.Structures.IsPlaced(pile) && TryFindSpot(world, placed, pile, out spot);
        }

        /// <summary>Le tas vient d'être posé : il reçoit son contenu, né du départ.</summary>
        public void Fill(Structure pile, Warehouses warehouses)
        {
            foreach (GoodAmount goods in _definition.StartingGoods)
                warehouses.Create(pile, goods.Good, goods.Amount, GoodsCause.StartingPile);
        }

        /// <summary>
        /// La place du tas : à droite de la porte vue de face (à l'angle 0, la porte regarde le nord, et qui lui fait
        /// face a l'ouest à sa droite), collé à la façade ; sinon, la plus proche de là où les règles de pose le
        /// permettent, décalée d'un nombre entier de mailles, à la distance de la fiche au plus. Faux s'il n'y en a
        /// aucune.
        /// </summary>
        private bool TryFindSpot(World world, Structure home, StructureDefinition pile, out Placement spot)
        {
            StructureDefinition definition = home.Definition;
            RotatedRect footprint = PlacementRules.Footprint(definition, home.Placement);
            var local = new Vector2(definition.DoorOffsetMeters - (definition.DoorWidthMeters + pile.WidthMeters) * 0.5f,
                (definition.DepthMeters + pile.DepthMeters) * 0.5f);
            Vector2 ideal = footprint.ToWorld(local);
            float rotation = home.Placement.RotationDegrees;
            float step = world.Grid.Metrics.CellSize;
            float radius = _definition.PileSearchMeters;
            int maxRings = (int)Math.Floor(radius / step);

            // Par anneaux carrés : un anneau est à ring × step au moins, mais ses coins vont plus loin ; on continue donc
            // tant qu'un anneau peut encore tenir plus près que la meilleure place trouvée.
            bool found = false;
            float best = float.MaxValue;
            spot = default;
            for (int ring = 0; ring <= maxRings && ring * step < best; ring++)
            {
                for (int dy = -ring; dy <= ring; dy++)
                {
                    // Sur les rangées du bas et du haut de l'anneau, toutes les places ; entre elles, les deux des bords.
                    int stride = dy == -ring || dy == ring ? 1 : 2 * ring;
                    for (int dx = -ring; dx <= ring; dx += stride)
                    {
                        float distance = new Vector2(dx, dy).magnitude * step;
                        if (distance > radius || distance >= best)
                            continue;
                        var candidate = new Placement(ideal + new Vector2(dx, dy) * step, rotation);
                        if (world.PlacementRules.Check(pile, candidate) != PlacementResult.Ok)
                            continue;
                        found = true;
                        best = distance;
                        spot = candidate;
                    }
                }
            }
            return found;
        }
    }
}
