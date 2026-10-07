using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les fiches de sol qui manquent (GDD, section 9 ; décisions 209 et 210) : la terre, gratuite, au pas normal
    /// d'une charrette, que la ville sait tracer dès le départ, peinte du calque de terre du pack ; un calque qui lui
    /// manque se remet. À sa naissance seulement, elle entre au catalogue et au déroulé du départ : un réglage fait à la
    /// main ensuite (la terre retirée d'une liste) n'est jamais remis.
    /// </summary>
    internal static class FloorContentBuilder
    {
        internal const string DirtPath = "Assets/Data/Sols/Terre.asset";

        // Le calque de la terre du pack (décision 209).
        private const string DirtLayerPath =
            "Assets/Synty/PolygonFantasyVillage/Materials/Terrain/Village_Dirt_01.terrainlayer";

        public static void CreateMissing(Catalog catalog, OpeningDefinition opening)
        {
            bool born = !EditorAssets.Exists(DirtPath);
            FloorDefinition dirt = EditorAssets.CreateIfMissing<FloorDefinition>(DirtPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "terre";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "sol.terre";
                EditorAssets.Property(fiche, "_cartSpeedFactor").floatValue = 1f;
            });
            EditorAssets.AssignIfMissing(dirt, "_layer", EditorAssets.Load<TerrainLayer>(DirtLayerPath));
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
