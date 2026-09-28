using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La sortie d'une construction posée sur un piéton (décision 88), sur la petite carte plate de 10 m, à deux mailles
    /// par mètre. Un bloc de 4 × 4 m posé par l'ordre « poser » en (5 ; 5) couvre les mailles 6 à 13 en x et en z. Les
    /// blocs ont leurs bords sur des limites de mailles : ils prennent les mêmes mailles quelle que soit la règle du
    /// tramage.
    /// </summary>
    public sealed class WayOutFinderTests
    {
        private SimulationSettings _settings;
        private StructureDefinition _block;
        private StructureDefinition _post;

        [SetUp]
        public void CreateData()
        {
            _settings = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2);
            _block = TestData.Structure("bloc", 4f, 4f, hasDoor: false);
            _post = TestData.Structure("poteau", 0.5f, 2f, hasDoor: false);
        }

        [TearDown]
        public void DestroyData()
        {
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_block);
            Object.DestroyImmediate(_post);
        }

        private World Place(float size, params (StructureDefinition Definition, float X, float Z)[] structures)
        {
            var world = new World(_settings, TestData.FlatMap(size));
            foreach ((StructureDefinition definition, float x, float z) in structures)
                world.Submit(new PlaceStructureCommand(definition, new Placement(new Vector2(x, z), 0f)));
            world.Step();
            Assert.AreEqual(structures.Length, world.Structures.All.Count);
            return world;
        }

        [Test]
        public void FromInsideTheWayOutIsTheNearestCellAlongItsEdge()
        {
            // Le point (6,6 ; 5,2) est dans la maille (13 ; 10) : à 0,65 m du milieu de (14 ; 10), à l'est ; l'ouest, le
            // sud et le nord sont à plus de 1,5 m.
            World world = Place(10f, (_block, 5f, 5f));

            Assert.IsTrue(new WayOutFinder(world.Grid, world.Islands).TryFind(new Vector2(6.6f, 5.2f), out Vector2Int exit));
            Assert.AreEqual(new Vector2Int(14, 10), exit);
        }

        [Test]
        public void SheNeverLeavesThroughAnotherConstruction()
        {
            // Un poteau de 0,5 × 2 m, collé au bloc à l'est, prend les mailles (14 ; 8 à 11). Du point (6,9 ; 5,25), dans
            // la maille (13 ; 10), la maille libre la plus proche serait (15 ; 10), à 0,85 m, mais derrière le poteau.
            // Au bord du bloc, la plus proche est (14 ; 12), à 1,06 m (√(0,35² + 1²)).
            World world = Place(10f, (_block, 5f, 5f), (_post, 7.25f, 5f));

            Assert.IsTrue(new WayOutFinder(world.Grid, world.Islands).TryFind(new Vector2(6.9f, 5.25f), out Vector2Int exit));
            Assert.AreEqual(new Vector2Int(14, 12), exit);
        }

        [Test]
        public void OnFreeGroundThereIsNothingToLeave()
        {
            World world = Place(10f, (_block, 5f, 5f));

            Assert.IsFalse(new WayOutFinder(world.Grid, world.Islands).TryFind(new Vector2(1.1f, 8.9f), out _));
        }

        [Test]
        public void WithoutAFreeCellAlongItsEdgeThereIsNone()
        {
            // Le bloc couvre toute la carte de 4 m : rien ne le borde.
            World world = Place(4f, (_block, 2f, 2f));

            Assert.IsFalse(new WayOutFinder(world.Grid, world.Islands).TryFind(new Vector2(2.25f, 2.25f), out _));
        }
    }
}
