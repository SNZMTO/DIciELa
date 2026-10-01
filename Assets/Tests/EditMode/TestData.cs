using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La seule fabrique de fiches des tests : elle crée une fiche en mémoire et la remplit par SerializedObject, le
    /// chemin de l'Inspector (ARCHITECTURE, section 12). Chaque test détruit ses fiches en fin de test.
    /// </summary>
    internal static partial class TestData
    {
        public static SimulationSettings Settings(float stepSeconds, float dayLengthSeconds, float workFraction,
            float eveningFraction, int maxStepsPerFrame = 10, int cellsPerMeter = 2, float maxBuildSlopeDegrees = 15f,
            float doorClearanceMeters = 1f)
        {
            SimulationSettings settings = ScriptableObject.CreateInstance<SimulationSettings>();
            var serialized = new SerializedObject(settings);
            Property(serialized, "_stepSeconds").floatValue = stepSeconds;
            Property(serialized, "_dayLengthSeconds").floatValue = dayLengthSeconds;
            Property(serialized, "_workFraction").floatValue = workFraction;
            Property(serialized, "_eveningFraction").floatValue = eveningFraction;
            Property(serialized, "_maxStepsPerFrame").intValue = maxStepsPerFrame;
            Property(serialized, "_cellsPerMeter").intValue = cellsPerMeter;
            Property(serialized, "_maxBuildSlopeDegrees").floatValue = maxBuildSlopeDegrees;
            Property(serialized, "_doorClearanceMeters").floatValue = doorClearanceMeters;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }

        /// <summary>Une construction de width × depth mètres, avec ou sans porte (au milieu de sa façade nord).</summary>
        public static StructureDefinition Structure(string id, float width, float depth, bool hasDoor = true,
            float doorWidth = 1f)
        {
            StructureDefinition definition = ScriptableObject.CreateInstance<StructureDefinition>();
            var serialized = new SerializedObject(definition);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_widthMeters").floatValue = width;
            Property(serialized, "_depthMeters").floatValue = depth;
            Property(serialized, "_hasDoor").boolValue = hasDoor;
            Property(serialized, "_doorWidthMeters").floatValue = doorWidth;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        /// <summary>
        /// Un logis de la Chimère, unique, de size mètres de côté, porte de 1 m au milieu de sa façade nord (GDD, section
        /// 6). Par défaut, le campement : dressé dès la pose ; sinon, une maison qui attend ses bâtisseurs.
        /// </summary>
        public static StructureDefinition Home(string id = "campement", float size = 3f, bool builtWhenPlaced = true,
            int capacity = 0, params WorkSlotDefinition[] slots)
        {
            StructureDefinition home = Structure(id, size, size);
            var serialized = new SerializedObject(home);
            Property(serialized, "_unique").boolValue = true;
            Property(serialized, "_chimeraHome").boolValue = true;
            Property(serialized, "_builtWhenPlaced").boolValue = builtWhenPlaced;
            Property(serialized, "_storageCapacity").intValue = capacity;
            Fill(Property(serialized, "_workSlots"), slots);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return home;
        }

        /// <summary>
        /// Une carte plate de size mètres de côté (10 par défaut), à 0 m de hauteur ; son point d'arrivée, s'il est
        /// donné, sinon le centre.
        /// </summary>
        public static MapDescription FlatMap(float size = 10f, Vector2? arrival = null,
            IReadOnlyList<NatureSpot> nature = null)
        {
            return new MapDescription(new Relief(size, size, 2, 2, new float[4]), arrival, nature);
        }

        /// <summary>
        /// Une sorte d'habitant qui marche à walk mètres par seconde et flâne dans ce rayon, avec des pauses de pauseMin
        /// à pauseMax heures de jeu ; sans personnage, que la simulation ne lit pas.
        /// </summary>
        public static VillagerKindDefinition VillagerKind(string id = "chimere", float walk = 1.5f, float radius = 6f,
            float pauseMin = 0.1f, float pauseMax = 0.3f, float minHop = 2f)
        {
            VillagerKindDefinition kind = ScriptableObject.CreateInstance<VillagerKindDefinition>();
            var serialized = new SerializedObject(kind);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_walkMetersPerSecond").floatValue = walk;
            Property(serialized, "_wanderRadiusMeters").floatValue = radius;
            Property(serialized, "_wanderMinHopMeters").floatValue = minHop;
            Property(serialized, "_wanderPauseMinHours").floatValue = pauseMin;
            Property(serialized, "_wanderPauseMaxHours").floatValue = pauseMax;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return kind;
        }

        /// <summary>Une couleur de corps, sans matériau : la simulation ne le lit pas.</summary>
        public static PaletteDefinition Palette(string id)
        {
            PaletteDefinition palette = ScriptableObject.CreateInstance<PaletteDefinition>();
            var serialized = new SerializedObject(palette);
            Property(serialized, "_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return palette;
        }

        /// <summary>
        /// Une tête sans maillage (la simulation ne le lit pas), dans cette couleur d'origine, qui porte ces pièces d'office.
        /// </summary>
        public static HeadDefinition Head(string id, PaletteDefinition palette, params AttachmentDefinition[] worn)
        {
            HeadDefinition head = ScriptableObject.CreateInstance<HeadDefinition>();
            var serialized = new SerializedObject(head);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_palette").objectReferenceValue = palette;
            Fill(Property(serialized, "_worn"), worn);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return head;
        }

        /// <summary>
        /// Un corps sans personnage ni maillage (la simulation ne les lit pas), dans cette couleur d'origine, qui porte ces
        /// pièces dans le pack.
        /// </summary>
        public static BodyDefinition Body(string id, PaletteDefinition palette, params AttachmentDefinition[] worn)
        {
            BodyDefinition body = ScriptableObject.CreateInstance<BodyDefinition>();
            var serialized = new SerializedObject(body);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_palette").objectReferenceValue = palette;
            SerializedProperty list = Property(serialized, "_worn");
            list.arraySize = worn.Length;
            for (int i = 0; i < worn.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = worn[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return body;
        }

        /// <summary>Une pièce sans modèle à cet emplacement (sans emplacement si slot est null).</summary>
        public static AttachmentDefinition Attachment(string id, AttachmentSlotDefinition slot)
        {
            AttachmentDefinition attachment = ScriptableObject.CreateInstance<AttachmentDefinition>();
            var serialized = new SerializedObject(attachment);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_slot").objectReferenceValue = slot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return attachment;
        }

        /// <summary>Un catalogue qui ne propose que ces têtes et ces corps, dans cet ordre.</summary>
        public static Catalog Catalog(HeadDefinition[] heads, BodyDefinition[] bodies)
        {
            Catalog catalog = ScriptableObject.CreateInstance<Catalog>();
            var serialized = new SerializedObject(catalog);
            Fill(Property(serialized, "_heads"), heads);
            Fill(Property(serialized, "_bodies"), bodies);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static void Fill(SerializedProperty list, UnityEngine.Object[] items)
        {
            list.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        public static AttachmentSlotDefinition Slot(string id)
        {
            AttachmentSlotDefinition slot = ScriptableObject.CreateInstance<AttachmentSlotDefinition>();
            var serialized = new SerializedObject(slot);
            Property(serialized, "_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return slot;
        }

        /// <summary>Une carte carrée de size mètres, un point par mètre, dont la hauteur monte de riseX mètres par mètre.</summary>
        public static MapDescription SlopedMap(int size, float riseX)
        {
            int samples = size + 1;
            var heights = new float[samples * samples];
            for (int z = 0; z < samples; z++)
            {
                for (int x = 0; x < samples; x++)
                    heights[z * samples + x] = riseX * x;
            }
            return new MapDescription(new Relief(size, size, samples, samples, heights));
        }

        private static SerializedProperty Property(SerializedObject serialized, string name)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null)
                throw new InvalidOperationException("La fiche " + serialized.targetObject.GetType().Name +
                                                    " n'a pas de champ " + name + ".");
            return property;
        }
    }
}
