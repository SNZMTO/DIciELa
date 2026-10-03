using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Déplacer un chantier (GDD, section 9 ; décision 199), par l'ordre du joueur : ce n'est qu'un plan, il saute tout de
    /// suite à sa nouvelle place, avec ce qui lui a été livré ; son sol suit, et qui le sert le suit. Le banc de la ville
    /// au travail : la Chimère au poste « à tout faire » du campement, en (20 ; 14) ; la cabane de 4 × 4 m, porte au nord,
    /// coûte 10 Bois, 5 Pierre et 2 h de travail.
    /// </summary>
    public sealed class MoveSiteTests
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
        public void ASiteJumpsAtOnceWithWhatWasDelivered()
        {
            World world = _town.WorkingWorld();
            Structure cabin = TestTown.Place(world, _town.Cabin, 20f, 24f);
            ConstructionSite site = world.ConstructionSites.SiteOf(cabin);
            TestGrids.StepUntil(world, () => site.DeliveredAt(0) > 0, 3000, "une première livraison");
            int delivered = site.DeliveredAt(0);
            var moved = new List<Structure>();
            world.Structures.StructureMoved += moved.Add;

            TestTown.Send(world, new MoveStructureCommand(cabin.Id, new Placement(new Vector2(28f, 24f), 90f)));
            Assert.AreEqual(new Vector2(28f, 24f), cabin.Placement.Center, "Le chantier a sauté à sa nouvelle place.");
            Assert.AreEqual(90f, cabin.Placement.RotationDegrees);
            CollectionAssert.AreEqual(new[] { cabin }, moved, "Les constructions le disent, une fois.");
            Assert.AreSame(site, world.ConstructionSites.SiteOf(cabin), "Le même chantier, à la même place dans la file.");
            Assert.GreaterOrEqual(site.DeliveredAt(0), delivered, "Ce qui lui a été livré l'a suivi.");
            Assert.AreEqual(0, world.Grid.OccupantAt(world.Grid.CellOf(new Vector2(20f, 24f))), "Il a rendu son sol.");
            Assert.AreEqual(cabin.Id, world.Grid.OccupantAt(world.Grid.CellOf(new Vector2(28f, 24f))), "Il a pris le nouveau.");

            TestGrids.StepUntil(world, () => cabin.State == StructureState.Built, 6000, "la cabane bâtie");
            Assert.AreEqual(new Vector2(28f, 24f), cabin.Placement.Center, "Bâtie à sa nouvelle place.");
        }

        [Test]
        public void TheChimeraFollowsTheSiteSheIsBuilding()
        {
            // Elle bâtit depuis les abords de la cabane, à moins de 1 m de son bord : à moins de 2,9 m (sa demi-diagonale)
            // plus 1 m de son milieu, là où elle est maintenant.
            World world = _town.WorkingWorld();
            Villager chimera = world.Villagers.Chimera;
            Structure cabin = TestTown.Place(world, _town.Cabin, 20f, 24f);
            TestGrids.StepUntil(world, () => chimera.IsBuilding, 6000, "la Chimère au travail sur la cabane");

            TestTown.Send(world, new MoveStructureCommand(cabin.Id, new Placement(new Vector2(30f, 26f), 0f)));
            int building = 0;
            TestGrids.StepUntil(world, () =>
            {
                if (chimera.IsBuilding)
                {
                    building++;
                    Assert.Less(Vector2.Distance(chimera.Position, cabin.Placement.Center), 2.9f + 1f,
                        "Elle bâtit la cabane là où elle est maintenant, jamais de loin.");
                }
                return cabin.State == StructureState.Built;
            }, 6000, "la cabane bâtie");
            Assert.Greater(building, 0, "Elle est allée la bâtir à sa nouvelle place.");
        }

        [Test]
        public void ASiteShiftsOverItsOwnGroundAndDoor()
        {
            // Un mètre au nord : la cabane recouvre le devant de sa propre porte ; un mètre au sud : le devant de sa
            // nouvelle porte tombe sur son ancien sol ; un quart de tour sur place : elle recouvre son ancien sol.
            World world = _town.WorkingWorld();
            Structure cabin = TestTown.Place(world, _town.Cabin, 20f, 24f);
            var refused = new List<PlacementResult>();
            world.Structures.MoveRefused += (structure, placement, result) => refused.Add(result);

            TestTown.Send(world, new MoveStructureCommand(cabin.Id, new Placement(new Vector2(20f, 25f), 0f)));
            Assert.AreEqual(new Vector2(20f, 25f), cabin.Placement.Center, "Un mètre au nord, sur le devant de sa porte.");
            TestTown.Send(world, new MoveStructureCommand(cabin.Id, new Placement(new Vector2(20f, 24f), 0f)));
            Assert.AreEqual(new Vector2(20f, 24f), cabin.Placement.Center, "Un mètre au sud, sa porte sur son ancien sol.");
            TestTown.Send(world, new MoveStructureCommand(cabin.Id, new Placement(new Vector2(20f, 24f), 90f)));
            Assert.AreEqual(90f, cabin.Placement.RotationDegrees, "Un quart de tour sur place.");
            Assert.IsEmpty(refused, "Rien de tout cela n'est refusé.");
        }

        [Test]
        public void MovingOntoAnotherBuildingIsRefused()
        {
            World world = _town.WorkingWorld();
            Structure cabin = TestTown.Place(world, _town.Cabin, 12f, 24f);
            Placement before = cabin.Placement;
            var refused = new List<PlacementResult>();
            world.Structures.MoveRefused += (structure, placement, result) => refused.Add(result);

            TestTown.Send(world, new MoveStructureCommand(cabin.Id, new Placement(new Vector2(20f, 15f), 0f)));
            CollectionAssert.AreEqual(new[] { PlacementResult.Overlaps }, refused, "Le campement est là.");
            Assert.AreEqual(before.Center, cabin.Placement.Center, "Refusé, rien ne change.");
            Assert.AreEqual(cabin.Id, world.Grid.OccupantAt(world.Grid.CellOf(before.Center)), "Elle garde son sol.");
        }

        [Test]
        public void AUniqueSiteMoves()
        {
            // Une construction unique, déjà posée, se déplace : elle-même ne compte pas.
            StructureDefinition statue = _town.KeepKnown(TestData.Structure("statue", 2f, 2f, hasDoor: false));
            TestData.Unique(statue);
            TestData.Cost(statue, 1f, new GoodAmount(_town.Stone, 5));
            World world = _town.WorkingWorld();
            Structure placed = TestTown.Place(world, statue, 12f, 24f);

            TestTown.Send(world, new MoveStructureCommand(placed.Id, new Placement(new Vector2(12f, 30f), 0f)));
            Assert.AreEqual(new Vector2(12f, 30f), placed.Placement.Center);
        }
    }
}
