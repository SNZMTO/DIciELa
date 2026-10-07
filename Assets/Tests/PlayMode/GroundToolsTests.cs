using Diciela.Controls;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les deux entrées de l'outil de sol (décision 210), sur le petit monde des tests, appelées comme leurs gestes les
    /// appellent : l'outil Chemins pose le premier sol connu, partout, en quatre modes ; l'outil de la Place pose la zone,
    /// ou un sol choisi, seulement dans la zone ; sans sol connu, l'outil Chemins se prend, mais ne trace rien. Le monde
    /// ne joue ses pas que quand le test le dit : rien ne part avant.
    /// </summary>
    public sealed class GroundToolsTests
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
            _test = new TestWorld(withPlace: true, withFloor: true);
            PlaceTool paths = _test.Paths;
            paths.Take();
            Assert.AreSame(_test.Floor, paths.Inks.Floor, "Le premier sol que la ville sait faire.");
            Assert.IsFalse(paths.Inks.LaysTheZone, "L'outil Chemins ne pose pas la zone.");
            paths.Choose(PlaceToolMode.CartSpot);
            Assert.AreEqual(PlaceToolMode.Brush, paths.Mode, "Pas de place de charrette aux Chemins.");

            paths.Choose(PlaceToolMode.Disc);
            Draw(paths, 0f, 3f);
            Assert.AreEqual(FloorState.Planned, StateAt(0f), "Un sol tracé, en fantôme, hors de toute Place.");
            Assert.AreEqual(0, _test.World.Place.CellCount, "La zone n'a pas bougé.");
            Assert.AreEqual(1, _test.World.ConstructionSites.Queue.Count, "Un chantier dans la file.");
        }

        [Test]
        public void ThePlaceToolLaysTheZoneOrAFloorOnlyInIt()
        {
            _test = new TestWorld(withPlace: true, withFloor: true);
            PlaceTool place = _test.Place;
            place.Take();
            Assert.IsTrue(place.Inks.LaysTheZone, "Au départ, l'outil de la Place pose la zone.");
            place.Choose(PlaceToolMode.Disc);
            Draw(place, 0f, 3f);
            Assert.Greater(_test.World.Place.CellCount, 0, "La zone est tracée.");

            place.ChooseFloor(_test.Floor);
            Assert.AreSame(_test.Floor, place.Inks.Floor, "Un sol se choisit…");
            Draw(place, 0f, 5f);
            Assert.AreEqual(FloorState.Planned, StateAt(0f), "… et se trace dans la zone…");
            Assert.AreEqual(FloorState.None, StateAt(4f), "… seulement dans la zone.");

            place.ChooseFloor(null);
            Assert.IsTrue(place.Inks.LaysTheZone, "La zone se rechoisit.");
        }

        [Test]
        public void WithoutAKnownFloorThePathsToolTracesNothing()
        {
            _test = new TestWorld(withPlace: true);
            PlaceTool paths = _test.Paths;
            paths.Take();
            Assert.IsTrue(paths.IsActive, "Il se prend quand même.");
            Assert.IsNull(paths.Inks.Floor);
            paths.Choose(PlaceToolMode.Disc);
            Draw(paths, 0f, 3f);
            Assert.IsEmpty(_test.World.ConstructionSites.Queue, "Rien ne part.");
            Assert.AreEqual(FloorState.None, StateAt(0f));
        }

        // Un disque tiré du milieu du monde (décalé de x mètres vers l'est) jusqu'à ce rayon, relâché ; puis un pas.
        private void Draw(PlaceTool tool, float x, float radius)
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
