using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Ce qu'une construction prend du maillage : toute maille qu'elle touche, même en partie (décision 87). Rien ne se
    /// chevauche donc à l'écran, et aucune fente plus étroite qu'une maille ne se traverse. Carte plate de 20 m, deux
    /// mailles par mètre (des mailles de 50 cm) ; la cabane fait 4 × 4 m, sa porte au nord ; posée en (10 ; 10), elle va
    /// de x = 8 à x = 12, soit les colonnes 16 à 23.
    /// </summary>
    public sealed class CoveringTests
    {
        private SimulationSettings _settings;
        private StructureDefinition _cabin;

        [SetUp]
        public void CreateData()
        {
            _settings = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2);
            _cabin = TestData.Structure("cabane", 4f, 4f);
        }

        [TearDown]
        public void DestroyData()
        {
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_cabin);
        }

        private World WorldWithACabin()
        {
            var world = new World(_settings, TestData.SlopedMap(20, 0f), TestData.EmptyCatalog);
            world.Submit(new PlaceStructureCommand(_cabin, new Placement(new Vector2(10f, 10f), 0f)));
            world.Step();
            Assert.AreEqual(1, world.Structures.All.Count);
            return world;
        }

        [Test]
        public void TwoStructuresNeverOverlapEvenByAFewCentimeters()
        {
            // Centrée en x = 13,76, une seconde cabane irait de 11,76 à 15,76 : elle recouvrirait la première de 24 cm,
            // dans la colonne 23 (de 11,5 à 12), que toutes deux touchent.
            World world = WorldWithACabin();

            Assert.AreEqual(PlacementResult.Overlaps,
                world.PlacementRules.Check(_cabin, new Placement(new Vector2(13.76f, 10f), 0f)));
        }

        [Test]
        public void AGapNarrowerThanACellIsNoPassage()
        {
            // Centrée en x = 14,26, la seconde cabane va de 12,26 à 16,26 : une fente de 26 cm la sépare de la première.
            // La colonne 24 (de 12 à 12,5) est dans la fente et sous la seconde : elle est prise, on ne passe pas.
            World world = WorldWithACabin();
            world.Submit(new PlaceStructureCommand(_cabin, new Placement(new Vector2(14.26f, 10f), 0f)));
            world.Step();

            Assert.AreEqual(2, world.Structures.All.Count, "La seconde cabane ne recouvre rien : elle se pose.");
            Assert.IsFalse(world.Grid.IsWalkable(new Vector2Int(24, 20)), "On passe dans la fente de 26 cm.");
        }
    }
}
