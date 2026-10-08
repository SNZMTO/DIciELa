using System;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les fiches de sol qui manquent (GDD, section 9 ; décisions 209, 210, 222 et 227). La terre, gratuite, au
    /// pas normal d'une charrette, que la ville sait tracer dès le départ, son bord effiloché, sans liant : elle est
    /// peinte d'une copie du calque de terre du pack, rangée hors du pack, qui se répète à l'échelle de l'herbe du pack
    /// (ses facettes ont la taille de celles de l'herbe) ; la copie naît une fois, à cette échelle, et la fiche la
    /// prend si elle n'a pas de calque ou garde encore celui du pack. Les pavés, un sol dur, peints des pavés du pack
    /// (Path_01, ceux des rues de la démo), cernés d'une bande de terre, leur liant, comme dans la démo : ils entrent
    /// au catalogue, pas au déroulé du départ, car ils viennent avec leur idée. À sa naissance seulement, une fiche
    /// entre dans ses listes : un réglage fait à la main ensuite (un sol retiré d'une liste, un autre calque, une autre
    /// échelle) n'est jamais remis.
    /// </summary>
    internal static class FloorContentBuilder
    {
        internal const string DirtPath = "Assets/Data/Sols/Terre.asset";
        internal const string PavingPath = "Assets/Data/Sols/Paves.asset";

        // La copie du calque de terre (décision 222), le calque du pack qu'elle copie, et l'herbe dont elle prend l'échelle.
        internal const string DirtLayerPath = "Assets/Art/Sols/Terre.terrainlayer";
        private const string PackDirtLayerPath =
            "Assets/Synty/PolygonFantasyVillage/Materials/Terrain/Village_Dirt_01.terrainlayer";
        private const string PackGrassLayerPath =
            "Assets/Synty/PolygonFantasyVillage/Materials/Terrain/Village_Grass_01.terrainlayer";
        private const string PackPavingLayerPath =
            "Assets/Synty/PolygonFantasyVillage/Materials/Terrain/Path_01.terrainlayer";

        // Les pavés : un sol dur (GDD, section 9), la bande de terre de la démo autour d'eux, qui va et vient : leur
        // bord s'effiloche et se fond vite dans la terre, elle-même fondue plus large dans l'herbe (décision 232).
        private const float PavingCartSpeed = 1.5f;
        private const float PavingBorderMeters = 0.8f;
        private const float PavingFrayMeters = 0.4f;
        private const float PavingFadeMeters = 0.5f;
        private const float PavingBorderFadeMeters = 1.2f;

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
            if (born)
            {
                EditorAssets.Set(catalog, fiche => Append(EditorAssets.Property(fiche, "_floors"), dirt));
                EditorAssets.Set(opening, fiche => Append(EditorAssets.Property(fiche, "_knownFloors"), dirt));
            }
            CreatePaving(catalog, copy);
        }

        // Les pavés, nés une fois : leur calque du pack, la terre pour liant ; au catalogue, pas au départ.
        private static void CreatePaving(Catalog catalog, TerrainLayer dirtLayer)
        {
            if (EditorAssets.Exists(PavingPath))
                return;
            TerrainLayer packPaving = EditorAssets.Load<TerrainLayer>(PackPavingLayerPath);
            FloorDefinition paving = EditorAssets.CreateIfMissing<FloorDefinition>(PavingPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "paves";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "sol.paves";
                EditorAssets.Property(fiche, "_layer").objectReferenceValue = packPaving;
                EditorAssets.Property(fiche, "_borderLayer").objectReferenceValue = dirtLayer;
                EditorAssets.Property(fiche, "_borderMeters").floatValue = PavingBorderMeters;
                EditorAssets.Property(fiche, "_frayMeters").floatValue = PavingFrayMeters;
                EditorAssets.Property(fiche, "_fadeMeters").floatValue = PavingFadeMeters;
                EditorAssets.Property(fiche, "_borderFadeMeters").floatValue = PavingBorderFadeMeters;
                EditorAssets.Property(fiche, "_cartSpeedFactor").floatValue = PavingCartSpeed;
            });
            EditorAssets.Set(catalog, fiche => Append(EditorAssets.Property(fiche, "_floors"), paving));
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
