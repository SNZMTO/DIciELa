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
    /// au centre, (50,125 ; 50,125) ; sa tente de 3 × 3 m, une boîte de 2,5 m de haut, est plantée en (50 ; 45), porte
    /// au nord. Toucher la tente l'envoie dormir, la toucher encore la réveille ; toucher autre chose ne fait rien ; chaque
    /// toucher, la tente comprise, est dit à qui l'écoute (la fiche d'un lieu s'y ferme). On
    /// vise le modèle, pas le sol : vu de biais, le haut de la tente cache le sol derrière elle. Bâtie, la tente se voit
    /// avec ses matériaux ; une cabane qui attend ses bâtisseurs, en fantôme translucide.
    /// </summary>
    public sealed class ChooseToolTests
    {
        private static readonly Vector3 TentTop = new Vector3(50f, 12.5f, 45f);

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
            _test.World.Submit(new PlaceStructureCommand(_test.Tent, new Placement(new Vector2(50f, 45f), 0f)));
            _test.World.Step();
            _tool = new ChooseTool(_test.World, _test.Camera, _test.Views);
        }

        [TearDown]
        public void DestroyWorld()
        {
            _test.Dispose();
        }

        private Villager Chimera => _test.World.Villagers.Chimera;

        [Test]
        public void TouchingTheTentSendsHerToBedAndTouchingAgainWakesHer()
        {
            Structure heard = null;
            _tool.Touched += touched => heard = touched;

            Assert.IsTrue(_tool.TouchAt(_test.OnScreen(TentTop)));
            Assert.AreSame(_test.World.Structures.ChimeraHome, heard,
                "Toucher la tente se dit aussi : la fiche ouverte se ferme.");
            _test.World.Step();
            Assert.IsTrue(Chimera.IsGoingToBed, "Elle va se coucher.");

            Assert.IsTrue(_tool.TouchAt(_test.OnScreen(TentTop)));
            _test.World.Step();
            Assert.IsFalse(Chimera.IsGoingToBed, "Touchée de nouveau, elle reste debout.");
            Assert.IsFalse(Chimera.IsAsleep);
        }

        [Test]
        public void TheTopOfTheTentIsTouchedWhereTheGroundBehindItIsNot()
        {
            // L'empreinte de la tente va de 48,5 à 51,5 en x, de 43,5 à 46,5 en z.
            Ray ray = _test.Camera.ScreenPointToRay(_test.OnScreen(TentTop));
            Assert.IsTrue(_test.World.Map.Relief.TryRaycast(ray.origin, ray.direction, 1000f, out Vector3 ground));
            bool groundInFootprint = ground.x >= 48.5f && ground.x <= 51.5f && ground.z >= 43.5f && ground.z <= 46.5f;
            Assert.IsFalse(groundInFootprint,
                "Le sol vu à travers le haut de la tente est hors de son empreinte : viser le sol la manquerait.");

            Assert.IsTrue(_test.Views.TryPick(ray, out Structure touched));
            Assert.AreSame(_test.World.Structures.ChimeraHome, touched);
        }

        [Test]
        public void TouchingTheGroundOrACabinDoesNothing()
        {
            _test.World.Submit(new PlaceStructureCommand(_test.Cabin, new Placement(new Vector2(60f, 60f), 0f)));
            _test.World.Step();

            Assert.IsFalse(_tool.TouchAt(_test.OnScreen(new Vector3(40f, 10f, 60f))), "Le sol nu.");
            Assert.IsFalse(_tool.TouchAt(_test.OnScreen(new Vector3(60f, 10.5f, 60f))), "Une cabane n'est pas son logis.");
            _test.World.Step();
            Assert.IsFalse(Chimera.IsGoingToBed);
        }

        [Test]
        public void TheBuiltTentShowsItsMaterialsAndACabinIsAGhost()
        {
            _test.World.Submit(new PlaceStructureCommand(_test.Cabin, new Placement(new Vector2(60f, 60f), 0f)));
            _test.World.Step();

            Material translucent = _test.Settings.TranslucentMaterial;
            Assert.AreNotSame(translucent, ViewOf(0).GetComponentInChildren<Renderer>().sharedMaterial, "La tente.");
            Assert.AreSame(translucent, ViewOf(1).GetComponentInChildren<Renderer>().sharedMaterial, "La cabane.");
        }

        private Transform ViewOf(int index)
        {
            return _test.Views.transform.GetChild(index);
        }
    }
}
