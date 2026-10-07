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
    /// Gommer un sol et tracer dans la Place (GDD, sections 9 et 11 ; décisions 209 et 210), par les ordres du joueur, sur
    /// le monde plat de 40 m aux mailles de 50 cm : la gomme retire un fantôme tout de suite et son chantier rapetisse,
    /// puis quitte la file ; l'outil de la Place s'arrête au bord de sa zone, sa gomme aussi, et la zone et le sol sont
    /// deux choses. Un ordre refusé ne change rien, et les sols disent pourquoi.
    /// </summary>
    public sealed class FloorEraseTests
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
        public void TheEraserRemovesAGhostAtOnce()
        {
            World world = Noting(_town, _refused);
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
        public void ThePlazaToolStopsAtTheEdgeOfTheZone()
        {
            World world = Noting(_town, _refused);
            TestTown.Send(world, new PaintPlazaCommand(new GroundDisc(Middle, 3f)));
            Trace(world, new GroundDisc(Middle, 5f), _town.Dirt, FloorScope.Plaza);
            Assert.AreEqual(FloorState.Planned, StateAt(world, Middle));
            Assert.AreEqual(FloorState.None, StateAt(world, Middle + new Vector2(4f, 0f)), "Hors de la zone : rien.");

            Trace(world, new GroundDisc(Middle, 5f), _town.Dirt);
            Assert.AreEqual(FloorState.Planned, StateAt(world, Middle + new Vector2(4f, 0f)), "Les Chemins vont partout.");

            // Gommer un bout de la zone n'efface pas son sol : la zone et le sol sont deux choses.
            var bitten = new Vector2(22.75f, 20.25f);
            TestTown.Send(world, new ErasePlazaCommand(new GroundDisc(new Vector2(23f, 20f), 1f)));
            Assert.IsFalse(world.Plaza.Contains(world.Grid.CellOf(bitten)));
            Assert.AreEqual(FloorState.Planned, StateAt(world, bitten));

            // La gomme de la Place ne passe que dans la zone.
            TestTown.Send(world, new EraseFloorCommand(new GroundDisc(Middle, 5f), FloorScope.Plaza));
            Assert.AreEqual(FloorState.None, StateAt(world, Middle));
            Assert.AreEqual(FloorState.Planned, StateAt(world, bitten));
            Assert.AreEqual(FloorState.Planned, StateAt(world, Middle + new Vector2(4f, 0f)));
            Assert.IsEmpty(_refused);
        }
    }
}
