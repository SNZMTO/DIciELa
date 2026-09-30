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
    /// maille hors du maillage est une erreur, jamais la maille de la rangée voisine.
    /// </summary>
    public sealed class NavGridTests
    {
        private SimulationSettings _settings;

        [SetUp]
        public void CreateSettings()
        {
            _settings = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2);
        }

        [TearDown]
        public void DestroySettings()
        {
            UnityEngine.Object.DestroyImmediate(_settings);
        }

        [Test]
        public void ACellOutsideTheGridIsAnError()
        {
            NavGrid grid = new World(_settings, TestData.FlatMap(), TestData.EmptyCatalog).Grid;
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
