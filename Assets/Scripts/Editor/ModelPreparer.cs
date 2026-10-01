using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Prépare les modèles des constructions (SAVOIR, 2.9) : le modèle du pack, tel quel, devient une copie qui se pose
    /// sans calcul. Sa porte dit de combien le tourner pour qu'elle regarde le nord ; la boîte de ses maillages, une fois
    /// tourné, donne l'empreinte et le centre ; l'origine du pack reste le sol (chez Synty, le niveau de la porte : les
    /// fondations s'enfoncent). Les pièces, dans leur version la plus détaillée, sont fusionnées (ModelMerger). La fiche
    /// reçoit le modèle préparé, son empreinte, sa porte, et sa couche si le modèle en a le repère (un objet « Couche » :
    /// les pieds de qui dort, son avant vers la tête). Une fiche sans modèle du pack reçoit sa boîte, à sa taille
    /// (PlaceholderBuilder) ; une fiche dont le modèle du pack est introuvable (le pack absent, ou ses GUID changés) est
    /// refusée, pour que sa boîte n'écrase pas le vrai modèle préparé. Tout se fait dans une scène d'aperçu, hors de la
    /// scène ouverte.
    /// </summary>
    internal static class ModelPreparer
    {
        // Le bas de la porte tombe au niveau de l'origine du pack, à quelques centimètres près ; sinon on le signale.
        private const float DoorGroundTolerance = 0.05f;

        [MenuItem("DICIELA/Contenu/Préparer les modèles")]
        public static void PrepareAll()
        {
            Catalog catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);
            foreach (StructureDefinition structure in catalog.AllStructures())
                Prepare(structure);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Prépare le modèle de cette construction depuis sa source et remplit sa fiche ; sans source, lui fait sa boîte.
        /// Faux s'il n'a pas pu (la raison est dans la console), et la fiche reste telle quelle.
        /// </summary>
        public static bool Prepare(StructureDefinition structure)
        {
            if (structure.SourceModel == null)
            {
                return SourceIsLost(structure)
                    ? Refuse(structure, "son modèle du pack est introuvable (le pack est-il importé ?).")
                    : PrepareBox(structure);
            }

            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                // Le pivot porte la source à son origine : on le tourne, puis on le décale pour centrer la boîte.
                var pivot = new GameObject("Pivot");
                SceneManager.MoveGameObjectToScene(pivot, scene);
                Object.Instantiate(structure.SourceModel, pivot.transform, false);

                List<MeshFilter> pieces = SourceModelParts.NearPieces(pivot, out int distant);
                List<MeshFilter> door = SourceModelParts.DoorPieces(pieces, pivot.transform);
                if (pieces.Count == 0)
                    return Refuse(structure, "son modèle n'a aucun maillage.");
                if (structure.HasDoor && door.Count == 0)
                    return Refuse(structure, "aucune pièce de son modèle ne porte « Door » dans son nom : pas de porte.");

                float yaw = structure.HasDoor
                    ? SourceModelParts.YawTowardsNorth(SourceModelParts.Box(pieces), SourceModelParts.Box(door))
                    : 0f;
                pivot.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                Bounds turned = SourceModelParts.Box(pieces);
                pivot.transform.position = new Vector3(-turned.center.x, 0f, -turned.center.z);
                Bounds box = SourceModelParts.Box(pieces);
                Bounds doorBox = structure.HasDoor ? SourceModelParts.Box(door) : default;
                Transform bed = SourceModelParts.Bed(pivot);

                GameObject prepared = ModelMerger.Save(structure.name, pieces, out string merge);
                if (prepared == null)
                    return false;
                Fill(structure, prepared, box, doorBox, bed);
                Debug.Log("DICIELA : " + structure.Id + " préparée depuis " + structure.SourceModel.name + " : " + merge +
                          " (la version la plus détaillée de chaque pièce ; " + distant + " maillages de loin laissés) ; " +
                          "tournée de " + yaw.ToString("0") + "°, empreinte " + Meters(box.size.x) + " × " +
                          Meters(box.size.z) + " m" + (structure.HasDoor ? DoorReport(box, doorBox, door.Count) : "") +
                          (bed != null ? BedReport(bed) : "") + ".");
                return true;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        /// <summary>
        /// Vrai si la fiche désigne un modèle du pack qui ne se charge pas : la référence garde son identifiant, sans objet
        /// derrière, et l'opérateur == d'Unity la dit nulle comme une fiche sans modèle.
        /// </summary>
        private static bool SourceIsLost(StructureDefinition structure)
        {
            SerializedProperty source = EditorAssets.Property(new SerializedObject(structure), "_sourceModel");
            return source.objectReferenceValue == null && source.objectReferenceEntityIdValue.IsValid();
        }

        /// <summary>
        /// Sans modèle du pack, la construction se voit en boîte à la taille de sa fiche (GDD, section 16) : la fiche fait
        /// foi, et la boîte la suit.
        /// </summary>
        private static bool PrepareBox(StructureDefinition structure)
        {
            GameObject box = PlaceholderBuilder.Build(structure, out string merge);
            if (box == null)
                return false;
            var fiche = new SerializedObject(structure);
            EditorAssets.Property(fiche, "_model").objectReferenceValue = box;
            EditorAssets.Property(fiche, "_hasBed").boolValue = false;
            fiche.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(structure);
            Debug.Log("DICIELA : " + structure.Id + " se voit en boîte de " + Meters(structure.WidthMeters) + " × " +
                      Meters(structure.DepthMeters) + " m, à la taille de sa fiche, en attendant son modèle : " + merge +
                      ".");
            return true;
        }

        /// <summary>
        /// La fiche reçoit le modèle préparé, l'empreinte (au centimètre supérieur), la porte, et la couche (au centimètre et
        /// au degré) ; sans repère, pas de couche.
        /// </summary>
        private static void Fill(StructureDefinition structure, GameObject prepared, Bounds box, Bounds door,
            Transform bed)
        {
            var fiche = new SerializedObject(structure);
            EditorAssets.Property(fiche, "_model").objectReferenceValue = prepared;
            EditorAssets.Property(fiche, "_widthMeters").floatValue = Mathf.Ceil(box.size.x * 100f) / 100f;
            EditorAssets.Property(fiche, "_depthMeters").floatValue = Mathf.Ceil(box.size.z * 100f) / 100f;
            if (structure.HasDoor)
            {
                EditorAssets.Property(fiche, "_doorOffsetMeters").floatValue = Mathf.Round(door.center.x * 100f) / 100f;
                EditorAssets.Property(fiche, "_doorWidthMeters").floatValue = Mathf.Round(door.size.x * 100f) / 100f;
            }
            EditorAssets.Property(fiche, "_hasBed").boolValue = bed != null;
            if (bed != null)
            {
                Vector3 feet = bed.position;
                EditorAssets.Property(fiche, "_bedFeet").vector3Value = new Vector3(Mathf.Round(feet.x * 100f) / 100f,
                    Mathf.Round(feet.y * 100f) / 100f, Mathf.Round(feet.z * 100f) / 100f);
                EditorAssets.Property(fiche, "_bedHeadingDegrees").floatValue = Mathf.Round(Heading(bed.forward));
            }
            fiche.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(structure);
        }

        private static string DoorReport(Bounds box, Bounds door, int pieces)
        {
            if (Mathf.Abs(door.min.y) > DoorGroundTolerance)
                Debug.LogWarning("DICIELA : une pièce de la porte descend à " + Meters(door.min.y) + " m de l'origine " +
                                 "du pack (un seuil de pierre, des fondations ?) : regarder la porte sur la planche des " +
                                 "modèles (DICIELA/Album).");
            return " ; porte (" + pieces + " pièces) décalée de " + Meters(door.center.x) + " m, large de " +
                   Meters(door.size.x) + " m, en retrait de " + Meters(box.max.z - door.max.z) + " m de la façade";
        }

        private static string BedReport(Transform bed)
        {
            return " ; couche aux pieds en (" + Meters(bed.position.x) + " ; " + Meters(bed.position.z) + ") m, à " +
                   Meters(bed.position.y) + " m du sol, la tête vers " + Heading(bed.forward).ToString("0") + "°";
        }

        // L'angle d'une direction sur le sol, depuis le nord vers l'est, entre 0 et 360°.
        private static float Heading(Vector3 direction)
        {
            return Mathf.Repeat(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 360f);
        }

        private static string Meters(float value)
        {
            return value.ToString("0.00");
        }

        private static bool Refuse(StructureDefinition structure, string reason)
        {
            Debug.LogError("DICIELA : " + structure.Id + " n'est pas préparée : " + reason);
            return false;
        }
    }
}
