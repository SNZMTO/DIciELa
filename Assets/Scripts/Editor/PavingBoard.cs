using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des pavés de la Place (GDD, section 11), en fichier, sans Play (Studio) : la Place ronde de 24 m et sa
    /// bordure de petits cailloux (PlaceBorder), sur l'herbe du pack, vêtue tour à tour de chacun des pavés du pack ; en
    /// haut, la Place entière de trois quarts ; en bas, son bord de près, une charrette du pack garée dessus pour l'échelle.
    /// De gauche à droite dans l'ordre de la liste, que la console redit. Rien n'est écrit dans le projet.
    /// </summary>
    internal static class PavingBoard
    {
        private const string SheetPath = "Temp/Planches/Paves.jpg";
        private const string Textures = "Assets/Synty/PolygonFantasyVillage/Textures/";
        private const string Generic = "Assets/Synty/PolygonGeneric/";
        private const string Template = "Assets/Synty/PolygonFantasyVillage/Materials/Buildings/Floor_Tiles_02.mat";
        private const string Cart = "Vehicles/SM_Veh_Cart_01";
        private const int Cell = 400;

        // La Place : son diamètre, celui de son dallage (la bordure le porte au diamètre), le dessus et la jupe du dallage.
        private const float Diameter = 24f;
        private const float PavingDiameter = 23.6f;
        private const float Top = 0.03f;
        private const float Skirt = 0.15f;

        // L'herbe autour : la texture du calque d'herbe du pack, tous les 5 m, comme ce calque.
        private const float GrassDiameter = 160f;
        private const float GrassPattern = 5f;

        // La charrette, garée sur le cercle des places de charrette (0,47 du demi-côté), au sud-est ; la vue de près, entre
        // elle et le bord.
        private const float CartRadius = 5.64f;
        private const float CartAngle = -60f;
        private const float NearRadius = 8.7f;
        private const float NearExtent = 4.2f;

        private static readonly Vector3 Whole = new Vector3(0.6f, 0.8f, -1f);
        private static readonly Vector3 Near = new Vector3(0.5f, 0.6f, -1f);

        private static readonly Paving[] Pavings =
        {
            new Paving("pavés bruns (Path_01)", Textures + "Terrain/Path_01.png", Textures + "Normals/Path_01_Normals.png",
                2.5f),
            new Paving("pavés moussus (Path_02, le chemin du pack)", Textures + "Terrain/Path_02.png",
                Textures + "Normals/Path_01_Normals.png", 2.5f),
            new Paving("pavés gris (Stone_01)", Textures + "Buildings/Stone_01.png",
                Textures + "Normals/Stone_01_Normals.png", 2.5f),
            new Paving("dalles carrées (FloorTiles_02)", Textures + "Buildings/FloorTiles_02.png",
                Textures + "Normals/Floor_Tiles_02_Normals.png", 3f),
            new Paving("carreaux octogonaux (FloorTiles_01)", Textures + "Buildings/FloorTiles_01.png",
                Textures + "Normals/Floor_Tiles_01_Normals.png", 2f),
            new Paving("briques (Generic_Brick)", Generic + "Textures/Generic_Brick.png", null, 3f,
                Generic + "Materials/Generic_Brick.mat")
        };

        [MenuItem("DICIELA/Album/Planche des pavés de la Place (fichier)")]
        private static void Write()
        {
            var made = new List<Object>();
            var whole = new List<Texture2D>();
            var near = new List<Texture2D>();
            var order = new StringBuilder();
            try
            {
                using (var studio = new Studio(Cell))
                {
                    var root = new GameObject("Planche des pavés");
                    var grass = new PackComposition.Floor(Template, Textures + "Terrain/Grass_01.png",
                        Textures + "Normals/Ground_01_Normals.png", GrassDiameter, 0f, 0f, GrassPattern);
                    if (Disc(root.transform, "Herbe", grass, made) == null)
                    {
                        Object.DestroyImmediate(root);
                        return;
                    }
                    var paving = new GameObject("Dallage", typeof(MeshFilter), typeof(MeshRenderer));
                    paving.transform.SetParent(root.transform, false);
                    foreach (PackComposition.Piece piece in PlaceBorder.For(Diameter))
                        Lay(piece, root.transform);
                    float angle = CartAngle * Mathf.Deg2Rad;
                    var outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    // Tangente au cercle : un quart de tour de la Place depuis son centre.
                    Lay(new PackComposition.Piece(Cart, outward.x * CartRadius, outward.z * CartRadius, -CartAngle, Top,
                        centered: true), root.transform);
                    studio.Add(root);

                    foreach (Paving candidate in Pavings)
                    {
                        if (!Dress(paving, candidate.Floor, made))
                            continue;
                        whole.Add(studio.Shoot(Vector3.zero, Whole, Diameter * 0.5f * 1.15f));
                        near.Add(studio.Shoot(outward * NearRadius, Near, NearExtent));
                        order.Append(whole.Count).Append(". ").Append(candidate.Name).Append(", un motif tous les ")
                            .Append(candidate.Floor.Pattern.ToString("0.#")).Append(" m ; ");
                    }
                    int columns = whole.Count;
                    whole.AddRange(near);
                    studio.Save(whole, Mathf.Max(1, columns), SheetPath);
                }
                Debug.Log("DICIELA : planche des pavés écrite : " + SheetPath + ". De gauche à droite : " + order);
            }
            finally
            {
                foreach (Object thing in made)
                    Object.DestroyImmediate(thing);
            }
        }

        // Un disque de ce sol sous ce parent : son maillage et sa matière, neufs, à détruire après la planche.
        private static GameObject Disc(Transform parent, string name, PackComposition.Floor floor, List<Object> made)
        {
            var disc = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            disc.transform.SetParent(parent, false);
            return Dress(disc, floor, made) ? disc : null;
        }

        // Vêt ce disque de ce sol : un maillage à son motif, sa matière.
        private static bool Dress(GameObject disc, PackComposition.Floor floor, List<Object> made)
        {
            Material material = floor.Dressed(disc.name);
            if (material == null)
            {
                Debug.LogError("DICIELA : la planche des pavés ne trouve pas dans le pack " + floor.TexturePath + ".");
                return false;
            }
            Mesh mesh = FloorMesh.Create(disc.name, floor.Diameter, floor.Top, floor.Skirt, floor.Pattern);
            made.Add(material);
            made.Add(mesh);
            disc.GetComponent<MeshFilter>().sharedMesh = mesh;
            disc.GetComponent<MeshRenderer>().sharedMaterial = material;
            return true;
        }

        private static void Lay(PackComposition.Piece piece, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(piece.Path);
            if (prefab == null)
            {
                Debug.LogError("DICIELA : la planche des pavés ne trouve pas " + piece.Path + ".");
                return;
            }
            piece.Pose(Object.Instantiate(prefab).transform, parent, PackMeasures.GeometryOf(prefab).Box);
        }

        /// <summary>Un pavé possible : son nom pour la console, et le dallage de la Place qu'il ferait.</summary>
        private readonly struct Paving
        {
            public readonly string Name;
            public readonly PackComposition.Floor Floor;

            public Paving(string name, string texture, string normals, float pattern, string template = Template)
            {
                Name = name;
                Floor = new PackComposition.Floor(template, texture, normals, PavingDiameter, Top, Skirt, pattern);
            }
        }
    }
}
