using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La pose, par le chemin du joueur : un ordre envoyé au monde, un pas joué ; l'aperçu, par les règles de pose.
    /// Carte plate de 20 × 20 m, deux mailles par mètre, pente maximale 15°, 1 m libre devant une porte. La cabane fait
    /// 4 × 4 m, sa porte 1 m de large au milieu de sa façade nord. Posée en (10 ; 10) à 0°, elle couvre x et z de 8 à
    /// 12, et le devant de sa porte x de 9,5 à 10,5, z de 12 à 13. Les positions voisines sont choisies d'après ces
    /// bornes.
    /// </summary>
    public sealed class PlacementTests
    {
        private readonly List<ScriptableObject> _created = new List<ScriptableObject>();
        private SimulationSettings _settings;
        private StructureDefinition _cabin;
        private readonly List<PlacementResult> _refusals = new List<PlacementResult>();

        [SetUp]
        public void CreateData()
        {
            _settings = Keep(TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2, maxBuildSlopeDegrees: 15f,
                doorClearanceMeters: 1f));
            _cabin = Keep(TestData.Structure("cabane", 4f, 4f));
            _refusals.Clear();
        }

        [TearDown]
        public void DestroyData()
        {
            foreach (ScriptableObject created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }

        private T Keep<T>(T asset) where T : ScriptableObject
        {
            _created.Add(asset);
            return asset;
        }

        private World NewWorld(MapDescription map)
        {
            var world = new World(_settings, map);
            world.Structures.PlacementRefused += (definition, placement, reason) => _refusals.Add(reason);
            return world;
        }

        private static Placement At(float x, float z, float rotation = 0f)
        {
            return new Placement(new Vector2(x, z), rotation);
        }

        private static void Place(World world, StructureDefinition definition, float x, float z, float rotation = 0f)
        {
            world.Submit(new PlaceStructureCommand(definition, At(x, z, rotation)));
            world.Step();
        }

        [Test]
        public void AnOrderPlacesTheStructureAtTheNextStep()
        {
            World world = NewWorld(TestData.SlopedMap(20, 0f));

            world.Submit(new PlaceStructureCommand(_cabin, At(10f, 10f)));
            Assert.AreEqual(0, world.Structures.All.Count);
            world.Step();

            Assert.AreEqual(1, world.Structures.All.Count);
            Structure cabin = world.Structures.All[0];
            Assert.AreEqual(1, cabin.Id);
            Assert.AreSame(_cabin, cabin.Definition);
            Assert.AreEqual(new Vector2(10f, 10f), cabin.Placement.Center);
            Assert.AreEqual(StructureState.Planned, cabin.State);
        }

        [Test]
        public void RefusesAStructureOutsideTheMap()
        {
            World world = NewWorld(TestData.SlopedMap(20, 0f));

            Place(world, _cabin, 1f, 10f);
            Place(world, _cabin, 10f, 18.5f);

            Assert.AreEqual(0, world.Structures.All.Count);
            Assert.AreEqual(new[] { PlacementResult.OutsideMap, PlacementResult.OutsideMap }, _refusals);
        }

        [Test]
        public void RefusesASlopeSteeperThanTheLimit()
        {
            World steep = NewWorld(TestData.SlopedMap(20, 0.5f));
            World gentle = NewWorld(TestData.SlopedMap(20, 0.2f));

            Place(steep, _cabin, 10f, 10f);
            Place(gentle, _cabin, 10f, 10f);

            Assert.AreEqual(new[] { PlacementResult.TooSteep }, _refusals);
            Assert.AreEqual(1, gentle.Structures.All.Count);
        }

        [Test]
        public void StructuresMayTouchButNotOverlap()
        {
            World world = NewWorld(TestData.SlopedMap(20, 0f));
            Place(world, _cabin, 10f, 10f);

            Assert.AreEqual(PlacementResult.Overlaps, world.PlacementRules.Check(_cabin, At(12f, 10f)));
            Assert.AreEqual(PlacementResult.Ok, world.PlacementRules.Check(_cabin, At(14f, 10f)));
        }

        [Test]
        public void NothingIsPlacedInFrontOfADoor()
        {
            World world = NewWorld(TestData.SlopedMap(20, 0f));
            Place(world, _cabin, 10f, 10f);

            Assert.AreEqual(PlacementResult.BlocksADoor, world.PlacementRules.Check(_cabin, At(10f, 14.5f)));
            Assert.AreEqual(PlacementResult.Ok, world.PlacementRules.Check(_cabin, At(10f, 15f)));
        }

        [Test]
        public void ADoorNeedsFreeGroundInFrontOfIt()
        {
            World world = NewWorld(TestData.SlopedMap(20, 0f));
            Place(world, _cabin, 10f, 14.5f);

            Assert.AreEqual(PlacementResult.DoorBlocked, world.PlacementRules.Check(_cabin, At(10f, 10f)));
        }

        [Test]
        public void TheDoorTurnsWithTheStructure()
        {
            World world = NewWorld(TestData.SlopedMap(20, 0f));
            Place(world, _cabin, 10f, 10f, 90f);

            Assert.AreEqual(PlacementResult.BlocksADoor, world.PlacementRules.Check(_cabin, At(14.5f, 10f)));
            Assert.AreEqual(PlacementResult.Ok, world.PlacementRules.Check(_cabin, At(10f, 14.5f)));
        }

        [Test]
        public void ARefusedOrderChangesNothing()
        {
            World world = NewWorld(TestData.SlopedMap(20, 0f));
            Place(world, _cabin, 10f, 10f);

            Place(world, _cabin, 11f, 11f);

            Assert.AreEqual(1, world.Structures.All.Count);
            Assert.AreEqual(new[] { PlacementResult.Overlaps }, _refusals);
            Assert.AreEqual(PlacementResult.Ok, world.PlacementRules.Check(_cabin, At(15f, 5f)));
        }
    }
}
