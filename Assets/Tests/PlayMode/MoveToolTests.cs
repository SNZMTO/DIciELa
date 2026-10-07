using Diciela.Controls;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'outil Déplacer (décisions 199 et 201), sur le petit monde des tests, par les mêmes appels que les gestes : on
    /// touche une construction en visant son modèle, son fantôme suit le pointeur, un clic l'envoie ; le clic droit
    /// repose le fantôme, puis l'outil. La cabane de 4 × 4 m, un chantier, a pour modèle un cube d'un mètre ; le
    /// campement de 3 × 3 m, dressé dès la pose, une boîte de 2,5 m de haut. Le petit monde n'a pas d'habitant : un
    /// bâtiment qu'on déplace y attend son ouvrier.
    /// </summary>
    public sealed class MoveToolTests
    {
        private TestWorld _test;

        [SetUp]
        public void CreateWorld()
        {
            _test = new TestWorld();
        }

        [TearDown]
        public void DestroyWorld()
        {
            _test.Dispose();
        }

        [Test]
        public void ACabinTouchedFollowsThePointerAndAClickMovesIt()
        {
            Structure cabin = TestMove.Place(_test, _test.Cabin, TestWorld.Center);
            _test.Mover.Take();
            Assert.IsTrue(TestMove.Touch(_test, TestWorld.Center, 0.5f), "On touche la cabane en visant son modèle.");
            Assert.AreSame(cabin, _test.Mover.Moving);

            Vector3 target = TestWorld.Center + new Vector3(10f, 0f, 0f);
            _test.Mover.AimAt(_test.OnScreen(target));
            Assert.AreEqual(PlacementResult.Ok, _test.Mover.Result);
            Transform ghost = _test.Preview.transform.GetChild(_test.Preview.transform.childCount - 1);
            Assert.That(Vector3.Distance(target, ghost.position), Is.LessThan(0.05f), "Son fantôme suit le pointeur.");
            Assert.IsTrue(_test.Mover.Confirm());
            _test.World.Step();

            Assert.That(Vector2.Distance(new Vector2(60f, 50f), cabin.Placement.Center), Is.LessThan(0.05f),
                "Un chantier saute tout de suite.");
            Assert.IsTrue(_test.Mover.IsActive, "L'outil reste en main…");
            Assert.IsNull(_test.Mover.Moving, "… sans fantôme : il attend la suivante.");
        }

        [Test]
        public void ARightClickPutsTheGhostBackThenTheTool()
        {
            TestMove.Place(_test, _test.Cabin, TestWorld.Center);
            _test.Mover.Take();
            TestMove.Touch(_test, TestWorld.Center, 0.5f);

            _test.Mover.Cancel();
            Assert.IsTrue(_test.Mover.IsActive, "Le fantôme reposé, l'outil reste en main.");
            Assert.IsNull(_test.Mover.Moving);
            _test.Mover.Cancel();
            Assert.IsFalse(_test.Mover.IsActive, "Sans fantôme, le clic droit lâche l'outil.");
        }

        [Test]
        public void ATakenPlaceIsRefusedAndTheGhostStays()
        {
            Structure cabin = TestMove.Place(_test, _test.Cabin, TestWorld.Center);
            Vector3 other = TestWorld.Center + new Vector3(10f, 0f, 0f);
            TestMove.Place(_test, _test.Cabin, other);
            _test.Mover.Take();
            TestMove.Touch(_test, TestWorld.Center, 0.5f);

            _test.Mover.AimAt(_test.OnScreen(other + new Vector3(1f, 0f, 1f)));
            Assert.AreEqual(PlacementResult.Overlaps, _test.Mover.Result);
            Assert.IsFalse(_test.Mover.Confirm(), "Rien ne part.");
            Assert.AreSame(cabin, _test.Mover.Moving, "Le fantôme suit encore le pointeur.");
        }

        [Test]
        public void ABuildingOnItsWayShowsItsNewPlaceAndIsNotTouchedAgain()
        {
            Structure camp = TestMove.Place(_test, _test.Camp, TestWorld.Center);
            _test.Mover.Take();
            Assert.IsTrue(TestMove.Touch(_test, TestWorld.Center, 1.25f));
            Vector3 target = TestWorld.Center + new Vector3(10f, 0f, 0f);
            _test.Mover.AimAt(_test.OnScreen(target));
            _test.Mover.Confirm();
            _test.World.Step();

            Assert.That(Vector2.Distance(new Vector2(60f, 50f), camp.MovingTo.Value.Center), Is.LessThan(0.05f),
                "Sa nouvelle place est retenue.");
            Assert.AreEqual(2, _test.Views.transform.childCount, "Son modèle, et le fantôme de sa nouvelle place.");
            Assert.IsFalse(TestMove.Touch(_test, target, 1.25f), "Le fantôme de sa nouvelle place ne se touche pas.");
            Assert.IsFalse(TestMove.Touch(_test, TestWorld.Center, 1.25f), "On ne reprend pas ce qu'on déplace déjà.");
            Assert.IsNull(_test.Mover.Moving);
        }

        [Test]
        public void TakingAnotherToolPutsTheMoverDownWithItsGhost()
        {
            TestMove.Place(_test, _test.Cabin, TestWorld.Center);
            using (var hand = new Hand(_test.Tool, _test.Clearing, _test.Mover))
            {
                _test.Mover.Take();
                TestMove.Touch(_test, TestWorld.Center, 0.5f);

                _test.Tool.Begin(_test.Cabin);
                Assert.IsTrue(hand.IsHolding);
                Assert.IsFalse(_test.Mover.IsActive, "Prendre une carte lâche l'outil Déplacer…");
                Assert.IsNull(_test.Mover.Moving, "… et son fantôme.");
            }
        }
    }
}
