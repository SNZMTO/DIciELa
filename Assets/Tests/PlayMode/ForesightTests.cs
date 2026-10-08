using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La prévision (GDD, section 16 ; décision 228), sur les petits mondes des tests : un chantier s'y montre fini (sa
    /// ligne en haut de son modèle, son fantôme caché), et une construction qu'on déplace, déjà bâtie à sa nouvelle
    /// place ; un arbre marqué quitte le Terrain, et revient quand on le démarque ou qu'elle s'éteint ; le filtre de
    /// rêverie entre et sort en fondu ; le bouton la bascule et reste enfoncé tant qu'elle dure. Les sols en prévision
    /// ont leur test avec les autres sols (FloorViewsTests), et V le sien (ForesightControlsTests).
    /// </summary>
    public sealed class ForesightTests
    {
        private const string Texts = "clé,fr\n" +
                                     "prevision.infobulle,Prévision\n" +
                                     "prevision.infobulle.suite,Ou appuie sur {touche}\n";

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
                Assert.IsTrue(camp.MovingTo.HasValue, "Le campement attend d'être déplacé.");
                GameObject here = test.Views.transform.GetChild(0).gameObject;
                Transform there = test.Views.transform.GetChild(1);
                GameObject ghost = there.Find("Fantôme").gameObject;

                test.Views.ShowForesight(true);
                yield return null;
                Assert.IsFalse(here.activeSelf, "En prévision, il a quitté sa place…");
                Assert.IsFalse(ghost.activeSelf, "… et sa nouvelle place n'est plus un fantôme…");
                foreach (MeshRenderer part in there.GetComponentsInChildren<MeshRenderer>())
                    Assert.GreaterOrEqual(SiteViewsTests.LineOf(part), part.bounds.max.y - 1e-3f, "… il y est bâti.");

                test.Views.ShowForesight(false);
                yield return null;
                Assert.IsTrue(here.activeSelf, "Elle éteinte, il est encore à sa place…");
                Assert.IsTrue(ghost.activeSelf, "… et sa nouvelle place, un fantôme.");
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

        [UnityTest]
        public IEnumerator TheReverieFadesInAndOut()
        {
            using (var camera = new TestCamera())
            {
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                try
                {
                    var filter = camera.Holder.AddComponent<ForesightFilter>();
#if UNITY_EDITOR
                    TestWorld.Fill(camera.Settings, "_foresightBlendSeconds", 0.5f);
                    TestWorld.Fill(filter, "_profile", profile);
                    TestWorld.Fill(filter, "_settings", camera.Settings);
#endif
                    var foresight = new Foresight();
                    filter.Show(foresight);
                    var volume = filter.GetComponentInChildren<Volume>();
                    Assert.IsTrue(volume.isGlobal, "Un filtre sur tout l'écran…");
                    Assert.AreSame(profile, volume.sharedProfile, "… du profil de la rêverie…");
                    Assert.AreEqual(0f, volume.weight, "… éteint tant que la prévision l'est.");

                    foresight.Set(true);
                    yield return null;
                    Assert.That(volume.weight, Is.GreaterThan(0f).And.LessThan(1f), "Il entre en fondu…");
                    yield return new WaitForSecondsRealtime(0.6f);
                    Assert.AreEqual(1f, volume.weight, "… jusqu'au bout…");
                    foresight.Set(false);
                    yield return new WaitForSecondsRealtime(0.6f);
                    Assert.AreEqual(0f, volume.weight, "… et sort de même.");
                }
                finally
                {
                    Object.Destroy(profile);
                }
            }
        }

        [UnityTest]
        public IEnumerator TheButtonTogglesForesightAndStaysPressedWhileItLasts()
        {
            using (var test = new TestWorld())
            {
                UIDocument document = TestHud.WithWheel(test);
                yield return null;
                var button = new ForesightButton(document);
                var foresight = new Foresight();
                button.Show(foresight, TextTable.Parse(Texts, "fr"), new Tooltips(document), "V");
                yield return null;
                VisualElement element = document.rootVisualElement.Q("prevision");
                try
                {
                    Assert.IsFalse(element.ClassListContains("bloc-cache"), "Le bouton paraît…");
                    Background picture = element.Q(className: "prevision-icone").resolvedStyle.backgroundImage;
                    Assert.IsNotNull(picture.vectorImage, "… avec son icône vectorielle…");
                    Assert.AreEqual("V", document.rootVisualElement.Q<Label>("prevision-touche").text, "… sa touche.");
                    TestHud.Click(element);
                    Assert.IsTrue(foresight.IsOn, "Un clic l'allume…");
                    Assert.IsTrue(element.ClassListContains("ouvert"), "… le bouton reste enfoncé…");
                    foresight.Toggle();
                    Assert.IsFalse(element.ClassListContains("ouvert"), "… et remonte quand elle s'éteint par V.");
                }
                finally
                {
                    button.Hide();
                }
                Assert.IsTrue(element.ClassListContains("bloc-cache"), "À la sortie, il se cache…");
                TestHud.Click(element);
                Assert.IsFalse(foresight.IsOn, "… débranché.");
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
