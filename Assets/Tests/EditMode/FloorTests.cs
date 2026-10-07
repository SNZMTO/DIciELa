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
    /// Tracer et gommer un sol (GDD, sections 9 et 11 ; décisions 209 et 210), par les ordres du joueur, sur le monde plat
    /// de 40 m aux mailles de 50 cm : une forme tracée couvre les mailles où l'on marche dont le milieu est en elle, en
    /// fantôme, et ouvre un chantier ; ce qui s'y tient et un sol déjà posé y font un trou ; l'outil de la Place s'arrête
    /// au bord de sa zone, et la zone et le sol sont deux choses ; la gomme retire un fantôme tout de suite ; un bâtiment
    /// ne se pose pas sur un sol. Un ordre refusé ne change rien, et les sols disent pourquoi.
    /// </summary>
    public sealed class FloorTests
    {
        private static readonly Vector2 Middle = new Vector2(20f, 20f);
        private TestTown _town;
        private List<FloorResult> _refused;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _refused = new List<FloorResult>();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void ATracedShapeIsAGhostThatWaitsInTheQueue()
        {
            World world = World();
            int version = world.Grid.Version;
            Trace(world, new GroundDisc(Middle, 2f), _town.Dirt);

            Assert.IsEmpty(_refused);
            Assert.AreEqual(FloorState.Planned, StateAt(world, Middle));
            Assert.AreSame(_town.Dirt, world.Floors.FloorAt(world.Grid.CellOf(Middle)));
            Assert.AreEqual(FloorState.None, StateAt(world, Middle + new Vector2(2.5f, 0f)), "Rien au-delà.");
            Assert.IsTrue(world.Grid.IsWalkable(world.Grid.CellOf(Middle)), "On marche sur un sol…");
            Assert.AreEqual(version, world.Grid.Version, "… et le tracer ne change rien au maillage.");
            ConstructionSite site = OnlySite(world);
            Assert.IsNull(site.Structure, "Un sol n'est pas une construction.");
            // Les milieux de mailles à moins de 2 m du centre : 52 mailles de 0,25 m² (comptées à la main).
            Assert.AreEqual(13f, site.Floor.AreaSquareMeters, 0.001f);
            Assert.AreEqual(0.13d, site.WorkNeeded, 1e-6d, "0,01 h d'un débutant par m².");
            Assert.IsTrue(site.IsSupplied, "La terre ne coûte rien : on peut la bâtir tout de suite.");
        }

        [Test]
        public void WhatStandsThereAndAFloorAlreadyLaidMakeHoles()
        {
            World world = _town.World(null, new[] { new NatureSpot(_town.Tree, new Placement(new Vector2(26f, 20f), 0f)) });
            world.Floors.FloorRefused += _refused.Add;
            TestTown.Place(world, _town.Cabin, 14f, 20f);
            Trace(world, Stroke(10f, 30f, 20f), _town.Dirt);

            Assert.AreEqual(FloorState.Planned, StateAt(world, new Vector2(20f, 20f)));
            Assert.AreEqual(FloorState.Planned, StateAt(world, new Vector2(11f, 20f)));
            Assert.AreEqual(FloorState.None, StateAt(world, new Vector2(14f, 20f)), "Un trou sous le fantôme de la cabane.");
            Assert.AreEqual(FloorState.None, StateAt(world, new Vector2(26f, 20f)), "Un trou sous l'arbre.");

            // La file : le chantier de la cabane, puis celui du chemin.
            FloorPatch first = world.ConstructionSites.Queue[1].Floor;
            int firstCells = first.CellCount;
            Trace(world, new GroundStroke(new[] { new Vector2(20f, 15f), new Vector2(20f, 25f) }, 1f), _town.Dirt);
            Assert.AreEqual(firstCells, first.CellCount, "Le premier sol ne change pas…");
            Assert.AreEqual(3, world.ConstructionSites.Queue.Count);
            var second = new List<Vector2Int>();
            world.ConstructionSites.Queue[2].Floor.CopyCells(second);
            CollectionAssert.DoesNotContain(second, world.Grid.CellOf(Middle), "… il fait un trou dans le second.");
            Assert.Contains(world.Grid.CellOf(new Vector2(20f, 24f)), second);

            Trace(world, Stroke(10f, 30f, 20f), _town.Dirt);
            CollectionAssert.AreEqual(new[] { FloorResult.Nothing }, _refused, "Le même trait n'ajoute rien.");
            Assert.AreEqual(3, world.ConstructionSites.Queue.Count);
        }

        [Test]
        public void ThePlaceToolStopsAtTheEdgeOfTheZone()
        {
            World world = World();
            TestTown.Send(world, new PaintPlaceCommand(new GroundDisc(Middle, 3f)));
            Trace(world, new GroundDisc(Middle, 5f), _town.Dirt, FloorScope.Place);
            Assert.AreEqual(FloorState.Planned, StateAt(world, Middle));
            Assert.AreEqual(FloorState.None, StateAt(world, Middle + new Vector2(4f, 0f)), "Hors de la zone : rien.");

            Trace(world, new GroundDisc(Middle, 5f), _town.Dirt);
            Assert.AreEqual(FloorState.Planned, StateAt(world, Middle + new Vector2(4f, 0f)), "Les Chemins vont partout.");

            // Gommer un bout de la zone n'efface pas son sol : la zone et le sol sont deux choses.
            var bitten = new Vector2(22.75f, 20.25f);
            TestTown.Send(world, new ErasePlaceCommand(new GroundDisc(new Vector2(23f, 20f), 1f)));
            Assert.IsFalse(world.Place.Contains(world.Grid.CellOf(bitten)));
            Assert.AreEqual(FloorState.Planned, StateAt(world, bitten));

            // La gomme de la Place ne passe que dans la zone.
            TestTown.Send(world, new EraseFloorCommand(new GroundDisc(Middle, 5f), FloorScope.Place));
            Assert.AreEqual(FloorState.None, StateAt(world, Middle));
            Assert.AreEqual(FloorState.Planned, StateAt(world, bitten));
            Assert.AreEqual(FloorState.Planned, StateAt(world, Middle + new Vector2(4f, 0f)));
            Assert.IsEmpty(_refused);
        }

        [Test]
        public void AFloorTheTownDoesNotKnowIsRefused()
        {
            World world = World();
            Trace(world, new GroundDisc(Middle, 2f), _town.Paving);

            CollectionAssert.AreEqual(new[] { FloorResult.Unknown }, _refused);
            Assert.AreEqual(FloorState.None, StateAt(world, Middle));
            Assert.IsEmpty(world.ConstructionSites.Queue);
        }

        [Test]
        public void TheEraserRemovesAGhostAtOnce()
        {
            World world = World();
            Trace(world, Stroke(10f, 30f, 20f), _town.Dirt);
            ConstructionSite site = OnlySite(world);
            double workBefore = site.WorkNeeded;

            TestTown.Send(world, new EraseFloorCommand(Box(26f, 20f, 12f, 4f), FloorScope.Anywhere));
            Assert.AreEqual(FloorState.None, StateAt(world, new Vector2(25f, 20f)), "Effacé tout de suite.");
            Assert.AreEqual(FloorState.Planned, StateAt(world, new Vector2(15f, 20f)));
            Assert.AreSame(site, OnlySite(world), "Le chantier reste, plus petit.");
            Assert.AreEqual(PlannedCells(world) * 0.25f, site.Floor.AreaSquareMeters, 0.001f);
            Assert.Less(site.WorkNeeded, workBefore);

            TestTown.Send(world, new EraseFloorCommand(Box(20f, 20f, 30f, 4f), FloorScope.Anywhere));
            Assert.AreEqual(0, PlannedCells(world));
            Assert.IsEmpty(world.ConstructionSites.Queue, "Vidé, il quitte la file.");
            Assert.IsTrue(site.IsFinished && site.Floor.IsOver);
            Assert.IsEmpty(_refused);

            TestTown.Send(world, new EraseFloorCommand(Box(20f, 20f, 30f, 4f), FloorScope.Anywhere));
            CollectionAssert.AreEqual(new[] { FloorResult.Nothing }, _refused, "Plus rien à gommer.");
        }

        [Test]
        public void NothingIsBuiltOnAFloorOrItsGhost()
        {
            World world = World();
            var refusals = new List<PlacementResult>();
            world.Structures.PlacementRefused += (definition, placement, reason) => refusals.Add(reason);
            Structure camp = TestTown.Place(world, _town.Camp, 20f, 10f);
            Trace(world, new GroundDisc(new Vector2(30f, 30f), 3f), _town.Dirt);

            TestTown.Send(world, new PlaceStructureCommand(_town.Cabin, new Placement(new Vector2(31f, 30f), 0f)));
            CollectionAssert.AreEqual(new[] { PlacementResult.OnAFloor }, refusals, "Sur un chemin : efface-le d'abord.");
            Assert.AreEqual(1, world.Structures.All.Count, "Seul le campement est posé.");
            Assert.AreEqual(PlacementResult.OnAFloor,
                world.PlacementRules.CheckMove(camp, new Placement(new Vector2(30f, 30f), 0f)), "Ni déplacé dessus.");
            Assert.AreEqual(PlacementResult.Ok,
                world.PlacementRules.Check(_town.Cabin, new Placement(new Vector2(30f, 20f), 0f)), "À côté, oui.");
        }

        private World World()
        {
            World world = _town.World();
            world.Floors.FloorRefused += _refused.Add;
            return world;
        }

        private static void Trace(World world, GroundShape shape, FloorDefinition floor,
            FloorScope scope = FloorScope.Anywhere)
        {
            TestTown.Send(world, new TraceFloorCommand(shape, floor, scope));
        }

        private static GroundStroke Stroke(float fromX, float toX, float z)
        {
            return new GroundStroke(new[] { new Vector2(fromX, z), new Vector2(toX, z) }, 1f);
        }

        private static GroundBox Box(float x, float z, float width, float depth)
        {
            return new GroundBox(new RotatedRect(new Vector2(x, z), new Vector2(width, depth), 0f));
        }

        private static FloorState StateAt(World world, Vector2 point)
        {
            return world.Floors.StateAt(world.Grid.CellOf(point));
        }

        private static ConstructionSite OnlySite(World world)
        {
            Assert.AreEqual(1, world.ConstructionSites.Queue.Count, "Un seul chantier dans la file.");
            return world.ConstructionSites.Queue[0];
        }

        // Les mailles en fantôme de toute la carte, comptées une à une : l'oracle de la surface d'un sol.
        private static int PlannedCells(World world)
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
