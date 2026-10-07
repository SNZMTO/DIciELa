using Diciela.Data;
using UnityEditor;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les fiches de sol qui manquent (GDD, section 9 ; décisions 209 et 210) : la terre, gratuite, au pas normal
    /// d'une charrette, que la ville sait tracer dès le départ. À sa naissance seulement, elle entre au catalogue et au
    /// déroulé du départ : un réglage fait à la main ensuite (la terre retirée d'une liste) n'est jamais remis.
    /// </summary>
    internal static class FloorContentBuilder
    {
        internal const string DirtPath = "Assets/Data/Sols/Terre.asset";

        public static void CreateMissing(Catalog catalog, OpeningDefinition opening)
        {
            bool born = !EditorAssets.Exists(DirtPath);
            FloorDefinition dirt = EditorAssets.CreateIfMissing<FloorDefinition>(DirtPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "terre";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "sol.terre";
                EditorAssets.Property(fiche, "_cartSpeedFactor").floatValue = 1f;
            });
            if (!born)
                return;
            EditorAssets.Set(catalog, fiche => Append(EditorAssets.Property(fiche, "_floors"), dirt));
            EditorAssets.Set(opening, fiche => Append(EditorAssets.Property(fiche, "_knownFloors"), dirt));
        }

        private static void Append(SerializedProperty list, FloorDefinition floor)
        {
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = floor;
        }
    }
}
