using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le pas d'une charrette selon le sol (GDD, section 9 ; décision 209), par le chemin du joueur : entre le campement
    /// (20 ; 14) et la Place (un disque de 7 m en (20 ; 26), deux places de charrette), deux bandes de 12 × 2,5 m, de terre
    /// puis de pavés, que la Chimère bâtit d'abord ; puis elle va chercher la charrette et la tire jusqu'à sa place. À
    /// 15 cm par pas à pied, elle tire la charrette à 7,5 cm par pas dans l'herbe (moitié moins vite), 15 cm sur la terre,
    /// 22,5 cm sur les pavés (une fois et demie plus vite) : le pas se relit à chaque pas, sous ses pieds.
    /// </summary>
    public sealed class CartFloorDriveTests
    {
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            TestData.Carts(_town.Camp, 1);
            TestData.KnowsFloors(_town.Opening, _town.Dirt, _town.Paving);
            TestData.FloorHours(_town.Settings, 0.0001f);
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void TheCarterPullsAtThePaceOfTheFloorUnderHerFeet()
        {
            World world = _town.WorkingWorld();
            Trace(world, 18.25f, _town.Dirt);
            Trace(world, 21.25f, _town.Paving);
            TestGrids.StepUntil(world, () => world.ConstructionSites.Queue.Count == 0, 3000, "les bandes bâties");
            TestTown.Send(world, new PaintPlazaCommand(new GroundDisc(new Vector2(20f, 26f), 7f)));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(18f, 26f), 0f));
            TestTown.Send(world, new AddCartSpotCommand(new Vector2(23f, 26f), 0f));
            Cart cart = world.Carts.All[0];
            Villager chimera = world.Villagers.Chimera;

            // Par sol sous ses pieds au début du pas (null : l'herbe), le plus long pas fait en tirant.
            var longest = new Dictionary<string, float>();
            CartState before = cart.State;
            TestGrids.StepUntil(world, () =>
            {
                if (before == CartState.Pulled && cart.State == CartState.Pulled)
                {
                    FloorDefinition floor = world.Floors.FloorAt(world.Grid.CellOf(chimera.PreviousPosition));
                    string key = floor == null ? "herbe" : floor.Id;
                    float moved = Vector2.Distance(chimera.PreviousPosition, chimera.Position);
                    longest[key] = Mathf.Max(longest.TryGetValue(key, out float best) ? best : 0f, moved);
                }
                before = cart.State;
                return cart.State == CartState.Parked;
            }, 3000, "la charrette garée");

            Assert.AreEqual(0.075f, longest["herbe"], 1e-4f, "Dans l'herbe, moitié moins vite qu'à pied.");
            Assert.AreEqual(0.15f, longest["terre"], 1e-4f, "Sur la terre, comme à pied.");
            Assert.AreEqual(0.225f, longest["paves"], 1e-4f, "Sur les pavés, une fois et demie plus vite.");
        }

        // Une bande de 12 × 2,5 m de ce sol, en travers du chemin, centrée sur z.
        private static void Trace(World world, float z, FloorDefinition floor)
        {
            var band = new GroundBox(new RotatedRect(new Vector2(20f, z), new Vector2(12f, 2.5f), 0f));
            TestTown.Send(world, new TraceFloorCommand(band, floor, FloorScope.Anywhere));
        }
    }
}
