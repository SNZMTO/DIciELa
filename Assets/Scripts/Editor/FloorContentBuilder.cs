using System;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les fiches de sol qui manquent (GDD, section 9 ; décisions 209, 210 et 222) : la terre, gratuite, au pas normal
    /// d'une charrette, que la ville sait tracer dès le départ. Elle est peinte d'une copie du calque de terre du pack,
    /// rangée hors du pack, qui se répète à l'échelle de l'herbe du pack : ses facettes ont la taille de celles de l'herbe.
    /// La copie naît une fois, à cette échelle ; la fiche la prend si elle n'a pas de calque ou garde encore celui du pack.
    /// À sa naissance seulement, la terre entre au catalogue et au déroulé du départ : un réglage fait à la main ensuite
    /// (la terre retirée d'une liste, un autre calque, une autre échelle) n'est jamais remis.
    /// </summary>
    internal static class FloorContentBuilder
    {
        internal const string DirtPath = "Assets/Data/Sols/Terre.asset";

        // La copie du calque de terre (décision 222), le calque du pack qu'elle copie, et l'herbe dont elle prend l'échelle.
        internal const string DirtLayerPath = "Assets/Art/Sols/Terre.terrainlayer";
        private const string PackDirtLayerPath =
            "Assets/Synty/PolygonFantasyVillage/Materials/Terrain/Village_Dirt_01.terrainlayer";
        private const string PackGrassLayerPath =
            "Assets/Synty/PolygonFantasyVillage/Materials/Terrain/Village_Grass_01.terrainlayer";

        public static void CreateMissing(Catalog catalog, OpeningDefinition opening)
        {
            bool born = !EditorAssets.Exists(DirtPath);
            FloorDefinition dirt = EditorAssets.CreateIfMissing<FloorDefinition>(DirtPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "terre";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "sol.terre";
                EditorAssets.Property(fiche, "_cartSpeedFactor").floatValue = 1f;
            });
            TerrainLayer packDirt = EditorAssets.Load<TerrainLayer>(PackDirtLayerPath);
            TerrainLayer copy = DirtLayer();
            EditorAssets.Set(dirt, fiche =>
            {
                SerializedProperty layer = EditorAssets.Property(fiche, "_layer");
                if (layer.objectReferenceValue == null || layer.objectReferenceValue == packDirt)
                    layer.objectReferenceValue = copy;
            });
            if (!born)
                return;
            EditorAssets.Set(catalog, fiche => Append(EditorAssets.Property(fiche, "_floors"), dirt));
            EditorAssets.Set(opening, fiche => Append(EditorAssets.Property(fiche, "_knownFloors"), dirt));
        }

        // La copie du calque de terre du pack ; née si elle manque, à l'échelle de l'herbe du pack.
        private static TerrainLayer DirtLayer()
        {
            if (EditorAssets.Exists(DirtLayerPath))
                return EditorAssets.Load<TerrainLayer>(DirtLayerPath);
            EditorAssets.EnsureFolder(DirtLayerPath.Substring(0, DirtLayerPath.LastIndexOf('/')));
            if (!AssetDatabase.CopyAsset(PackDirtLayerPath, DirtLayerPath))
                throw new InvalidOperationException("DICIELA : le calque de terre du pack ne se copie pas vers " +
                                                    DirtLayerPath + ".");
            TerrainLayer copy = EditorAssets.Load<TerrainLayer>(DirtLayerPath);
            copy.tileSize = EditorAssets.Load<TerrainLayer>(PackGrassLayerPath).tileSize;
            EditorUtility.SetDirty(copy);
            AssetDatabase.SaveAssetIfDirty(copy);
            Debug.Log("DICIELA : calque créé : " + DirtLayerPath + ", qui se répète tous les " + copy.tileSize.x + " m.");
            return copy;
        }

        private static void Append(SerializedProperty list, FloorDefinition floor)
        {
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = floor;
        }
    }
}
