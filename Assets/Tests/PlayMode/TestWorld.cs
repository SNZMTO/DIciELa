using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Un petit monde monté en mémoire pour les tests de la pose : plat, 100 × 100 m à 10 m de haut ; la caméra de
    /// gestion qui le montre ; l'aperçu, les constructions vues, une cabane de 4 × 4 m (un cube d'un mètre pour modèle),
    /// une tente de 3 × 3 m (unique, logis de la Chimère, dressée dès la pose ; une boîte de 3 × 2,5 × 3 m pour modèle)
    /// et l'outil de pose. Les fiches ont leurs valeurs par défaut, remplies par SerializedObject, le chemin de
    /// l'Inspector.
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
            GameObject tentModel = Keep(new GameObject("Tente de test"));
            GameObject tentBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tentBox.transform.SetParent(tentModel.transform, false);
            tentBox.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            tentBox.transform.localScale = new Vector3(3f, 2.5f, 3f);
            Cabin = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            Tent = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
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
            Fill(Tent, "_id", "tente");
            Fill(Tent, "_nameKey", "construction.tente");
            Fill(Tent, "_widthMeters", 3f);
            Fill(Tent, "_depthMeters", 3f);
            Fill(Tent, "_doorWidthMeters", 1.2f);
            Fill(Tent, "_unique", true);
            Fill(Tent, "_chimeraHome", true);
            Fill(Tent, "_builtWhenPlaced", true);
            Fill(Tent, "_model", tentModel);
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
        public StructureDefinition Tent { get; }
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

        /// <summary>
        /// Une apparence sans pièce : un corps de ce modèle (aucun si null), dans une couleur sans matériau ; ses fiches
        /// sont confiées à keep, pour être détruites avec le test.
        /// </summary>
        public static Appearance PlainAppearance(Action<Object> keep, GameObject model = null)
        {
            var palette = ScriptableObject.CreateInstance<PaletteDefinition>();
            var body = ScriptableObject.CreateInstance<BodyDefinition>();
            keep(palette);
            keep(body);
#if UNITY_EDITOR
            Fill(palette, "_id", "roux");
            Fill(body, "_id", "chat");
            Fill(body, "_palette", palette);
            if (model != null)
                Fill(body, "_model", model);
#endif
            return new Appearance(body, palette, Array.Empty<AttachmentDefinition>());
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
            else if (value is bool flag)
                property.boolValue = flag;
            else if (value is Vector3 vector)
                property.vector3Value = vector;
            else
                property.objectReferenceValue = (Object)value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Remplit une liste d'une fiche par SerializedObject, comme l'Inspector.</summary>
        public static void FillList(Object target, string field, params Object[] items)
        {
            var serialized = new UnityEditor.SerializedObject(target);
            UnityEditor.SerializedProperty list = serialized.FindProperty(field);
            list.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }
}
