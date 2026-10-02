using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée le contenu qui manque : la ville au travail d'abord (TownContentBuilder), puis les fiches du campement et de la
    /// cabane et leurs modèles, leur famille, la sorte de la Chimère, le catalogue et ce que l'écran de création propose
    /// (AppearanceBuilder ; GDD, sections 6, 9 et 16). Une fiche naît complète : le campement avec son stock et son poste,
    /// la cabane avec son coût, le catalogue avec ses marchandises, ses statistiques et sa nature. Réexécutable : une fiche
    /// qui existe n'est jamais touchée, sauf pour une référence qui lui manque (un modèle, une icône) ; un réglage fait à
    /// la main n'est jamais remis (une liste vidée, un nombre à 0, le campement rangé ailleurs).
    /// </summary>
    internal static class ContentBuilder
    {
        internal const string CatalogPath = "Assets/Data/Catalogue.asset";
        private const string StructuresFolder = "Assets/Data/Structures";
        internal const string CampPath = StructuresFolder + "/Campement.asset";
        internal const string CabinPath = StructuresFolder + "/Cabane.asset";
        private const string HousingPath = StructuresFolder + "/Famille_Logement.asset";
        private const string ChimeraPath = "Assets/Data/Habitants/Chimere.asset";
        private const string OpeningPath = "Assets/Data/Depart.asset";

        // Le modèle du pack de la cabane (décision 75).
        private const string CabinSourcePath =
            "Assets/Synty/PolygonFantasyVillage/Prefabs/Buildings/Preset/Bld_Preset_Shed_02.prefab";

        public static void CreateMissing()
        {
            TownContentBuilder.Made town = TownContentBuilder.CreateMissing();
            // Le campement se voit en boîte à sa taille (Défaut : 3 × 3 m, porte de 1,2 m ; GDD, section 16) tant que sa
            // composition n'est pas faite (DICIELA/Contenu/Composer le campement). Il fait tout au départ (décision 172) :
            // il tient 100 marchandises pour la ville et ouvre le poste « à tout faire ».
            StructureDefinition camp = EditorAssets.CreateIfMissing<StructureDefinition>(CampPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "campement";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "construction.campement";
                EditorAssets.Property(fiche, "_widthMeters").floatValue = 3f;
                EditorAssets.Property(fiche, "_depthMeters").floatValue = 3f;
                EditorAssets.Property(fiche, "_doorWidthMeters").floatValue = 1.2f;
                EditorAssets.Property(fiche, "_unique").boolValue = true;
                EditorAssets.Property(fiche, "_chimeraHome").boolValue = true;
                EditorAssets.Property(fiche, "_builtWhenPlaced").boolValue = true;
                EditorAssets.Property(fiche, "_storageCapacity").intValue = 100;
                EditorAssets.SetList(EditorAssets.Property(fiche, "_workSlots"), new[] { town.OddJobs });
            });
            // Une cabane coûte 10 Bois et 5 Pierre, demande 2 h de travail à un débutant (décision 137) et offre deux lits
            // (GDD, section 4).
            StructureDefinition cabin = EditorAssets.CreateIfMissing<StructureDefinition>(CabinPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "cabane";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "construction.cabane";
                TownContentBuilder.SetAmounts(EditorAssets.Property(fiche, "_cost"), town.Wood, 10, town.Stone, 5);
                EditorAssets.Property(fiche, "_workHours").floatValue = 2f;
                EditorAssets.Property(fiche, "_beds").intValue = 2;
            });
            // Les modèles se font ici une fois ; ensuite, par le menu DICIELA/Contenu/Préparer les modèles.
            EditorAssets.AssignIfMissing(cabin, "_sourceModel", EditorAssets.Load<GameObject>(CabinSourcePath));
            if (camp.Model == null)
                ModelPreparer.Prepare(camp);
            if (cabin.Model == null)
                ModelPreparer.Prepare(cabin);

            StructureFamilyDefinition housing = EditorAssets.CreateIfMissing<StructureFamilyDefinition>(HousingPath,
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = "logement";
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "famille.logement";
                    SetList(EditorAssets.Property(fiche, "_structures"), camp, cabin);
                });
            // L'icône de son onglet ; son pastel, le rose, est celui par défaut de la fiche (décision 142).
            EditorAssets.AssignIfMissing(housing, "_icon",
                EditorAssets.Load<UnityEngine.UIElements.VectorImage>(InterfaceIconBuilder.Folder + "/maison.asset"));
            VillagerKindDefinition chimera = EditorAssets.CreateIfMissing<VillagerKindDefinition>(ChimeraPath,
                fiche => EditorAssets.Property(fiche, "_id").stringValue = "chimere");

            // Le déroulé du départ (décisions 172 et 185) : la Chimère sait déjà bâtir le campement et la cabane ; son
            // campement reçoit 30 Bois et 20 Pierre quand on l'installe.
            OpeningDefinition opening = EditorAssets.CreateIfMissing<OpeningDefinition>(OpeningPath, fiche =>
            {
                SetList(EditorAssets.Property(fiche, "_knownStructures"), camp, cabin);
                TownContentBuilder.SetAmounts(EditorAssets.Property(fiche, "_startingGoods"), town.Wood, 30, town.Stone,
                    20);
            });
            Catalog catalog = EditorAssets.CreateIfMissing<Catalog>(CatalogPath, fiche =>
            {
                SetList(EditorAssets.Property(fiche, "_families"), housing);
                EditorAssets.Property(fiche, "_opening").objectReferenceValue = opening;
                SetList(EditorAssets.Property(fiche, "_nature"), town.Nature);
                SetList(EditorAssets.Property(fiche, "_goods"), town.Wood, town.Stone);
                EditorAssets.SetList(EditorAssets.Property(fiche, "_statistics"), town.Statistics);
            });
            // Le campement vient en premier (GDD, section 6) : en tête du logement, s'il n'est encore dans aucune famille.
            if (!catalog.AllStructures().Contains(camp))
                InsertFirst(housing, "_structures", camp);
            EditorAssets.AssignIfMissing(catalog, "_chimeraKind", chimera);
            AppearanceBuilder.CreateMissing(catalog);
        }

        private static void SetList(SerializedProperty list, params Object[] items)
        {
            list.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        /// <summary>Met cet élément en tête de la liste d'une fiche ; le reste de la liste ne bouge pas.</summary>
        private static void InsertFirst(Object target, string listName, Object item)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty list = EditorAssets.Property(serialized, listName);
            list.InsertArrayElementAtIndex(0);
            list.GetArrayElementAtIndex(0).objectReferenceValue = item;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
