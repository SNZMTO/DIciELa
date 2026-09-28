using System.Collections;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les habitants vus : un monde plat de 100 × 100 m à 10 m de haut, aux réglages par défaut (quatre mailles par
    /// mètre, 15 cm par pas), la Chimère créée par son ordre, son corps une capsule à la place du personnage du pack.
    /// Elle naît au centre, dans la maille (200 ; 200), dont le milieu est (50,125 ; 50,125).
    /// </summary>
    public sealed class VillagerViewsTests
    {
        private readonly List<Object> _created = new List<Object>();
        private World _world;
        private VillagerKindDefinition _kind;
        private VillagerViews _views;

        [SetUp]
        public void CreateWorld()
        {
            var settings = Keep(ScriptableObject.CreateInstance<SimulationSettings>());
            _kind = Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            GameObject model = Keep(GameObject.CreatePrimitive(PrimitiveType.Capsule));
#if UNITY_EDITOR
            TestWorld.Fill(_kind, "_id", "chimere");
#endif
            var map = new MapDescription(new Relief(100f, 100f, 2, 2, new[] { 10f, 10f, 10f, 10f }));
            _world = new World(settings, map);
            _views = Keep(new GameObject("Habitants de test")).AddComponent<VillagerViews>();
            _views.Show(_world.Villagers, map.Relief);
            _world.Submit(new CreateChimeraCommand(_kind, TestWorld.PlainAppearance(o => Keep(o), model)));
            _world.Step();
        }

        [TearDown]
        public void DestroyWorld()
        {
            _views.Hide();
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        private T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        private Transform Shown => _views.transform.GetChild(0);

        /// <summary>Joue des pas jusqu'à ce qu'elle bouge, après sa première pause (90 pas au plus).</summary>
        private Villager StepUntilSheMoves()
        {
            Villager chimera = _world.Villagers.Chimera;
            for (int step = 0; step < 500 && chimera.Position == chimera.PreviousPosition; step++)
                _world.Step();
            Assert.AreNotEqual(chimera.PreviousPosition, chimera.Position, "Elle ne part pas.");
            return chimera;
        }

        private static void AssertNear(Vector3 expected, Vector3 actual)
        {
            Assert.Less(Vector3.Distance(expected, actual), 1e-4f, "Attendu " + expected.ToString("F4") + ", vu " +
                                                                    actual.ToString("F4") + ".");
        }

        [Test]
        public void SheIsShownWhereSheIsBornOnTheGround()
        {
            Assert.AreEqual(1, _views.transform.childCount);
            Assert.AreEqual("chimere n° 1", Shown.name);
            AssertNear(new Vector3(50.125f, 10f, 50.125f), Shown.position);
        }

        [Test]
        public void BetweenTwoStepsSheIsShownOnTheWay()
        {
            Villager chimera = StepUntilSheMoves();

            _views.Follow(0.5f, 0f);
            Vector2 halfway = (chimera.PreviousPosition + chimera.Position) * 0.5f;
            AssertNear(new Vector3(halfway.x, 10f, halfway.y), Shown.position);

            _views.Follow(1f, 0f);
            AssertNear(new Vector3(chimera.Position.x, 10f, chimera.Position.y), Shown.position);
        }

        [Test]
        public void SheTurnsTowardWhereSheGoes()
        {
            Villager chimera = StepUntilSheMoves();

            // Dix secondes : bien assez pour se tourner, à 540° par seconde.
            _views.Follow(1f, 10f);

            Vector2 heading = (chimera.Position - chimera.PreviousPosition).normalized;
            AssertNear(new Vector3(heading.x, 0f, heading.y), Shown.forward);
        }

        [Test]
        public void AsleepSheIsHiddenAndWokenSheIsSeenAgain()
        {
            // Sa tente de 3 × 3 m, plantée en (50 ; 45), porte au nord : elle y va, s'y couche, et on ne la voit plus.
            var tent = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(tent, "_widthMeters", 3f);
            TestWorld.Fill(tent, "_depthMeters", 3f);
            TestWorld.Fill(tent, "_unique", true);
            TestWorld.Fill(tent, "_chimeraHome", true);
            TestWorld.Fill(tent, "_builtWhenPlaced", true);
#endif
            Villager chimera = _world.Villagers.Chimera;
            _world.Submit(new PlaceStructureCommand(tent, new Placement(new Vector2(50f, 45f), 0f)));
            _world.Submit(new SendChimeraToBedCommand());
            for (int step = 0; step < 2000 && !chimera.IsAsleep; step++)
                _world.Step();

            Assert.IsTrue(chimera.IsAsleep, "Elle a rejoint sa tente.");
            Assert.IsFalse(Shown.gameObject.activeSelf, "Endormie, elle est dans sa tente.");

            _world.Submit(new WakeChimeraCommand());
            _world.Step();
            Assert.IsTrue(Shown.gameObject.activeSelf, "Réveillée, elle sort.");
        }

        [UnityTest]
        public IEnumerator HidingRemovesTheCharacters()
        {
            _views.Hide();
            yield return null;

            Assert.AreEqual(0, _views.transform.childCount);
        }
    }
}
