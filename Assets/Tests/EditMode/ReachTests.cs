using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Ce qu'un habitant peut rejoindre à pied (ARCHITECTURE, section 7), par les ordres du joueur, quand le sol change :
    /// une cabane posée dans un enclos d'arbres n'est pas servie, sur un autre îlot que la Chimère ; un arbre de l'enclos
    /// retiré ouvre un passage, les deux îlots n'en font plus qu'un, et la Chimère y porte ses matériaux. Ce qui est
    /// joignable se garde d'un pas à l'autre, et se revoit dès que le maillage change.
    /// </summary>
    public sealed class ReachTests
    {
        private TestTown _town;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void AWalledSiteWaitsForAWayInThenGetsItsMaterials()
        {
            World world = _town.WorkingWorld(Enclosure());
            Structure cabin = TestTown.Place(world, _town.Cabin, 30f, 30f);
            Assert.IsNotNull(cabin, "La cabane se pose au milieu de l'enclos.");
            ConstructionSite site = world.ConstructionSites.SiteOf(cabin);
            Vector2Int inside = world.Grid.CellOf(new Vector2(30f, 27.5f));
            Vector2Int outside = world.Grid.CellOf(new Vector2(30f, 25.5f));

            for (int i = 0; i < 600; i++)
                world.Step();
            Assert.AreNotEqual(world.Islands.IslandOf(outside), world.Islands.IslandOf(inside), "L'enclos est fermé.");
            Assert.AreEqual(0, DeliveredTo(site), "Personne ne rejoint le chantier.");
            Assert.AreEqual(0, world.Villagers.Chimera.Load.Total);

            // L'arbre du bas de l'enclos, face au campement.
            NatureItem gate = GateOf(world);
            TestTown.Send(world, new MarkForClearingCommand(new[] { gate.Id }));
            TestGrids.StepUntil(world, () => gate.IsRemoved, 3000, "l'arbre de l'enclos retiré");

            Assert.AreEqual(world.Islands.IslandOf(outside), world.Islands.IslandOf(inside), "L'enclos est ouvert.");
            TestGrids.StepUntil(world, () => DeliveredTo(site) > 0, 3000, "des matériaux livrés dans l'enclos");
        }

        /// <summary>
        /// Un enclos d'arbres de 1 m autour du carré de 26 à 34 m : 28 pieds collés, qui laissent dedans une allée d'un
        /// mètre autour d'une cabane de 4 m posée en (30 ; 30), et devant sa porte le mètre qu'elle demande.
        /// </summary>
        private IReadOnlyList<NatureSpot> Enclosure()
        {
            var spots = new List<NatureSpot>();
            for (int i = 0; i < 8; i++)
            {
                spots.Add(NatureTests.Spot(_town.Tree, 26.5f + i, 26.5f));
                spots.Add(NatureTests.Spot(_town.Tree, 26.5f + i, 33.5f));
            }
            for (int i = 0; i < 6; i++)
            {
                spots.Add(NatureTests.Spot(_town.Tree, 26.5f, 27.5f + i));
                spots.Add(NatureTests.Spot(_town.Tree, 33.5f, 27.5f + i));
            }
            return spots;
        }

        private static NatureItem GateOf(World world)
        {
            foreach (NatureItem item in world.Nature.All)
            {
                if (item.Placement.Center == new Vector2(30.5f, 26.5f))
                    return item;
            }
            Assert.Fail("L'arbre du bas de l'enclos manque.");
            return null;
        }

        private static int DeliveredTo(ConstructionSite site)
        {
            int delivered = 0;
            for (int line = 0; line < site.LineCount; line++)
                delivered += site.DeliveredAt(line);
            return delivered;
        }
    }
}
