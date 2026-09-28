using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée le contenu qui manque : la fiche de la cabane et son modèle préparé, sa famille, la sorte de la Chimère, le
    /// catalogue (GDD, sections 6, 9 et 16). Réexécutable : une fiche qui existe n'est jamais touchée, sauf pour ce qui
    /// lui manque.
    /// </summary>
    internal static class ContentBuilder
    {
        internal const string CatalogPath = "Assets/Data/Catalogue.asset";
        private const string StructuresFolder = "Assets/Data/Structures";
        private const string CabinPath = StructuresFolder + "/Cabane.asset";
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
            StructureDefinition cabin = EditorAssets.CreateIfMissing<StructureDefinition>(CabinPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "cabane";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "construction.cabane";
            });
            // Le modèle préparé se fait ici une fois ; ensuite, par le menu DICIELA/Contenu/Préparer les modèles.
            EditorAssets.AssignIfMissing(cabin, "_sourceModel", EditorAssets.Load<GameObject>(CabinSourcePath));
            if (cabin.Model == null)
                ModelPreparer.Prepare(cabin);

            StructureFamilyDefinition housing = EditorAssets.CreateIfMissing<StructureFamilyDefinition>(HousingPath,
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = "logement";
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "famille.logement";
                    SetList(EditorAssets.Property(fiche, "_structures"), cabin);
                });
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
    }
}
