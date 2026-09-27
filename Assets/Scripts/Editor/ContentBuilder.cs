using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée le contenu qui manque : la fiche de la cabane et son remplaçant, sa famille, le catalogue (GDD, sections 9
    /// et 16). Réexécutable : une fiche qui existe n'est jamais touchée.
    /// </summary>
    internal static class ContentBuilder
    {
        internal const string CatalogPath = "Assets/Data/Catalogue.asset";
        private const string StructuresFolder = "Assets/Data/Structures";
        private const string CabinPath = StructuresFolder + "/Cabane.asset";
        private const string HousingPath = StructuresFolder + "/Famille_Logement.asset";

        public static void CreateMissing()
        {
            StructureDefinition cabin = EditorAssets.CreateIfMissing<StructureDefinition>(CabinPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "cabane";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "construction.cabane";
            });
            if (cabin.Model == null)
                EditorAssets.AssignIfMissing(cabin, "_model", PlaceholderBuilder.CreateBoxIfMissing(cabin));

            StructureFamilyDefinition housing = EditorAssets.CreateIfMissing<StructureFamilyDefinition>(HousingPath,
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = "logement";
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "famille.logement";
                    SetList(EditorAssets.Property(fiche, "_structures"), cabin);
                });
            EditorAssets.CreateIfMissing<Catalog>(CatalogPath,
                fiche => SetList(EditorAssets.Property(fiche, "_families"), housing));
        }

        private static void SetList(SerializedProperty list, params Object[] items)
        {
            list.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
