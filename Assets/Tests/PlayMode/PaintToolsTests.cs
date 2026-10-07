using Diciela.Controls;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les deux entrées de l'outil de sol (décisions 210 et 216), sur le petit monde des tests, appelées comme leurs gestes
    /// les appellent : l'outil Chemins pose le premier sol connu, partout, en quatre modes ; l'outil de la Place pose la
    /// zone, ou un sol choisi, seulement dans la zone ; sans sol connu, l'outil Chemins se prend, mais ne trace rien. Le
    /// monde ne joue ses pas que quand le test le dit : rien ne part avant. Les gestes pointés ont leurs tests à part
    /// (PaintPointsTests).
    /// </summary>
    public sealed class PaintToolsTests
    {
        private TestWorld _test;

        [TearDown]
        public void Destroy()
        {
            _test?.Dispose();
        }

        [Test]
        public void ThePathsToolLaysTheFirstKnownFloorAnywhere()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            PaintTool paths = _test.Paths;
            paths.Take();
            Assert.AreSame(_test.Floor, paths.Inks.Chosen.Floor, "Le premier sol que la ville sait faire.");
            Assert.IsFalse(paths.Inks.Chosen.IsZone, "L'outil Chemins ne pose pas la zone.");
            paths.Choose(PaintToolMode.CartSpot);
            Assert.AreEqual(PaintToolMode.Brush, paths.Mode, "Pas de place de charrette aux Chemins.");

            paths.Choose(PaintToolMode.Shape);
            Draw(paths, 0f, 3f);
            Assert.AreEqual(FloorState.Planned, StateAt(0f), "Un sol tracé, en fantôme, hors de toute Place.");
            Assert.AreEqual(0, _test.World.Plaza.CellCount, "La zone n'a pas bougé.");
            Assert.AreEqual(1, _test.World.ConstructionSites.Queue.Count, "Un chantier dans la file.");
        }

        [Test]
        public void ThePlazaToolLaysTheZoneOrAFloorOnlyInIt()
        {
            _test = new TestWorld(withPlaza: true, withFloor: true);
            PaintTool plaza = _test.Plaza;
            plaza.Take();
            Assert.IsTrue(plaza.Inks.Chosen.IsZone, "Au départ, l'outil de la Place pose la zone.");
            plaza.Choose(PaintToolMode.Shape);
            Draw(plaza, 0f, 3f);
            Assert.Greater(_test.World.Plaza.CellCount, 0, "La zone est tracée.");

            plaza.ChooseInk(PaintInk.Of(_test.Floor));
            Assert.AreSame(_test.Floor, plaza.Inks.Chosen.Floor, "Un sol se choisit…");
            Draw(plaza, 0f, 5f);
            Assert.AreEqual(FloorState.Planned, StateAt(0f), "… et se trace dans la zone…");
            Assert.AreEqual(FloorState.None, StateAt(4f), "… seulement dans la zone.");

            plaza.ChooseInk(PaintInk.Zone);
            Assert.IsTrue(plaza.Inks.Chosen.IsZone, "La zone se rechoisit.");
        }

        [Test]
        public void WithoutAKnownFloorThePathsToolTracesNothing()
        {
            _test = new TestWorld(withPlaza: true);
            PaintTool paths = _test.Paths;
            paths.Take();
            Assert.IsTrue(paths.IsActive, "Il se prend quand même.");
            Assert.IsTrue(paths.Inks.Chosen.IsNone, "Il ne pose rien.");
            paths.Choose(PaintToolMode.Shape);
            Draw(paths, 0f, 3f);
            Assert.IsEmpty(_test.World.ConstructionSites.Queue, "Rien ne part.");
            Assert.AreEqual(FloorState.None, StateAt(0f));
        }

        // Un disque tiré du milieu du monde (décalé de x mètres vers l'est) jusqu'à ce rayon, relâché ; puis un pas.
        private void Draw(PaintTool tool, float x, float radius)
        {
            tool.BeginDrawing(Ground(x));
            tool.AimAt(Ground(x + radius));
            tool.EndDrawing();
            _test.World.Step();
            tool.AimAt(Ground(x));
        }

        private Vector2 Ground(float east)
        {
            return _test.OnScreen(TestWorld.Center + new Vector3(east, 0f, 0f));
        }

        private FloorState StateAt(float east)
        {
            Vector3 point = TestWorld.Center + new Vector3(east, 0f, 0f);
            return _test.World.Floors.StateAt(_test.World.Grid.CellOf(new Vector2(point.x, point.z)));
        }
    }
}
