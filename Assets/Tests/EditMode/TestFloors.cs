using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Ce que partagent les tests des sols (FloorTests, FloorEraseTests), sur le monde plat de 40 m aux mailles de 50 cm :
    /// un monde qui note les refus, tracer par l'ordre du joueur, un trait et un rectangle tout faits, l'état d'un sol en
    /// un point, le seul chantier de la file, et l'oracle de la surface en fantôme, comptée maille à maille.
    /// </summary>
    internal static class TestFloors
    {
        /// <summary>Le milieu du monde de 40 m.</summary>
        public static readonly Vector2 Middle = new Vector2(20f, 20f);

        /// <summary>Un monde de ce banc, dont les refus des sols s'ajoutent à cette liste.</summary>
        public static World Noting(TestTown town, List<FloorResult> refused)
        {
            World world = town.World();
            world.Floors.FloorRefused += refused.Add;
            return world;
        }

        public static void Trace(World world, GroundShape shape, FloorDefinition floor,
            FloorScope scope = FloorScope.Anywhere)
        {
            TestTown.Send(world, new TraceFloorCommand(shape, floor, scope));
        }

        /// <summary>Un trait de 2 m de large, d'est en ouest, à cette hauteur.</summary>
        public static GroundStroke Stroke(float fromX, float toX, float z)
        {
            return new GroundStroke(new[] { new Vector2(fromX, z), new Vector2(toX, z) }, 1f);
        }

        public static GroundBox Box(float x, float z, float width, float depth)
        {
            return new GroundBox(new RotatedRect(new Vector2(x, z), new Vector2(width, depth), 0f));
        }

        public static FloorState StateAt(World world, Vector2 point)
        {
            return world.Floors.StateAt(world.Grid.CellOf(point));
        }

        public static ConstructionSite OnlySite(World world)
        {
            Assert.AreEqual(1, world.ConstructionSites.Queue.Count, "Un seul chantier dans la file.");
            return world.ConstructionSites.Queue[0];
        }

        /// <summary>Les mailles en fantôme de toute la carte, comptées une à une : l'oracle de la surface d'un sol.</summary>
        public static int PlannedCells(World world)
        {
            int count = 0;
            for (int x = 0; x < world.Grid.Width; x++)
            {
                for (int y = 0; y < world.Grid.Depth; y++)
                {
                    if (world.Floors.StateAt(new Vector2Int(x, y)) == FloorState.Planned)
                        count++;
                }
            }
            return count;
        }
    }
}
