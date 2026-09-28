using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Un petit monde monté en mémoire pour les tests de la pose : plat, 100 × 100 m à 10 m de haut ; la caméra de
    /// gestion qui le montre ; l'aperçu, les constructions vues, une cabane de 4 × 4 m (une boîte) et l'outil de pose.
    /// Les fiches ont leurs valeurs par défaut, remplies par SerializedObject, le chemin de l'Inspector.
    /// </summary>
    internal sealed class TestWorld : IDisposable
    {
        public static readonly Vector3 Center = new Vector3(50f, 10f, 50f);

        private readonly List<Object> _created = new List<Object>();

        public TestWorld()
        {
            var simulation = Keep(ScriptableObject.CreateInstance<SimulationSettings>());
            Settings = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
            var cameraSettings = Keep(ScriptableObject.CreateInstance<CameraSettings>());
            var translucent = Keep(new Material(Shader.Find("Universal Render Pipeline/Lit")));
            GameObject model = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            Cabin = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            var map = new MapDescription(new Relief(100f, 100f, 2, 2, new[] { 10f, 10f, 10f, 10f }));
            World = new World(simulation, map);

            var rig = Keep(new GameObject("Caméra de test", typeof(Camera))).AddComponent<CameraRig>();
            Preview = Keep(new GameObject("Aperçu de test")).AddComponent<PlacementPreview>();
            Views = Keep(new GameObject("Constructions de test")).AddComponent<StructureViews>();
#if UNITY_EDITOR
            Fill(Settings, "_translucentMaterial", translucent);
            Fill(Cabin, "_id", "cabane");
            Fill(Cabin, "_nameKey", "construction.cabane");
            Fill(Cabin, "_model", model);
            Fill(rig, "_settings", cameraSettings);
            Fill(Preview, "_settings", Settings);
            Fill(Views, "_settings", Settings);
#endif
            rig.Show(map);
            Camera = rig.Camera;
            Views.Show(World.Structures, map.Relief);
            Tool = new PlacementTool(World, Camera, Preview, Settings);
        }

        public World World { get; }
        public PlacementSettings Settings { get; }
        public Camera Camera { get; }
        public PlacementPreview Preview { get; }
        public StructureViews Views { get; }
        public StructureDefinition Cabin { get; }
        public PlacementTool Tool { get; }

        /// <summary>Le point de l'écran où l'on voit ce point du sol.</summary>
        public Vector2 OnScreen(Vector3 ground)
        {
            return Camera.WorldToScreenPoint(ground);
        }

        /// <summary>Garde une chose créée pour le test, détruite avec le monde.</summary>
        public T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        public void Dispose()
        {
            Tool.Dispose();
            Views.Hide();
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

#if UNITY_EDITOR
        /// <summary>Remplit un champ par SerializedObject, comme l'Inspector.</summary>
        public static void Fill(Object target, string field, object value)
        {
            var serialized = new UnityEditor.SerializedObject(target);
            UnityEditor.SerializedProperty property = serialized.FindProperty(field);
            if (value is string text)
                property.stringValue = text;
            else if (value is float number)
                property.floatValue = number;
            else
                property.objectReferenceValue = (Object)value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }
}
