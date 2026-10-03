using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des pièces de la Place (GDD, section 11), en fichier, sans Play (Studio) : les pièces du pack qui
    /// peuvent la faire, chacune vue de trois quarts au-dessus, de gauche à droite et de haut en bas dans l'ordre de la
    /// liste ; la console dit la taille de chacune. Elle sert à choisir le dallage, les cailloux du bord et les charrettes
    /// des étals.
    /// </summary>
    internal static class PlaceBoard
    {
        private const string SheetPath = "Temp/Planches/Place.jpg";
        private const string Pack = "Assets/Synty/PolygonFantasyVillage/Prefabs/";
        private const int Cell = 300;
        private const int Columns = 7;
        private static readonly Vector3 Direction = new Vector3(0.6f, 0.9f, 1f);

        private static readonly string[] Pieces =
        {
            "Environment/SM_Env_Stone_Slab_01", "Environment/SM_Env_Stone_Slab_02", "Environment/SM_Env_Stone_Slab_03",
            "Environment/SM_Env_Stone_Slab_04", "Environment/SM_Env_Stone_Slab_05", "Buildings/SM_Bld_Shrine_Tile_01",
            "Buildings/SM_Bld_Floor_Round_S_01",
            "Environment/SM_Env_Stepping_Stone_01", "Environment/SM_Env_Stepping_Stone_02",
            "Environment/SM_Env_Stepping_Stone_03", "Environment/SM_Env_Stepping_Stone_04",
            "Environment/SM_Env_Stepping_Stone_05", "Environment/SM_Env_Rock_Ground_01", "Environment/SM_Env_Rock_Ground_02",
            "Environment/SM_Env_Rock_Pile_01", "Environment/SM_Env_Rock_Pile_02", "Environment/SM_Env_Rock_Pile_03",
            "Environment/SM_Env_Rock_Pile_04", "Environment/SM_Env_Rock_Pile_05", "Environment/SM_Env_Rock_01",
            "Environment/SM_Env_Rock_02",
            "Buildings/SM_Bld_Stone_Edge_01", "Buildings/SM_Bld_Stone_Edge_02", "Buildings/SM_Bld_Stone_Edge_Half_01",
            "Environment/SM_Env_Stone_Ring_01", "Buildings/SM_Bld_Floor_Round_L_01", "Buildings/SM_Bld_Shrine_Steps_01",
            "Environment/SM_Env_Rock_03",
            "Vehicles/SM_Veh_Cart_01", "Vehicles/SM_Veh_Cart_02", "Vehicles/SM_Veh_Cart_03", "Vehicles/SM_Veh_Cart_04"
        };

        [MenuItem("DICIELA/Album/Planche des pièces de la Place (fichier)")]
        private static void Write()
        {
            var photos = new List<Texture2D>();
            var sizes = new StringBuilder();
            using (var studio = new Studio(Cell))
            {
                foreach (string piece in Pieces)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Pack + piece + ".prefab");
                    if (prefab == null)
                    {
                        Debug.LogError("DICIELA : la planche de la Place ne trouve pas " + piece + ".");
                        continue;
                    }
                    Bounds box = PackMeasures.GeometryOf(prefab).Box;
                    var subject = Object.Instantiate(prefab);
                    studio.Add(subject);
                    photos.Add(studio.Shoot(box.center, Direction, Mathf.Max(0.3f, box.extents.magnitude)));
                    Object.DestroyImmediate(subject);
                    sizes.Append(photos.Count).Append(". ").Append(prefab.name).Append(' ')
                        .Append(box.size.x.ToString("0.00")).Append(" × ").Append(box.size.z.ToString("0.00"))
                        .Append(" × ").Append(box.size.y.ToString("0.00")).Append(" (bas à ")
                        .Append(box.min.y.ToString("0.00")).Append(", milieu en ").Append(box.center.x.ToString("0.00"))
                        .Append(" ; ").Append(box.center.z.ToString("0.00")).Append(") ; ");
                }
                studio.Save(photos, Columns, SheetPath);
            }
            Debug.Log("DICIELA : planche de la Place écrite : " + SheetPath + ". Largeur × profondeur × hauteur en mètres : " +
                      sizes);
        }

        /// <summary>
        /// La Place préparée (son modèle, tel que le jeu le pose), en fichier : de trois quarts, d'en haut, puis deux bouts de
        /// son bord rond de près, au sud-est et au sud.
        /// </summary>
        [MenuItem("DICIELA/Album/Planche de la Place préparée (fichier)")]
        private static void WritePrepared()
        {
            var place = EditorAssets.Load<Data.StructureDefinition>(ContentBuilder.PlacePath);
            if (place == null || place.Model == null)
            {
                Debug.LogError("DICIELA : la Place n'a pas de modèle préparé.");
                return;
            }
            var photos = new List<Texture2D>();
            using (var studio = new Studio(720))
            {
                GameObject subject = Object.Instantiate(place.Model);
                studio.Add(subject);
                float half = place.WidthMeters * 0.5f;
                photos.Add(studio.Shoot(Vector3.zero, new Vector3(0.6f, 0.8f, -1f), half * 1.25f));
                photos.Add(studio.Shoot(Vector3.zero, new Vector3(0f, 1f, -0.01f), half * 1.1f));
                float diagonal = (half - 1f) * Mathf.Sqrt(0.5f);
                photos.Add(studio.Shoot(new Vector3(diagonal, 0f, -diagonal), new Vector3(0.5f, 0.7f, -1f), 1.6f));
                photos.Add(studio.Shoot(new Vector3(0f, 0f, -half + 0.5f), new Vector3(0.3f, 0.6f, -1f), 1.6f));
                studio.Save(photos, 2, "Temp/Planches/Place_preparee.jpg");
            }
            Debug.Log("DICIELA : planche de la Place préparée écrite : Temp/Planches/Place_preparee.jpg.");
        }
    }
}
