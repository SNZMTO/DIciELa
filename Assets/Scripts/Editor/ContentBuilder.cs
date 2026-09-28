using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée le contenu qui manque : les fiches de la tente et de la cabane et leurs modèles, leur famille, la sorte de la
    /// Chimère, le catalogue (GDD, sections 6, 9 et 16). Réexécutable : une fiche qui existe n'est jamais touchée, sauf
    /// pour ce qui lui manque.
    /// </summary>
    internal static class ContentBuilder
    {
        internal const string CatalogPath = "Assets/Data/Catalogue.asset";
        private const string StructuresFolder = "Assets/Data/Structures";
        internal const string TentPath = StructuresFolder + "/Tente.asset";
        internal const string CabinPath = StructuresFolder + "/Cabane.asset";
        private const string HousingPath = StructuresFolder + "/Famille_Logement.asset";
        private const string ChimeraPath = "Assets/Data/Habitants/Chimere.asset";

        // Le modèle du pack de la cabane (décision 75).
        private const string CabinSourcePath =
            "Assets/Synty/PolygonFantasyVillage/Prefabs/Buildings/Preset/Bld_Preset_Shed_02.prefab";

        // Le personnage de la Chimère en attendant l'écran de création (décision 86) ; le choix est au développeur.
        private const string ChimeraModelPath =
            "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters/SM_Chr_Cat_Female_01.prefab";

        public static void CreateMissing()
        {
            // Le pack n'a pas de tente : elle se voit en boîte à sa taille tant que son modèle n'est pas choisi (Défaut :
            // 3 × 3 m, porte de 1,2 m ; GDD, section 16).
            StructureDefinition tent = EditorAssets.CreateIfMissing<StructureDefinition>(TentPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "tente";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "construction.tente";
                EditorAssets.Property(fiche, "_widthMeters").floatValue = 3f;
                EditorAssets.Property(fiche, "_depthMeters").floatValue = 3f;
                EditorAssets.Property(fiche, "_doorWidthMeters").floatValue = 1.2f;
                EditorAssets.Property(fiche, "_unique").boolValue = true;
                EditorAssets.Property(fiche, "_chimeraHome").boolValue = true;
                EditorAssets.Property(fiche, "_builtWhenPlaced").boolValue = true;
            });
            StructureDefinition cabin = EditorAssets.CreateIfMissing<StructureDefinition>(CabinPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "cabane";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "construction.cabane";
            });
            // Les modèles se font ici une fois ; ensuite, par le menu DICIELA/Contenu/Préparer les modèles.
            EditorAssets.AssignIfMissing(cabin, "_sourceModel", EditorAssets.Load<GameObject>(CabinSourcePath));
            if (tent.Model == null)
                ModelPreparer.Prepare(tent);
            if (cabin.Model == null)
                ModelPreparer.Prepare(cabin);

            StructureFamilyDefinition housing = EditorAssets.CreateIfMissing<StructureFamilyDefinition>(HousingPath,
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = "logement";
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "famille.logement";
                    SetList(EditorAssets.Property(fiche, "_structures"), tent, cabin);
                });
            // La tente vient en premier (GDD, section 6) : en tête de sa famille, si elle n'y est pas encore.
            InsertFirstIfMissing(housing, "_structures", tent);
            VillagerKindDefinition chimera = EditorAssets.CreateIfMissing<VillagerKindDefinition>(ChimeraPath,
                fiche => EditorAssets.Property(fiche, "_id").stringValue = "chimere");
            EditorAssets.AssignIfMissing(chimera, "_model", EditorAssets.Load<GameObject>(ChimeraModelPath));

            Catalog catalog = EditorAssets.CreateIfMissing<Catalog>(CatalogPath,
                fiche => SetList(EditorAssets.Property(fiche, "_families"), housing));
            EditorAssets.AssignIfMissing(catalog, "_chimeraKind", chimera);
        }

        private static void SetList(SerializedProperty list, params Object[] items)
        {
            list.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        /// <summary>Met cet élément en tête de la liste d'une fiche, s'il n'y est pas ; le reste de la liste ne bouge pas.</summary>
        private static void InsertFirstIfMissing(Object target, string listName, Object item)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty list = EditorAssets.Property(serialized, listName);
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == item)
                    return;
            }
            list.InsertArrayElementAtIndex(0);
            list.GetArrayElementAtIndex(0).objectReferenceValue = item;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
