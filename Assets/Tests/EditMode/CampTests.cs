using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le campement, par l'ordre « poser » (GDD, sections 6 et 9) : dressé dès sa pose, sans fantôme ; unique, une seconde
    /// est refusée et dit pourquoi ; bâtie, elle est le logis de la Chimère. Carte plate de 20 × 20 m, deux mailles par
    /// mètre, 1 m libre devant une porte ; le campement fait 3 × 3 m, la cabane 4 × 4 m.
    /// </summary>
    public sealed class CampTests
    {
        private readonly List<ScriptableObject> _created = new List<ScriptableObject>();
        private readonly List<PlacementResult> _refusals = new List<PlacementResult>();
        private SimulationSettings _settings;
        private StructureDefinition _camp;
        private StructureDefinition _cabin;

        [SetUp]
        public void CreateData()
        {
            _settings = Keep(TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2, doorClearanceMeters: 1f));
            _camp = Keep(TestData.Home());
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

        private World NewWorld()
        {
            var world = new World(_settings, TestData.FlatMap(20f), TestData.EmptyCatalog);
            world.Structures.PlacementRefused += (definition, placement, reason) => _refusals.Add(reason);
            return world;
        }

        private static Placement At(float x, float z)
        {
            return new Placement(new Vector2(x, z), 0f);
        }

        [Test]
        public void TheCampStandsAsSoonAsItIsPlacedAndACabinWaits()
        {
            World world = NewWorld();

            world.Submit(new PlaceStructureCommand(_camp, At(5f, 5f)));
            world.Submit(new PlaceStructureCommand(_cabin, At(14f, 14f)));
            world.Step();

            Assert.AreEqual(2, world.Structures.All.Count);
            Assert.AreEqual(StructureState.Built, world.Structures.All[0].State, "Le campement se plante d'un coup.");
            Assert.AreEqual(StructureState.Planned, world.Structures.All[1].State, "La cabane attend ses bâtisseurs.");
        }

        [Test]
        public void ThePlacedCampIsTheChimerasHome()
        {
            World world = NewWorld();
            Assert.IsNull(world.Structures.ChimeraHome, "Pas de logis avant le campement.");

            world.Submit(new PlaceStructureCommand(_cabin, At(14f, 14f)));
            world.Submit(new PlaceStructureCommand(_camp, At(5f, 5f)));
            world.Step();

            Assert.AreSame(world.Structures.All[1], world.Structures.ChimeraHome);
        }

        [Test]
        public void AHomeThatWaitsForItsBuildersIsNotHerHomeYet()
        {
            // Une maison, logis de la Chimère, mais bâtie par un chantier : posée, ce n'est qu'un fantôme.
            StructureDefinition house = Keep(TestData.Home("maison", 4f, builtWhenPlaced: false));
            World world = NewWorld();

            world.Submit(new PlaceStructureCommand(house, At(10f, 10f)));
            world.Step();

            Assert.AreEqual(1, world.Structures.All.Count);
            Assert.IsNull(world.Structures.ChimeraHome);
        }

        [Test]
        public void ASecondCampIsRefusedAndSaysWhy()
        {
            World world = NewWorld();
            Assert.IsTrue(world.PlacementRules.IsAvailable(_camp));

            world.Submit(new PlaceStructureCommand(_camp, At(5f, 5f)));
            world.Step();
            world.Submit(new PlaceStructureCommand(_camp, At(15f, 5f)));
            world.Step();

            Assert.AreEqual(1, world.Structures.All.Count);
            Assert.AreEqual(new[] { PlacementResult.AlreadyPlaced }, _refusals);
            Assert.IsFalse(world.PlacementRules.IsAvailable(_camp), "Un campement posé ne se pose plus nulle part.");
            Assert.AreEqual(PlacementResult.AlreadyPlaced, world.PlacementRules.Check(_camp, At(15f, 15f)),
                "L'aperçu le dit, par la même fonction que l'ordre.");
            Assert.IsTrue(world.PlacementRules.IsAvailable(_cabin), "Les cabanes ne sont pas uniques.");
        }

        [Test]
        public void TwoCampOrdersInOneStepPlaceOnlyOne()
        {
            World world = NewWorld();

            world.Submit(new PlaceStructureCommand(_camp, At(5f, 5f)));
            world.Submit(new PlaceStructureCommand(_camp, At(15f, 15f)));
            world.Step();

            Assert.AreEqual(1, world.Structures.All.Count);
            Assert.AreEqual(new Vector2(5f, 5f), world.Structures.All[0].Placement.Center, "La première l'emporte.");
            Assert.AreEqual(new[] { PlacementResult.AlreadyPlaced }, _refusals);
        }
    }
}
