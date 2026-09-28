using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le maillage d'un monde sur la petite carte plate de 10 × 10 m, à deux mailles par mètre : 20 × 20 mailles. Une
    /// maille hors du maillage est une erreur, jamais la maille de la rangée voisine. La maille libre la plus proche
    /// d'un point se cherche autour d'un bloc de 4 × 4 m posé par l'ordre « poser » en (5 ; 5) : il couvre les mailles
    /// 6 à 13 en x et en z (leur milieu de 3,25 à 6,75 m).
    /// </summary>
    public sealed class NavGridTests
    {
        private SimulationSettings _settings;
        private StructureDefinition _block;

        [SetUp]
        public void CreateSettings()
        {
            _settings = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2);
            _block = TestData.Structure("bloc", 4f, 4f, hasDoor: false);
        }

        [TearDown]
        public void DestroySettings()
        {
            UnityEngine.Object.DestroyImmediate(_settings);
            UnityEngine.Object.DestroyImmediate(_block);
        }

        private World BlockWorld(float size)
        {
            var world = new World(_settings, TestData.FlatMap(size));
            world.Submit(new PlaceStructureCommand(_block, new Placement(new Vector2(size / 2f, size / 2f), 0f)));
            world.Step();
            Assert.AreEqual(1, world.Structures.All.Count);
            return world;
        }

        [Test]
        public void TheNearestFreeCellOfAFreePointIsItsOwn()
        {
            NavGrid grid = BlockWorld(10f).Grid;

            Assert.IsTrue(grid.TryFindNearestWalkable(new Vector2(1.1f, 8.9f), 4, out Vector2Int nearest));
            Assert.AreEqual(new Vector2Int(2, 17), nearest);
        }

        [Test]
        public void FromInsideABlockTheNearestFreeCellIsJustOutside()
        {
            // Le point (6,6 ; 5,2) est dans la maille (13 ; 10) : à 0,65 m du milieu de la maille libre (14 ; 10), à
            // l'est ; l'ouest, le sud et le nord sont à plus de 1,5 m.
            NavGrid grid = BlockWorld(10f).Grid;

            Assert.IsFalse(grid.IsWalkable(new Vector2Int(13, 10)));
            Assert.IsTrue(grid.TryFindNearestWalkable(new Vector2(6.6f, 5.2f), 8, out Vector2Int nearest));
            Assert.AreEqual(new Vector2Int(14, 10), nearest);
        }

        [Test]
        public void WithoutAFreeCellInReachThereIsNone()
        {
            // Au milieu du bloc, la maille libre la plus proche est à quatre mailles : trois anneaux n'y suffisent pas.
            NavGrid grid = BlockWorld(10f).Grid;

            Assert.IsFalse(grid.TryFindNearestWalkable(new Vector2(5.25f, 5.25f), 3, out _));
            Assert.IsTrue(grid.TryFindNearestWalkable(new Vector2(5.25f, 5.25f), 4, out _));
        }

        [Test]
        public void ACellOutsideTheGridIsAnError()
        {
            NavGrid grid = new World(_settings, TestData.FlatMap()).Grid;
            var outside = new[] { new Vector2Int(-1, 5), new Vector2Int(20, 5), new Vector2Int(5, -1), new Vector2Int(5, 20) };

            Assert.IsTrue(grid.Contains(new Vector2Int(19, 19)));
            foreach (Vector2Int cell in outside)
            {
                Assert.IsFalse(grid.Contains(cell));
                Assert.Throws<ArgumentOutOfRangeException>(() => grid.OccupantAt(cell));
                Assert.Throws<ArgumentOutOfRangeException>(() => grid.IsDoorway(cell));
            }
        }
    }
}
