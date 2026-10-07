using System.Collections.Generic;
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
    /// Le chantier d'un sol (GDD, section 9 ; décision 209), tenu par la Chimère au poste « à tout faire », par les ordres
    /// du joueur : elle va sur la forme tracée et la bâtit d'un coup, quand son travail est fait (0,01 h d'un débutant par
    /// m² ; à 10 partout, elle apporte ×1,2, à 300 pas par heure de jeu). Un sol bâti qu'on gomme entre dans un petit
    /// chantier, la moitié du travail, et sert jusqu'à sa fin. Un fantôme gommé pendant qu'elle le bâtit la renvoie.
    /// </summary>
    public sealed class FloorSiteTests
    {
        private const int MaxSteps = 4000;
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
        public void SheBuildsTheWholeFloorAtOnceStandingOnIt()
        {
            World world = _town.WorkingWorld();
            ConstructionSite site = Trace(world, 24f, 34f);
            FloorPatch patch = site.Floor;
            Villager chimera = world.Villagers.Chimera;
            var near = new Vector2(24f, 22f);
            var far = new Vector2(34f, 22f);
            long firstBuildingStep = -1;
            FloorState underHer = FloorState.None;
            bool builtTooSoon = false;

            TestGrids.StepUntil(world, () =>
            {
                if (chimera.IsBuilding && firstBuildingStep < 0)
                {
                    firstBuildingStep = world.Clock.StepCount;
                    underHer = world.Floors.StateAt(world.Grid.CellOf(chimera.Position));
                }
                if (!patch.IsOver && (StateAt(world, near) != FloorState.Planned ||
                                      StateAt(world, far) != FloorState.Planned))
                    builtTooSoon = true;
                return patch.IsOver;
            }, MaxSteps, "le sol bâti");

            Assert.AreEqual(FloorState.Planned, underHer, "Elle se tient sur le sol qu'elle bâtit.");
            Assert.IsFalse(builtTooSoon, "Rien n'est bâti avant la fin du travail.");
            Assert.AreEqual(FloorState.Built, StateAt(world, near), "Bâti d'un coup, d'un bout…");
            Assert.AreEqual(FloorState.Built, StateAt(world, far), "… à l'autre.");
            // 0,01 h par m², ×1,2, 300 pas par heure : 2,5 pas par m².
            Assert.AreEqual(patch.AreaSquareMeters * 2.5d, world.Clock.StepCount - firstBuildingStep, 1d);
            Assert.IsEmpty(world.ConstructionSites.Queue, "Son chantier a quitté la file.");
        }

        [Test]
        public void ABuiltFloorErasedServesUntilItsSmallSiteEnds()
        {
            World world = _town.WorkingWorld();
            ConstructionSite traced = Trace(world, 24f, 34f);
            TestGrids.StepUntil(world, () => traced.Floor.IsOver, MaxSteps, "le sol bâti");

            TestTown.Send(world, new EraseFloorCommand(Box(29f, 22f, 14f, 4f), FloorScope.Anywhere));
            Assert.AreEqual(1, world.ConstructionSites.Queue.Count);
            ConstructionSite erasing = world.ConstructionSites.Queue[0];
            Assert.IsTrue(erasing.Floor.IsErase);
            Assert.AreEqual(traced.WorkNeeded * 0.5d, erasing.WorkNeeded, 1e-6d, "Effacer : la moitié du travail.");
            Assert.AreEqual(FloorState.Erasing, StateAt(world, new Vector2(29f, 22f)), "Il sert encore…");
            var onIt = new Placement(new Vector2(29f, 22f), 0f);
            Assert.AreEqual(PlacementResult.OnAFloor, world.PlacementRules.Check(_town.Cabin, onIt));

            TestGrids.StepUntil(world, () => erasing.Floor.IsOver, MaxSteps, "le sol effacé");
            Assert.AreEqual(FloorState.None, StateAt(world, new Vector2(29f, 22f)), "… jusqu'à la fin de son chantier.");
            Assert.IsNull(world.Floors.FloorAt(world.Grid.CellOf(new Vector2(29f, 22f))));
            Assert.AreEqual(PlacementResult.Ok, world.PlacementRules.Check(_town.Cabin, onIt), "La place est libre.");
        }

        [Test]
        public void AGhostErasedWhileSheBuildsItSendsHerAway()
        {
            World world = _town.WorkingWorld();
            ConstructionSite site = Trace(world, 14f, 34f);
            Villager chimera = world.Villagers.Chimera;
            TestGrids.StepUntil(world, () => chimera.IsBuilding, MaxSteps, "la Chimère au travail");

            TestTown.Send(world, new EraseFloorCommand(Box(24f, 22f, 24f, 4f), FloorScope.Anywhere));
            Assert.IsEmpty(world.ConstructionSites.Queue);
            Assert.IsTrue(site.IsFinished && site.Floor.IsOver);
            TestGrids.StepUntil(world, () => !chimera.IsBuilding, 3, "la Chimère renvoyée");
            for (int step = 0; step < 50; step++)
            {
                world.Step();
                Assert.IsFalse(chimera.IsBuilding, "Plus rien à bâtir.");
            }
        }

        // Trace un chemin de terre de 2 m de large, de fromX à toX, à z = 22 (au nord du campement), et rend son chantier.
        private ConstructionSite Trace(World world, float fromX, float toX)
        {
            var stroke = new GroundStroke(new[] { new Vector2(fromX, 22f), new Vector2(toX, 22f) }, 1f);
            TestTown.Send(world, new TraceFloorCommand(stroke, _town.Dirt, FloorScope.Anywhere));
            IReadOnlyList<ConstructionSite> queue = world.ConstructionSites.Queue;
            Assert.AreEqual(1, queue.Count, "Le chemin est un chantier dans la file.");
            return queue[0];
        }

        private static GroundBox Box(float x, float z, float width, float depth)
        {
            return new GroundBox(new RotatedRect(new Vector2(x, z), new Vector2(width, depth), 0f));
        }

        private static FloorState StateAt(World world, Vector2 point)
        {
            return world.Floors.StateAt(world.Grid.CellOf(point));
        }
    }
}
