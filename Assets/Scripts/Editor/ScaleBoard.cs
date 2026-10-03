using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des proportions (fichier, sans Play ; Studio) : devant la porte de la cabane préparée, un animal du pack
    /// (le corps de la Chimère et des maîtres, un chat), le bâtisseur et la fermière (des humains du pack), tels que le
    /// pack les fait, vus de face puis de trois quarts. La console dit la hauteur de chacun et celle de la porte. Elle
    /// sert à juger l'échelle des personnages entre eux et contre les bâtiments ; l'animal y est à l'échelle que la sorte de
    /// la Chimère lui donne en jeu (décision 195).
    /// </summary>
    internal static class ScaleBoard
    {
        private const string SheetPath = "Temp/Planches/Proportions.jpg";
        private const string Characters = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters/";
        private const string CabinSource =
            "Assets/Synty/PolygonFantasyVillage/Prefabs/Buildings/Preset/Bld_Preset_Shed_02.prefab";
        private static readonly string[] Lineup = { "SM_Chr_Cat_Male_01", "SM_Chr_Male_02", "SM_Chr_Female_04" };

        [MenuItem("DICIELA/Album/Planche des proportions (fichier)")]
        private static void Write()
        {
            var cabin = EditorAssets.Load<StructureDefinition>(ContentBuilder.CabinPath);
            if (cabin == null || cabin.Model == null)
            {
                Debug.LogError("DICIELA : la planche des proportions demande la cabane préparée.");
                return;
            }
            var heights = new List<string> { "porte de la cabane " + DoorHeight().ToString("0.00") + " m" };
            var photos = new List<Texture2D>();
            using (var studio = new Studio(720))
            {
                studio.Add(Object.Instantiate(cabin.Model));
                float front = cabin.DepthMeters * 0.5f + 0.9f;
                for (int i = 0; i < Lineup.Length; i++)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Characters + Lineup[i] + ".prefab");
                    if (prefab == null)
                        continue;
                    float scale = i == 0 ? ChimeraScale() : 1f;
                    GameObject character = Object.Instantiate(prefab);
                    character.transform.position = new Vector3(cabin.DoorOffsetMeters - 1.3f + 1.3f * i, 0f, front);
                    character.transform.localScale = Vector3.one * scale;
                    studio.Add(character);
                    heights.Add(prefab.name + " " + (PackMeasures.GeometryOf(prefab).Box.size.y * scale).ToString("0.00") +
                                " m");
                }
                var target = new Vector3(cabin.DoorOffsetMeters, 1.2f, front - 0.6f);
                photos.Add(studio.Shoot(target, new Vector3(0f, 0.12f, 1f), 2.6f));
                photos.Add(studio.Shoot(target, new Vector3(0.8f, 0.35f, 1f), 3.2f));
                studio.Save(photos, 2, SheetPath);
            }
            Debug.Log("DICIELA : planche des proportions écrite : " + SheetPath + ". Hauteurs : " +
                      string.Join(" ; ", heights) + ".");
        }

        /// <summary>La console dit la taille de chaque battant de porte du pack (SM_Bld_Door_*) : largeur et hauteur.</summary>
        [MenuItem("DICIELA/Album/Mesurer les portes du pack")]
        private static void MeasureDoors()
        {
            var sizes = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("SM_Bld_Door_ t:Prefab",
                         new[] { "Assets/Synty/PolygonFantasyVillage/Prefabs/Buildings" }))
            {
                var door = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                Bounds box = PackMeasures.GeometryOf(door).Box;
                sizes.Add(door.name + " " + box.size.x.ToString("0.00") + " × " + box.size.y.ToString("0.00") +
                          " m (de " + box.min.y.ToString("0.00") + " à " + box.max.y.ToString("0.00") + ")");
            }
            Debug.Log("DICIELA : les battants de porte du pack, largeur × hauteur : " + string.Join(" ; ", sizes) + ".");
        }

        // L'échelle que la sorte de la Chimère donne à son modèle en jeu.
        private static float ChimeraScale()
        {
            var kind = EditorAssets.Load<VillagerKindDefinition>("Assets/Data/Habitants/Chimere.asset");
            return kind != null ? kind.ModelScale : 1f;
        }

        // La hauteur des pièces de la porte du modèle du pack de la cabane (celles dont le nom porte « Door »).
        private static float DoorHeight()
        {
            if (!EditorAssets.Exists(CabinSource))
                return 0f;
            GameObject root = PrefabUtility.LoadPrefabContents(CabinSource);
            try
            {
                List<MeshFilter> pieces = SourceModelParts.NearPieces(root, out int _);
                List<MeshFilter> door = SourceModelParts.DoorPieces(pieces, root.transform);
                return door.Count == 0 ? 0f : SourceModelParts.Box(door).size.y;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
