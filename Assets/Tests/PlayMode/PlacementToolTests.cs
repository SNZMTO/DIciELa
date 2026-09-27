using System.Collections;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'outil de pose, monté en mémoire : un monde plat de 100 × 100 m à 10 m de haut, la caméra de gestion qui le
    /// montre, l'aperçu et les constructions vues. On vise, on tourne, on pose et on annule par les mêmes appels que les
    /// gestes. La cabane est une boîte de 4 × 4 m, aux réglages par défaut des fiches.
    /// </summary>
    public sealed class PlacementToolTests
    {
        private static readonly Vector3 MapCenter = new Vector3(50f, 10f, 50f);

        private readonly List<Object> _created = new List<Object>();
        private World _world;
        private Camera _camera;
        private PlacementPreview _preview;
        private StructureViews _views;
        private StructureDefinition _cabin;
        private PlacementTool _tool;

        [SetUp]
        public void CreateWorld()
        {
            var simulation = Keep(ScriptableObject.CreateInstance<SimulationSettings>());
            var placement = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
            var cameraSettings = Keep(ScriptableObject.CreateInstance<CameraSettings>());
            var translucent = Keep(new Material(Shader.Find("Universal Render Pipeline/Lit")));
            GameObject model = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            _cabin = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            var map = new MapDescription(new Relief(100f, 100f, 2, 2, new[] { 10f, 10f, 10f, 10f }));
            _world = new World(simulation, map);

            var cameraObject = Keep(new GameObject("Caméra de test", typeof(Camera)));
            var rig = cameraObject.AddComponent<CameraRig>();
            _preview = Keep(new GameObject("Aperçu de test")).AddComponent<PlacementPreview>();
            _views = Keep(new GameObject("Constructions de test")).AddComponent<StructureViews>();
#if UNITY_EDITOR
            Fill(placement, "_translucentMaterial", translucent);
            Fill(_cabin, "_id", "cabane");
            Fill(_cabin, "_model", model);
            Fill(rig, "_settings", cameraSettings);
            Fill(_preview, "_settings", placement);
            Fill(_views, "_settings", placement);
#endif
            rig.Show(map.Relief);
            _camera = rig.Camera;
            _views.Show(_world.Structures, map.Relief);
            _tool = new PlacementTool(_world, _camera, _preview, placement);
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

#if UNITY_EDITOR
        private static void Fill(Object target, string field, object value)
        {
            var serialized = new UnityEditor.SerializedObject(target);
            UnityEditor.SerializedProperty property = serialized.FindProperty(field);
            if (value is string text)
                property.stringValue = text;
            else
                property.objectReferenceValue = (Object)value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
#endif

        private Vector2 OnScreen(Vector3 ground)
        {
            return _camera.WorldToScreenPoint(ground);
        }

        [Test]
        public void TheAimFollowsThePointerAndAClickPlaces()
        {
            _tool.Begin(_cabin);
            _tool.AimAt(OnScreen(MapCenter));

            Assert.AreEqual(PlacementResult.Ok, _tool.Result);
            Transform shown = _preview.transform.GetChild(0);
            Assert.That(Vector3.Distance(MapCenter, shown.position), Is.LessThan(0.05f));

            Assert.IsTrue(_tool.Confirm());
            _world.Step();

            Assert.AreEqual(1, _world.Structures.All.Count);
            Assert.That(Vector2.Distance(new Vector2(50f, 50f), _world.Structures.All[0].Placement.Center),
                Is.LessThan(0.05f));
            Assert.AreEqual(1, _views.transform.childCount);
        }

        [Test]
        public void AKeyPressTurnsTheNextStructureByOneStep()
        {
            _tool.Begin(_cabin);
            _tool.AimAt(OnScreen(MapCenter));

            _tool.RotateStep();
            _tool.Confirm();
            _world.Step();

            Assert.AreEqual(15f, _world.Structures.All[0].Placement.RotationDegrees, 1e-3f);
        }

        [Test]
        public void ATakenPlaceIsRefusedAndNothingIsSent()
        {
            _tool.Begin(_cabin);
            _tool.AimAt(OnScreen(MapCenter));
            _tool.Confirm();
            _world.Step();

            _tool.AimAt(OnScreen(MapCenter + new Vector3(1f, 0f, 1f)));

            Assert.AreEqual(PlacementResult.Overlaps, _tool.Result);
            Assert.IsFalse(_tool.Confirm());
            _world.Step();
            Assert.AreEqual(1, _world.Structures.All.Count);
        }

        [UnityTest]
        public IEnumerator CancellingPutsTheToolAside()
        {
            _tool.Begin(_cabin);
            _tool.AimAt(OnScreen(MapCenter));

            _tool.Cancel();
            yield return null;

            Assert.IsFalse(_tool.IsActive);
            Assert.AreEqual(0, _preview.transform.childCount);
            Assert.IsFalse(_tool.Confirm());
        }
    }
}
