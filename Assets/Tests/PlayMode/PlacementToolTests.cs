using System.Collections;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'outil de pose, sur le petit monde des tests : on vise, on tourne, on pose et on annule par les mêmes appels que
    /// les gestes. La cabane fait 4 × 4 m ; le cran de rotation vaut sa valeur par défaut, 15°.
    /// </summary>
    public sealed class PlacementToolTests
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
        public void TheAimFollowsThePointerAndAClickPlaces()
        {
            _test.Tool.Begin(_test.Cabin);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center));

            Assert.AreEqual(PlacementResult.Ok, _test.Tool.Result);
            Transform shown = _test.Preview.transform.GetChild(0);
            Assert.That(Vector3.Distance(TestWorld.Center, shown.position), Is.LessThan(0.05f));

            Assert.IsTrue(_test.Tool.Confirm());
            _test.World.Step();

            Assert.AreEqual(1, _test.World.Structures.All.Count);
            Assert.That(Vector2.Distance(new Vector2(50f, 50f), _test.World.Structures.All[0].Placement.Center),
                Is.LessThan(0.05f));
            Assert.AreEqual(1, _test.Views.transform.childCount);
        }

        [Test]
        public void AKeyPressTurnsTheNextStructureByOneStep()
        {
            _test.Tool.Begin(_test.Cabin);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center));

            _test.Tool.RotateStep();
            _test.Tool.Confirm();
            _test.World.Step();

            Assert.AreEqual(15f, _test.World.Structures.All[0].Placement.RotationDegrees, 1e-3f);
        }

        [Test]
        public void ATakenPlaceIsRefusedAndNothingIsSent()
        {
            _test.Tool.Begin(_test.Cabin);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center));
            _test.Tool.Confirm();
            _test.World.Step();

            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center + new Vector3(1f, 0f, 1f)));

            Assert.AreEqual(PlacementResult.Overlaps, _test.Tool.Result);
            Assert.IsFalse(_test.Tool.Confirm());
            _test.World.Step();
            Assert.AreEqual(1, _test.World.Structures.All.Count);
        }

        [UnityTest]
        public IEnumerator CancellingPutsTheToolAside()
        {
            _test.Tool.Begin(_test.Cabin);
            _test.Tool.AimAt(_test.OnScreen(TestWorld.Center));

            _test.Tool.Cancel();
            yield return null;

            Assert.IsFalse(_test.Tool.IsActive);
            Assert.AreEqual(0, _test.Preview.transform.childCount);
            Assert.IsFalse(_test.Tool.Confirm());
        }
    }
}
