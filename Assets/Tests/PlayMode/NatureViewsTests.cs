using System.Collections.Generic;
using System.Text.RegularExpressions;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les arbres peints et vus (décision 157), sur les arbres d'essai (TestPaintedNature) : la lecture ne garde que les
    /// deux arbres au modèle rangé, à leur place, leur angle et leur largeur, avec leur numéro d'instance, et dit le modèle
    /// orphelin ; marqué puis retiré par la Chimère, un arbre disparaît du Terrain (sa taille passe à 0), l'autre reste.
    /// </summary>
    public sealed class NatureViewsTests
    {
        private TestPaintedNature _painted;

        [SetUp]
        public void Paint()
        {
            _painted = new TestPaintedNature();
        }

        [TearDown]
        public void Clean()
        {
            _painted.Dispose();
        }

        [Test]
        public void TheReaderKeepsTheRangedTreesAndNamesTheOrphanModel()
        {
            var instances = new List<int>();
            LogAssert.Expect(LogType.Warning, new Regex("Cube.*n'est rangé dans aucune sorte"));
            MapDescription map = _painted.Read(instances);

            Assert.AreEqual(2, map.Nature.Count);
            Assert.AreEqual(new[] { 0, 2 }, instances.ToArray(), "Le numéro d'instance de chacun sur le Terrain.");
            Assert.Less(Vector2.Distance(new Vector2(28f, 22f), map.Nature[0].Placement.Center), 1e-3f);
            Assert.AreEqual(90f, map.Nature[0].Placement.RotationDegrees, 1e-3f);
            Assert.AreEqual(1.5f, map.Nature[0].Scale, 1e-4f);
            Assert.AreSame(_painted.Tree, map.Nature[1].Definition);
        }

        [Test]
        public void AClearedTreeLeavesTheTerrainAndTheOtherStays()
        {
            var instances = new List<int>();
            LogAssert.Expect(LogType.Warning, new Regex("n'est rangé dans aucune sorte"));
            var town = new TestCampTown(true, _painted.Read(instances));
            try
            {
                _painted.Show(town.World.Nature, instances, town.World.Map.Relief);
                town.InstallTheCamp();
                town.World.Submit(new AssignVillagerCommand(town.World.Villagers.Chimera.Id, town.PlacedCamp.Id));
                town.World.Submit(new MarkForClearingCommand(new[] { 1 }));
                NatureItem first = town.World.Nature.Find(1);
                for (int step = 0; step < 6000 && !first.IsRemoved; step++)
                    town.World.Step();

                Assert.IsTrue(first.IsRemoved, "La Chimère a retiré l'arbre marqué.");
                Assert.AreEqual(0f, _painted.Data.GetTreeInstance(0).heightScale, "Retiré, il a quitté le Terrain.");
                Assert.AreEqual(0f, _painted.Data.GetTreeInstance(0).widthScale);
                Assert.AreEqual(1f, _painted.Data.GetTreeInstance(2).heightScale, 1e-4f, "L'autre reste.");
            }
            finally
            {
                town.Dispose();
            }
        }
    }
}
