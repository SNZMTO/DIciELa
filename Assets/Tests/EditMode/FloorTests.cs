using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using static Diciela.Tests.EditMode.TestFloors;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Tracer un sol (GDD, sections 9 et 11 ; décisions 209 et 210), par les ordres du joueur, sur le monde plat de 40 m
    /// aux mailles de 50 cm : une forme tracée couvre les mailles où l'on marche dont le milieu est en elle, en fantôme, et
    /// ouvre un chantier ; ce qui s'y tient et un sol déjà posé y font un trou ; un sol que la ville ne sait pas faire est
    /// refusé ; un bâtiment ne se pose pas sur un sol. Un ordre refusé ne change rien, et les sols disent pourquoi.
    /// </summary>
    public sealed class FloorTests
    {
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
            World world = Noting(_town, _refused);
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
        public void AFloorTheTownDoesNotKnowIsRefused()
        {
            World world = Noting(_town, _refused);
            Trace(world, new GroundDisc(Middle, 2f), _town.Paving);

            CollectionAssert.AreEqual(new[] { FloorResult.Unknown }, _refused);
            Assert.AreEqual(FloorState.None, StateAt(world, Middle));
            Assert.IsEmpty(world.ConstructionSites.Queue);
        }

        [Test]
        public void NothingIsBuiltOnAFloorOrItsGhost()
        {
            World world = Noting(_town, _refused);
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
    }
}
