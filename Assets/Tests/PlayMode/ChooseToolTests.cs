using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'outil « choisir » et les constructions vues, sur le petit monde des tests (le sol à 10 m) : la Chimère y naît
    /// au centre, (50,125 ; 50,125) ; son campement de 3 × 3 m, une boîte de 2,5 m de haut, est installé en (50 ; 45), porte
    /// au nord. Chaque toucher est dit à qui l'écoute (la fiche d'un lieu s'y ouvre ou s'y ferme), sans envoyer d'ordre :
    /// la Chimère s'envoie dormir depuis la fiche de son logis (décision 172) ; toucher le sol ne touche rien. On
    /// vise le modèle, pas le sol : vu de biais, le haut du campement cache le sol derrière lui. Bâti, le campement se voit
    /// avec ses matériaux ; une cabane qui attend ses bâtisseurs, en chantier (décision 166).
    /// </summary>
    public sealed class ChooseToolTests
    {
        private static readonly Vector3 CampTop = new Vector3(50f, 12.5f, 45f);

        private TestWorld _test;
        private ChooseTool _tool;

        [SetUp]
        public void CreateWorld()
        {
            _test = new TestWorld();
            var kind = _test.Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(kind, "_id", "chimere");
#endif
            _test.World.Submit(new CreateChimeraCommand(kind, TestWorld.PlainAppearance(o => _test.Keep(o))));
            _test.World.Submit(new PlaceStructureCommand(_test.Camp, new Placement(new Vector2(50f, 45f), 0f)));
            _test.World.Step();
            _tool = new ChooseTool(_test.Camera, _test.Views);
        }

        [TearDown]
        public void DestroyWorld()
        {
            _test.Dispose();
        }

        private Villager Chimera => _test.World.Villagers.Chimera;

        [Test]
        public void TouchingTheCampSaysSoAndSendsNoOrder()
        {
            Structure heard = null;
            _tool.Touched += touched => heard = touched;

            Assert.IsTrue(_tool.TouchAt(_test.OnScreen(CampTop)));
            Assert.AreSame(_test.World.Structures.ChimeraHome, heard, "Toucher le campement se dit : sa fiche s'ouvre.");
            _test.World.Step();
            Assert.IsFalse(Chimera.IsGoingToBed, "Le toucher n'envoie plus dormir : c'est un bouton de la fiche.");
        }

        [Test]
        public void TheTopOfTheCampIsTouchedWhereTheGroundBehindItIsNot()
        {
            // L'empreinte du campement va de 48,5 à 51,5 en x, de 43,5 à 46,5 en z.
            Ray ray = _test.Camera.ScreenPointToRay(_test.OnScreen(CampTop));
            Assert.IsTrue(_test.World.Map.Relief.TryRaycast(ray.origin, ray.direction, 1000f, out Vector3 ground));
            bool groundInFootprint = ground.x >= 48.5f && ground.x <= 51.5f && ground.z >= 43.5f && ground.z <= 46.5f;
            Assert.IsFalse(groundInFootprint,
                "Le sol vu à travers le haut du campement est hors de son empreinte : viser le sol la manquerait.");

            Assert.IsTrue(_test.Views.TryPick(ray, out Structure touched));
            Assert.AreSame(_test.World.Structures.ChimeraHome, touched);
        }

        [Test]
        public void TouchingTheGroundTouchesNothingAndACabinIsTouched()
        {
            _test.World.Submit(new PlaceStructureCommand(_test.Cabin, new Placement(new Vector2(60f, 60f), 0f)));
            _test.World.Step();
            Structure heard = _test.World.Structures.ChimeraHome;
            _tool.Touched += touched => heard = touched;

            Assert.IsFalse(_tool.TouchAt(_test.OnScreen(new Vector3(40f, 10f, 60f))), "Le sol nu.");
            Assert.IsNull(heard, "Toucher rien se dit aussi : la fiche ouverte se ferme.");
            Assert.IsTrue(_tool.TouchAt(_test.OnScreen(new Vector3(60f, 10.5f, 60f))), "La cabane.");
            Assert.IsNotNull(heard);
            Assert.AreNotSame(_test.World.Structures.ChimeraHome, heard);
        }

        [Test]
        public void TheBuiltCampShowsItsMaterialsAndACabinIsASite()
        {
            _test.World.Submit(new PlaceStructureCommand(_test.Cabin, new Placement(new Vector2(60f, 60f), 0f)));
            _test.World.Step();

            Shader site = _test.Settings.SiteMaterial.shader;
            Assert.AreNotSame(site, ViewOf(0).GetComponentInChildren<Renderer>().sharedMaterial.shader, "Le campement.");
            Assert.AreSame(site, ViewOf(1).GetComponentInChildren<Renderer>().sharedMaterial.shader, "La cabane.");
        }

        private Transform ViewOf(int index)
        {
            return _test.Views.transform.GetChild(index);
        }
    }
}
