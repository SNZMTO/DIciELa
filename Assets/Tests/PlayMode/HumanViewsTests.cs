using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Un premier habitant vu (GDD, sections 3, 6 et 16) : un monde plat de 100 × 100 m aux réglages par défaut, un
    /// déroulé du départ qui sait bâtir une maison d'un lit, dressée dès la pose, et fait arriver un habitant dont l'allure
    /// est un personnage du pack, ici un cube. Il se montre tel quel ; endormi chez lui la nuit (une maison sans couche),
    /// on ne le voit plus ; levé au matin, on le revoit.
    /// </summary>
    public sealed class HumanViewsTests
    {
        private const int MaxSteps = 100000;
        private readonly List<Object> _created = new List<Object>();
        private World _world;
        private VillagerViews _views;
        private StructureDefinition _house;
        private GameObject _cube;

        [SetUp]
        public void CreateWorld()
        {
            var settings = Keep(ScriptableObject.CreateInstance<SimulationSettings>());
            var kind = Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            var character = Keep(ScriptableObject.CreateInstance<CharacterDefinition>());
            var first = Keep(ScriptableObject.CreateInstance<FirstVillagerDefinition>());
            var opening = Keep(ScriptableObject.CreateInstance<OpeningDefinition>());
            var catalog = Keep(ScriptableObject.CreateInstance<Catalog>());
            _house = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            _cube = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
#if UNITY_EDITOR
            TestWorld.Fill(kind, "_id", "habitant");
            TestWorld.Fill(kind, "_sleepsAtNight", true);
            TestWorld.Fill(character, "_model", _cube);
            TestWorld.Fill(first, "_character", character);
            TestWorld.Fill(_house, "_builtWhenPlaced", true);
            TestWorld.Fill(_house, "_beds", 1);
            TestWorld.FillList(opening, "_knownStructures", _house);
            TestWorld.FillList(opening, "_firstVillagers", first);
            TestWorld.Fill(catalog, "_opening", opening);
            TestWorld.Fill(catalog, "_villagerKind", kind);
#endif
            var map = new MapDescription(new Relief(100f, 100f, 2, 2, new[] { 10f, 10f, 10f, 10f }));
            _world = new World(settings, map, catalog);
            _views = Keep(new GameObject("Habitants de test")).AddComponent<VillagerViews>();
            _views.Show(_world.Villagers, map.Relief);
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

        [Test]
        public void TheArrivingHumanIsShownAsThePackMadeHimAndHiddenAsleepAtHome()
        {
            _world.Submit(new PlaceStructureCommand(_house, new Placement(new Vector2(60f, 50f), 0f)));
            _world.Step();

            Assert.AreEqual(1, _views.transform.childCount, "L'habitant arrive dès que la maison offre son lit.");
            Transform shown = _views.transform.GetChild(0);
            Assert.AreSame(_cube.GetComponent<MeshFilter>().sharedMesh, shown.GetComponent<MeshFilter>().sharedMesh,
                "Il est le personnage du pack, tel quel.");

            Villager human = _world.Villagers.All[0];
            for (int step = 0; step < MaxSteps && !human.IsAsleep; step++)
                _world.Step();
            Assert.IsTrue(human.IsAsleep, "La nuit, il est rentré chez lui.");
            Assert.IsFalse(shown.gameObject.activeSelf, "Endormi dans une maison sans couche, on ne le voit plus.");

            for (int step = 0; step < MaxSteps && _world.Clock.Phase != DayPhase.Work; step++)
                _world.Step();
            Assert.IsTrue(shown.gameObject.activeSelf, "Au matin, il sort.");
        }
    }
}
