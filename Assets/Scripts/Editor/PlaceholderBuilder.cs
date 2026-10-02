using System.Collections.Generic;
using Diciela.Data;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Le remplaçant d'une construction dont le modèle n'est pas encore choisi (GDD, section 16 ; l'art est au
    /// développeur) : une boîte à la taille de son empreinte, et sa porte marquée sur la façade nord, en retrait dans
    /// l'empreinte, pour lire sa taille et son sens ; un sol qu'on traverse (la Place), une dalle plate à sa taille. Le
    /// remplaçant est fusionné comme un modèle préparé (ModelMerger) : un seul maillage, centré sur l'empreinte, son
    /// origine au sol ; il se refait à la taille de sa fiche à chaque préparation. Tout se fait dans une scène d'aperçu,
    /// hors de la scène ouverte.
    /// </summary>
    internal static class PlaceholderBuilder
    {
        private const string Folder = "Assets/Art/Remplacants";
        private const string BodyMaterialPath = Folder + "/Remplacant_Corps.mat";
        private const string DoorMaterialPath = Folder + "/Remplacant_Porte.mat";

        // La hauteur de la boîte et de sa porte, en mètres : de quoi lire la taille et le sens, sans plus.
        private const float BoxHeight = 2.5f;
        private const float DoorHeight = 1.8f;
        private const float DoorThickness = 0.1f;

        // La dalle d'un sol : son dessus à peine au-dessus du sol, ses fondations dessous, qui cachent le vide sur une
        // pente douce ; en mètres.
        private const float FloorHeight = 0.05f;
        private const float FloorFoundation = 0.3f;

        private static readonly Color BodyColor = new Color(0.86f, 0.8f, 0.68f);
        private static readonly Color DoorColor = new Color(0.36f, 0.24f, 0.14f);

        /// <summary>
        /// Fait le remplaçant de cette construction à la taille de sa fiche, et le lui donne pour modèle ; null si la
        /// fusion a échoué (la raison est dans la console).
        /// </summary>
        public static GameObject Build(StructureDefinition definition, out string report)
        {
            Material body = MaterialBuilder.OpaqueIfMissing(BodyMaterialPath, BodyColor);
            Material door = MaterialBuilder.OpaqueIfMissing(DoorMaterialPath, DoorColor);
            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject(definition.name + "_Remplacant");
                SceneManager.MoveGameObjectToScene(root, scene);
                List<MeshFilter> pieces = definition.IsWalkable
                    ? new List<MeshFilter> { Floor(root, definition, body) }
                    : Box(root, definition, body, door);
                return ModelMerger.Save(definition.name, pieces, out report);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        // La boîte et sa porte : le corps laisse devant lui l'épaisseur de la porte, qui affleure la façade ; ensemble,
        // ils remplissent l'empreinte de la fiche, sans rien qui dépasse.
        private static List<MeshFilter> Box(GameObject root, StructureDefinition definition, Material body,
            Material door)
        {
            float bodyDepth = definition.DepthMeters - (definition.HasDoor ? DoorThickness : 0f);
            var pieces = new List<MeshFilter>
            {
                AddBox(root, new Vector3(0f, BoxHeight * 0.5f, (bodyDepth - definition.DepthMeters) * 0.5f),
                    new Vector3(definition.WidthMeters, BoxHeight, bodyDepth), body)
            };
            if (definition.HasDoor)
            {
                var at = new Vector3(definition.DoorOffsetMeters, DoorHeight * 0.5f,
                    (definition.DepthMeters - DoorThickness) * 0.5f);
                pieces.Add(AddBox(root, at, new Vector3(definition.DoorWidthMeters, DoorHeight, DoorThickness), door));
            }
            return pieces;
        }

        // La dalle d'un sol qu'on traverse, à la taille de l'empreinte.
        private static MeshFilter Floor(GameObject root, StructureDefinition definition, Material body)
        {
            float thickness = FloorHeight + FloorFoundation;
            return AddBox(root, new Vector3(0f, FloorHeight - thickness * 0.5f, 0f),
                new Vector3(definition.WidthMeters, thickness, definition.DepthMeters), body);
        }

        private static MeshFilter AddBox(GameObject parent, Vector3 position, Vector3 size, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.SetParent(parent.transform, false);
            box.transform.localPosition = position;
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(box.GetComponent<Collider>());
            return box.GetComponent<MeshFilter>();
        }
    }
}
