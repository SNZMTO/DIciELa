using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La pose d'un sol qu'on traverse, comme la Place (décision 192), par le chemin du joueur : l'ordre « poser » et les
    /// règles de pose. Rien ne le chevauche et il ne chevauche rien ; il s'étend devant une porte sans la boucher, et une
    /// porte s'ouvre sur lui. Carte plate de 20 × 20 m, deux mailles par mètre, 1 m libre devant une porte. La cabane fait
    /// 4 × 4 m, sa porte 1 m de large au milieu de sa façade nord : posée en (10 ; 10), elle couvre x et z de 8 à 12, et
    /// le devant de sa porte x de 9,5 à 10,5, z de 12 à 13. Le sol fait aussi 4 × 4 m : posé en (10 ; 14), il couvre z de
    /// 12 à 16, le devant de cette porte compris.
    /// </summary>
    public sealed class FloorPlacementTests
    {
        private readonly List<ScriptableObject> _created = new List<ScriptableObject>();
        private SimulationSettings _settings;
        private StructureDefinition _cabin;
        private StructureDefinition _floor;

        [SetUp]
        public void CreateData()
        {
            _settings = Keep(TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2, maxBuildSlopeDegrees: 15f,
                doorClearanceMeters: 1f));
            _cabin = Keep(TestData.Structure("cabane", 4f, 4f));
            _floor = Keep(TestData.Floor("place", 4f, 4f));
        }

        [TearDown]
        public void DestroyData()
        {
            foreach (ScriptableObject created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }

        [Test]
        public void NothingOverlapsAFloorAndAFloorOverlapsNothing()
        {
            World onAFloor = NewWorld();
            Place(onAFloor, _floor, 10f, 10f);
            Assert.AreEqual(PlacementResult.Overlaps, onAFloor.PlacementRules.Check(_cabin, At(12f, 10f)));
            Assert.AreEqual(PlacementResult.Overlaps, onAFloor.PlacementRules.Check(_floor, At(12f, 10f)));
            Assert.AreEqual(PlacementResult.Ok, onAFloor.PlacementRules.Check(_cabin, At(14f, 10f)));

            World byACabin = NewWorld();
            Place(byACabin, _cabin, 10f, 10f);
            Assert.AreEqual(PlacementResult.Overlaps, byACabin.PlacementRules.Check(_floor, At(12f, 10f)));
            Assert.AreEqual(PlacementResult.Ok, byACabin.PlacementRules.Check(_floor, At(14f, 10f)));
        }

        [Test]
        public void AFloorStretchesInFrontOfADoorWhichStaysOpen()
        {
            World world = NewWorld();
            Place(world, _cabin, 10f, 10f);
            Assert.AreEqual(PlacementResult.BlocksADoor, world.PlacementRules.Check(_cabin, At(10f, 14f)),
                "Un bâtiment devant la porte la boucherait.");

            Place(world, _floor, 10f, 14f);

            Assert.AreEqual(2, world.Structures.All.Count, "Le sol se pose devant la porte.");
            Vector2Int doorway = world.Grid.CellOf(new Vector2(10.1f, 12.4f));
            Assert.IsTrue(world.Grid.IsWalkable(doorway), "Le devant de la porte reste ouvert : on y marche.");
        }

        [Test]
        public void ADoorOpensOntoAFloor()
        {
            World world = NewWorld();
            Place(world, _floor, 10f, 14f);

            Assert.AreEqual(PlacementResult.Ok, world.PlacementRules.Check(_cabin, At(10f, 10f)),
                "La porte s'ouvre sur le sol.");

            World blocked = NewWorld();
            Place(blocked, _cabin, 10f, 14f);
            Assert.AreEqual(PlacementResult.DoorBlocked, blocked.PlacementRules.Check(_cabin, At(10f, 10f)),
                "Sur un bâtiment, elle ne s'ouvrirait pas.");
        }

        private World NewWorld()
        {
            return new World(_settings, TestData.SlopedMap(20, 0f), TestData.EmptyCatalog);
        }

        private T Keep<T>(T asset) where T : ScriptableObject
        {
            _created.Add(asset);
            return asset;
        }

        private static Placement At(float x, float z)
        {
            return new Placement(new Vector2(x, z), 0f);
        }

        private static void Place(World world, StructureDefinition definition, float x, float z)
        {
            world.Submit(new PlaceStructureCommand(definition, At(x, z)));
            world.Step();
        }
    }
}
