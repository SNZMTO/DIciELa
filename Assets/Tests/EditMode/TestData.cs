using System;
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
    internal static class TestData
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

        /// <summary>Une petite carte plate de 10 m de côté, à 0 m de hauteur.</summary>
        public static MapDescription FlatMap()
        {
            return new MapDescription(new Relief(10f, 10f, 2, 2, new float[4]));
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
