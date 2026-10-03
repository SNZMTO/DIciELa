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
    /// Un petit monde monté en mémoire pour les tests de la pose : 100 × 100 m, plat à 10 m de haut (ou sur le relief
    /// qu'on lui donne) ; la caméra de gestion qui le montre ; l'aperçu, les constructions vues, une cabane de 4 × 4 m
    /// (un cube d'un mètre pour modèle), un campement de 3 × 3 m (unique, logis de la Chimère, dressé dès la pose ; une
    /// boîte de 3 × 2,5 × 3 m pour modèle), l'outil de pose et l'outil « Retirer » (sans arbre vu : la carte n'en a pas).
    /// Les fiches ont leurs valeurs par défaut, remplies par SerializedObject, le chemin de l'Inspector.
    /// </summary>
    internal sealed class TestWorld : IDisposable
    {
        public static readonly Vector3 Center = new Vector3(50f, 10f, 50f);

        private readonly List<Object> _created = new List<Object>();

        public TestWorld(Relief relief = null)
        {
            var simulation = Keep(ScriptableObject.CreateInstance<SimulationSettings>());
            Settings = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
            var cameraSettings = Keep(ScriptableObject.CreateInstance<CameraSettings>());
            var translucent = Keep(new Material(Shader.Find("Universal Render Pipeline/Lit")));
            var site = Keep(new Material(Shader.Find("Diciela/Chantier")));
            var siteGhost = Keep(new Material(Shader.Find("Diciela/ChantierFantome")));
            GameObject model = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            GameObject campModel = Keep(new GameObject("Campement de test"));
            GameObject campBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            campBox.transform.SetParent(campModel.transform, false);
            campBox.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            campBox.transform.localScale = new Vector3(3f, 2.5f, 3f);
            Cabin = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            Camp = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            var map = new MapDescription(relief ?? new Relief(100f, 100f, 2, 2, new[] { 10f, 10f, 10f, 10f }));
            World = new World(simulation, map, Keep(ScriptableObject.CreateInstance<Catalog>()));

            var rig = Keep(new GameObject("Caméra de test", typeof(Camera))).AddComponent<CameraRig>();
            Preview = Keep(new GameObject("Aperçu de test")).AddComponent<PlacementPreview>();
            Views = Keep(new GameObject("Constructions de test")).AddComponent<StructureViews>();
#if UNITY_EDITOR
            Fill(Settings, "_translucentMaterial", translucent);
            Fill(Settings, "_siteMaterial", site);
            Fill(Settings, "_siteGhostMaterial", siteGhost);
            Fill(Cabin, "_id", "cabane");
            Fill(Cabin, "_nameKey", "construction.cabane");
            Fill(Cabin, "_model", model);
            Fill(Camp, "_id", "campement");
            Fill(Camp, "_nameKey", "construction.campement");
            Fill(Camp, "_widthMeters", 3f);
            Fill(Camp, "_depthMeters", 3f);
            Fill(Camp, "_doorWidthMeters", 1.2f);
            Fill(Camp, "_unique", true);
            Fill(Camp, "_chimeraHome", true);
            Fill(Camp, "_builtWhenPlaced", true);
            Fill(Camp, "_model", campModel);
            Fill(rig, "_settings", cameraSettings);
            Fill(Preview, "_settings", Settings);
            Fill(Views, "_settings", Settings);
#endif
            rig.Show(map);
            Camera = rig.Camera;
            Views.Show(World.Structures, World.ConstructionSites, map.Relief);
            Tool = new PlacementTool(World, Camera, Preview, Settings);
            Clearing = new ClearingTool(World, Camera, Keep(new GameObject("Arbres de test")).AddComponent<NatureViews>());
            Mover = new MoveTool(World, Camera, Views, Preview, Settings);
        }

        public World World { get; }
        public PlacementSettings Settings { get; }
        public Camera Camera { get; }
        public PlacementPreview Preview { get; }
        public StructureViews Views { get; }
        public StructureDefinition Cabin { get; }
        public StructureDefinition Camp { get; }
        public PlacementTool Tool { get; }
        public ClearingTool Clearing { get; }
        public MoveTool Mover { get; }

        /// <summary>Le point de l'écran où l'on voit ce point du sol.</summary>
        public Vector2 OnScreen(Vector3 ground)
        {
            return Camera.WorldToScreenPoint(ground);
        }

        /// <summary>
        /// Pose, par l'ordre du joueur, une haie de 2 × 2 m en (70 ; 70), dont le modèle est un quad vêtu de ce matériau :
        /// un chantier ; rend sa vue, la dernière des constructions vues.
        /// </summary>
        public Transform PlaceASiteDressedIn(Material material)
        {
            GameObject model = Keep(GameObject.CreatePrimitive(PrimitiveType.Quad));
            model.GetComponent<MeshRenderer>().sharedMaterial = material;
            var hedge = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            Fill(hedge, "_id", "haie");
            Fill(hedge, "_widthMeters", 2f);
            Fill(hedge, "_depthMeters", 2f);
            Fill(hedge, "_model", model);
#endif
            World.Submit(new PlaceStructureCommand(hedge, new Placement(new Vector2(70f, 70f), 0f)));
            World.Step();
            return Views.transform.GetChild(Views.transform.childCount - 1);
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
            Mover.Dispose();
            Views.Hide();
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        /// <summary>
        /// Une apparence sans pièce ni modèle, pour un monde que personne ne regarde : une tête et un corps d'une couleur
        /// sans matériau ; ses fiches sont confiées à keep, pour être détruites avec le test.
        /// </summary>
        public static Appearance PlainAppearance(Action<Object> keep)
        {
            var palette = ScriptableObject.CreateInstance<PaletteDefinition>();
            var head = ScriptableObject.CreateInstance<HeadDefinition>();
            var body = ScriptableObject.CreateInstance<BodyDefinition>();
            keep(palette);
            keep(head);
            keep(body);
            return new Appearance(head, palette, body, palette, Array.Empty<WornAttachment>());
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
            else if (value is int whole)
                property.intValue = whole;
            else if (value is Vector3 vector)
                property.vector3Value = vector;
            else if (value is Enum choice)
                property.enumValueIndex = Convert.ToInt32(choice);
            else if (value is SurfaceMap map)
                property.boxedValue = map;
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
