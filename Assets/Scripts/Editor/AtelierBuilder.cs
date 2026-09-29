using Diciela.View;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Monte l'atelier de l'écran de création (GDD, section 6 ; Atelier) : ses maillages à facettes (AtelierShapes),
    /// réécrits à chaque passage ; ses matériaux, créés s'ils manquent (leurs couleurs se règlent dans l'Inspector) ; son
    /// prefab (le décor, une lumière chaude, une caméra qui cadre la Chimère à gauche du panneau), créé s'il manque. Dans
    /// une scène racine, il le pose loin de toute carte s'il n'y est pas. Réexécutable.
    /// </summary>
    internal static class AtelierBuilder
    {
        private const string Folder = "Assets/Art/Atelier";
        private const string PrefabPath = Folder + "/Atelier.prefab";
        // Loin de la carte et au-dessus d'elle : rien de la carte n'entre dans le champ de sa caméra.
        private static readonly Vector3 Place = new Vector3(-2000f, 500f, -2000f);
        // La caméra regarde la Chimère de face, un peu de haut, décalée pour la montrer à gauche du panneau de droite.
        private static readonly Vector3 CameraPlace = new Vector3(-1.15f, 2f, 6.8f);
        private const float CameraPitchDegrees = 4f;
        private const float CameraFieldOfView = 30f;
        private const float CameraFarMeters = 40f;
        private static readonly Vector3 LightPlace = new Vector3(-1.5f, 4f, 3.5f);
        private static readonly Color LightColor = new Color(1f, 0.88f, 0.72f);
        private const float LightIntensity = 2.5f;
        private const float LightRange = 12f;
        private const float Smoothness = 0.1f;

        [MenuItem("DICIELA/Interface/Générer l'atelier")]
        public static void Generate()
        {
            EnsureAssets();
            Debug.Log("DICIELA : atelier généré dans " + Folder + ".");
        }

        /// <summary>Les maillages réécrits, les matériaux et le prefab créés s'ils manquent.</summary>
        public static void EnsureAssets()
        {
            Mesh floor = SaveMesh(AtelierShapes.Floor(), "Sol");
            Mesh pedestal = SaveMesh(AtelierShapes.Pedestal(), "Socle");
            Mesh backdrop = SaveMesh(AtelierShapes.Backdrop(), "Fond");
            if (EditorAssets.Exists(PrefabPath))
                return;

            var root = new GameObject("Atelier");
            Piece(root, "Sol", floor, false, Paint("Sol_A", 0.62f, 0.47f, 0.32f), Paint("Sol_B", 0.57f, 0.43f, 0.29f));
            Piece(root, "Socle", pedestal, true, Paint("Socle_Pans", 0.36f, 0.23f, 0.14f),
                Paint("Socle_Dessus", 0.72f, 0.55f, 0.36f));
            Piece(root, "Fond", backdrop, false, Paint("Fond_Plinthe", 0.3f, 0.19f, 0.11f),
                Paint("Fond_A", 0.86f, 0.74f, 0.56f), Paint("Fond_B", 0.8f, 0.67f, 0.49f));
            var spot = new GameObject("Place de la Chimère").transform;
            spot.SetParent(root.transform, false);
            spot.localPosition = Vector3.up * AtelierShapes.PedestalHeight;
            AddLight(root);
            AddCamera(root);
            Atelier atelier = root.AddComponent<Atelier>();
            EditorAssets.AssignIfMissing(atelier, "_spot", spot);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("DICIELA : prefab de l'atelier créé : " + PrefabPath + ".");
        }

        /// <summary>L'atelier de cette scène, posé loin de la carte s'il n'y est pas ; null (avec une erreur) sans prefab.</summary>
        public static Atelier EnsureIn(Scene scene)
        {
            Atelier existing = EditorAssets.FindInScene<Atelier>(scene);
            if (existing != null)
                return existing;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("DICIELA : le prefab de l'atelier manque : « DICIELA/Interface/Générer l'atelier ».");
                return null;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.position = Place;
            return instance.GetComponent<Atelier>();
        }

        private static Mesh SaveMesh(FacetMesh shape, string name)
        {
            string path = Folder + "/" + name + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                EditorAssets.EnsureFolder(Folder);
                mesh = new Mesh { name = name };
                shape.WriteTo(mesh);
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            shape.WriteTo(mesh);
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssetIfDirty(mesh);
            return mesh;
        }

        // Un matériau mat de cette couleur, créé s'il manque ; celui qui existe garde ses réglages.
        private static Material Paint(string name, float r, float g, float b)
        {
            string path = Folder + "/" + name + ".mat";
            bool existed = EditorAssets.Exists(path);
            Material material = MaterialBuilder.OpaqueIfMissing(path, new Color(r, g, b));
            if (!existed)
            {
                material.SetFloat("_Smoothness", Smoothness);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
            }
            return material;
        }

        // Le sol et le fond ne portent pas d'ombre : le soleil de la carte, d'où qu'il vienne, n'y met pas la Chimère
        // dans le noir ; le socle en porte une sur le sol.
        private static void Piece(GameObject root, string name, Mesh mesh, bool castsShadows, params Material[] materials)
        {
            var piece = new GameObject(name);
            piece.transform.SetParent(root.transform, false);
            piece.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = piece.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = castsShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        private static void AddLight(GameObject root)
        {
            var holder = new GameObject("Lumière de l'atelier");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localPosition = LightPlace;
            var light = holder.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = LightColor;
            light.intensity = LightIntensity;
            light.range = LightRange;
            light.shadows = LightShadows.None;
        }

        // Sans écouteur de son : celui de la caméra de la ville reste le seul.
        private static void AddCamera(GameObject root)
        {
            var holder = new GameObject("Caméra de l'atelier");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localPosition = CameraPlace;
            holder.transform.localRotation = Quaternion.Euler(CameraPitchDegrees, 180f, 0f);
            var camera = holder.AddComponent<Camera>();
            camera.fieldOfView = CameraFieldOfView;
            camera.farClipPlane = CameraFarMeters;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        }
    }
}
