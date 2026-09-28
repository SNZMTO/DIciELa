using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La ligne tendue, cas par cas, sur des terrains dessinés (le nord en haut) à deux mailles par mètre : « # » un
    /// bloc posé par l'ordre « poser », « S » le départ, « G » l'arrivée. Le milieu de la maille (x ; y) est en
    /// ((x + ½) / 2 ; (y + ½) / 2) mètres ; les coins et les longueurs attendus sont comptés à la main.
    /// </summary>
    public sealed class TautLineTests
    {
        private readonly Path _path = new Path();
        private SimulationSettings _settings;
        private StructureDefinition _block;

        [SetUp]
        public void CreateData()
        {
            _settings = TestData.Settings(0.1f, 10f, 0.5f, 0.25f, cellsPerMeter: 2);
            _block = TestData.Structure("bloc", 0.5f, 0.5f, hasDoor: false);
        }

        [TearDown]
        public void DestroyData()
        {
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_block);
        }

        private TestGround FindIn(params string[] rows)
        {
            TestGround ground = TestGround.Drawn(_settings, _block, rows);
            Assert.IsTrue(ground.World.PathFinder.TryFindPath(ground.Starts, ground.Goals, _path),
                ground.Describe(_path.Cells));
            return ground;
        }

        [Test]
        public void OnOpenGroundTheTautLineGoesStraight()
        {
            // De la maille (0 ; 0) à (8 ; 3) : l'escalier fait des pas droits et des diagonales, la ligne va droit.
            FindIn(
                ".........",
                ".........",
                ".........",
                ".........",
                ".........",
                "........G",
                ".........",
                ".........",
                "S........");

            Assert.AreEqual(new[] { new Vector2(0.25f, 0.25f), new Vector2(4.25f, 1.75f) }, _path.Corners);
            Assert.AreEqual(Mathf.Sqrt(4f * 4f + 1.5f * 1.5f), _path.Length, 1e-4f);
        }

        [Test]
        public void InABentCorridorTheTautLineFoldsAtTheBend()
        {
            // Le couloir descend de (0 ; 4) à (0 ; 0), puis file vers l'est jusqu'à (4 ; 0) : 2 m, puis 2 m.
            FindIn(
                "S####",
                ".####",
                ".####",
                ".####",
                "....G");

            var expected = new[] { new Vector2(0.25f, 2.25f), new Vector2(0.25f, 0.25f), new Vector2(2.25f, 0.25f) };
            Assert.AreEqual(expected, _path.Corners);
            Assert.AreEqual(4f, _path.Length, 1e-4f);
        }

        [Test]
        public void TheTautLineNeverPassesThroughTheCornerOfABlock()
        {
            // Tout droit de (0 ; 0) à (2 ; 2), la ligne passerait par le coin du bloc (1 ; 0) : elle plie une fois, en
            // (0 ; 1) ou en (1 ; 2), pour 1 maille puis racine de 5, en demi-mètres.
            FindIn(
                "..G",
                "...",
                "S#.");

            Assert.AreEqual(3, _path.Corners.Count);
            Vector2 fold = _path.Corners[1];
            Assert.IsTrue(fold == new Vector2(0.25f, 0.75f) || fold == new Vector2(0.75f, 1.25f), "Le pli est en " + fold);
            Assert.AreEqual((1f + Mathf.Sqrt(5f)) * 0.5f, _path.Length, 1e-4f);
        }

        [Test]
        public void ATrajetOfOneCellIsAPoint()
        {
            TestGround ground = TestGround.Drawn(_settings, _block,
                "...",
                ".S.",
                "...");
            ground.Goals.Add(ground.Starts[0]);

            Assert.IsTrue(ground.World.PathFinder.TryFindPath(ground.Starts, ground.Goals, _path));
            Assert.AreEqual(new[] { new Vector2(0.75f, 0.75f) }, _path.Corners);
            Assert.AreEqual(0f, _path.Length);
        }
    }
}
