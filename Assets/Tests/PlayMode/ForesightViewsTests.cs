using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Diciela.Controls;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le monde en prévision (GDD, section 16 ; décision 228), sur les petits mondes des tests : un chantier s'y montre
    /// fini (sa ligne en haut de son modèle, son fantôme caché) ; une construction qu'on déplace, déjà bâtie à sa nouvelle
    /// place, où on la touche, et plus à l'ancienne, vide ; un arbre marqué quitte le Terrain, et revient quand on le
    /// démarque ou qu'elle s'éteint. Les sols en prévision ont leur test avec les autres sols (FloorViewsTests) ; le
    /// filtre, le bouton et V, les leurs.
    /// </summary>
    public sealed class ForesightViewsTests
    {
        [UnityTest]
        public IEnumerator ASiteShowsFinishedInForesight()
        {
            using (var test = new TestWorld())
            {
                // Une cabane qui demande deux heures de travail : sans ouvrier, son chantier reste à rien de fait.
#if UNITY_EDITOR
                TestWorld.Fill(test.Cabin, "_workHours", 2f);
#endif
                test.World.Submit(new PlaceStructureCommand(test.Cabin, new Placement(new Vector2(60f, 60f), 0f)));
                test.World.Step();
                Transform cabin = test.Views.transform.GetChild(0);
                var built = cabin.GetComponent<MeshRenderer>();
                GameObject ghost = cabin.Find("Fantôme").gameObject;

                test.Views.ShowForesight(true);
                yield return null;
                Assert.AreEqual(10.5f, SiteViewsTests.LineOf(built), 1e-4f, "En prévision, la ligne est en haut…");
                Assert.IsFalse(ghost.activeSelf, "… et le fantôme caché : la cabane se voit bâtie.");

                test.Views.ShowForesight(false);
                yield return null;
                Assert.AreEqual(9.5f, SiteViewsTests.LineOf(built), 1e-4f, "Elle éteinte, rien n'est encore fait…");
                Assert.IsTrue(ghost.activeSelf, "… et le fantôme revient.");
            }
        }

        [UnityTest]
        public IEnumerator ABuildingOnItsWayIsAlreadyAtItsNewPlaceInForesight()
        {
            using (var test = new TestWorld())
            {
                Structure camp = TestMove.Place(test, test.Camp, TestWorld.Center);
                test.Mover.Take();
                Assert.IsTrue(TestMove.Touch(test, TestWorld.Center, 1.25f));
                test.Mover.AimAt(test.OnScreen(TestWorld.Center + new Vector3(10f, 0f, 0f)));
                test.Mover.Confirm();
                test.World.Step();
                test.Mover.PutDown();
                Assert.IsTrue(camp.MovingTo.HasValue, "Le campement attend d'être déplacé.");
                GameObject here = test.Views.transform.GetChild(0).gameObject;
                Transform there = test.Views.transform.GetChild(1);
                GameObject ghost = there.Find("Fantôme").gameObject;
                var choose = new ChooseTool(test.Camera, test.Views);
                Vector2 before = test.OnScreen(TestWorld.Center + new Vector3(0f, 1.25f, 0f));
                Vector2 after = test.OnScreen(TestWorld.Center + new Vector3(10f, 1.25f, 0f));

                test.Views.ShowForesight(true);
                yield return null;
                Assert.IsFalse(here.activeSelf, "En prévision, il a quitté sa place…");
                Assert.IsFalse(ghost.activeSelf, "… et sa nouvelle place n'est plus un fantôme…");
                foreach (MeshRenderer part in there.GetComponentsInChildren<MeshRenderer>())
                    Assert.GreaterOrEqual(SiteViewsTests.LineOf(part), part.bounds.max.y - 1e-3f, "… il y est bâti…");
                Assert.IsTrue(choose.TouchAt(after), "… et on l'y touche, là où on le voit…");
                Assert.IsFalse(choose.TouchAt(before), "… pas à sa place d'avant, vide.");

                test.Views.ShowForesight(false);
                yield return null;
                Assert.IsTrue(here.activeSelf, "Elle éteinte, il est encore à sa place…");
                Assert.IsTrue(ghost.activeSelf, "… sa nouvelle place, un fantôme…");
                Assert.IsTrue(choose.TouchAt(before), "… on le touche à sa place…");
                Assert.IsFalse(choose.TouchAt(after), "… et pas son fantôme.");
            }
        }

        [Test]
        public void AMarkedTreeIsGoneInForesightUntilItIsUnmarked()
        {
            using (var painted = new TestPaintedNature())
            {
                var instances = new List<int>();
                LogAssert.Expect(LogType.Warning, new Regex("n'est rangé dans aucune sorte"));
                var town = new TestCampTown(true, painted.Read(instances));
                try
                {
                    NatureViews views = painted.Show(town.World.Nature, instances, town.World.Map.Relief);
                    TerrainData data = painted.Data;
                    Order(town.World, new MarkForClearingCommand(new[] { 1 }));

                    views.ShowForesight(true);
                    Assert.AreEqual(0f, data.GetTreeInstance(0).heightScale, "En prévision, l'arbre marqué est parti…");
                    Assert.AreEqual(1f, data.GetTreeInstance(2).heightScale, 1e-4f, "… l'autre reste.");
                    Order(town.World, new UnmarkForClearingCommand(new[] { 1 }));
                    Assert.AreEqual(1.5f, data.GetTreeInstance(0).heightScale, 1e-4f, "Démarqué, il revient.");
                    Order(town.World, new MarkForClearingCommand(new[] { 1 }));
                    Assert.AreEqual(0f, data.GetTreeInstance(0).heightScale, "Marqué de nouveau, il repart.");

                    views.ShowForesight(false);
                    Assert.AreEqual(1.5f, data.GetTreeInstance(0).heightScale, 1e-4f, "Elle éteinte, il revient.");
                    Assert.AreEqual(1.5f, data.GetTreeInstance(0).widthScale, 1e-4f);
                }
                finally
                {
                    town.Dispose();
                }
            }
        }

        // Un ordre du joueur, joué par la simulation.
        private static void Order(World world, Command command)
        {
            world.Submit(command);
            world.Step();
        }
    }
}
