using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La fabrique de fiches des tests, suite : les sols (décisions 209 et 210). Même règle : en mémoire, remplies par
    /// SerializedObject.
    /// </summary>
    internal static partial class TestData
    {
        /// <summary>Une sorte de sol, où une charrette tirée va à tant de fois le pas de qui la tire.</summary>
        public static FloorDefinition Floor(string id, float cartSpeedFactor)
        {
            FloorDefinition floor = ScriptableObject.CreateInstance<FloorDefinition>();
            var serialized = new SerializedObject(floor);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_cartSpeedFactor").floatValue = cartSpeedFactor;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return floor;
        }

        /// <summary>Dit, dans ce déroulé du départ, les sols que la ville sait tracer au départ.</summary>
        public static void KnowsFloors(OpeningDefinition opening, params FloorDefinition[] floors)
        {
            var serialized = new SerializedObject(opening);
            Fill(Property(serialized, "_knownFloors"), floors);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
