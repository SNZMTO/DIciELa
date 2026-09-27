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
            float eveningFraction, int maxStepsPerFrame = 10)
        {
            SimulationSettings settings = ScriptableObject.CreateInstance<SimulationSettings>();
            var serialized = new SerializedObject(settings);
            Property(serialized, "_stepSeconds").floatValue = stepSeconds;
            Property(serialized, "_dayLengthSeconds").floatValue = dayLengthSeconds;
            Property(serialized, "_workFraction").floatValue = workFraction;
            Property(serialized, "_eveningFraction").floatValue = eveningFraction;
            Property(serialized, "_maxStepsPerFrame").intValue = maxStepsPerFrame;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }

        /// <summary>Une petite carte plate de 10 m de côté, à 0 m de hauteur.</summary>
        public static MapDescription FlatMap()
        {
            return new MapDescription(new Relief(10f, 10f, 2, 2, new float[4]));
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
