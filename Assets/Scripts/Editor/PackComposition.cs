using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Un modèle fait d'objets du pack tels quels (SAVOIR, 2.9 ; décisions 170, 172 et 193) : son nom, la fiche de la
    /// construction dont il devient le modèle source, ses objets, chacun à sa place, s'il en a un son dallage (un sol
    /// vêtu d'une matière du pack, FloorMesh), et, s'il en a une, sa couche (un repère « Couche » : les pieds de qui dort,
    /// son avant vers la tête). Ceux du jeu : le campement, qui fait tout au départ (un foyer, une couche où dort la
    /// Chimère, un coffre, un sac, une marmite, et son stock : une pile de bûches et un tas de pierres, au fond, près du
    /// coffre ; deux pierres plates sous un objet « Door » font son entrée) ; la Place, un disque de galets de rivière
    /// entouré de petits cailloux (PlaceBorder). Les rochers du pack sont à l'échelle du paysage : le foyer, le tas de pierres et
    /// les cailloux en sont des réductions.
    /// </summary>
    internal sealed class PackComposition
    {
        private const string Pack = "Assets/Synty/PolygonFantasyVillage/Prefabs/";
        private const string PackMaterials = "Assets/Synty/PolygonFantasyVillage/Materials/";
        private const string PackTextures = "Assets/Synty/PolygonFantasyVillage/Textures/";

        private PackComposition(string name, string structurePath, Spot? couch, Floor floor, params Piece[] pieces)
        {
            Name = name;
            StructurePath = structurePath;
            Couch = couch;
            Paving = floor;
            Pieces = pieces;
        }

        public string Name { get; }
        public string StructurePath { get; }
        public Spot? Couch { get; }
        public Floor Paving { get; }
        public Piece[] Pieces { get; }

        /// <summary>
        /// Un dallage rond (FloorMesh) : sa matière, une copie de celle-ci du pack (son shader et ses réglages) portant
        /// cette texture et ces reliefs du pack (aucun : une surface lisse) ; son diamètre, la hauteur de son dessus, la
        /// profondeur de sa jupe sous le sol et la taille d'un motif de sa texture, en mètres.
        /// </summary>
        public sealed class Floor
        {
            private const string AlbedoMap = "_Albedo_Map";
            private const string NormalMap = "_Normal_Map";

            public Floor(string templatePath, string texturePath, string normalsPath, float diameter, float top,
                float skirt, float pattern)
            {
                TemplatePath = templatePath;
                TexturePath = texturePath;
                NormalsPath = normalsPath;
                Diameter = diameter;
                Top = top;
                Skirt = skirt;
                Pattern = pattern;
            }

            public string TemplatePath { get; }
            public string TexturePath { get; }
            public string NormalsPath { get; }
            public float Diameter { get; }
            public float Top { get; }
            public float Skirt { get; }
            public float Pattern { get; }

            /// <summary>Sa matière, neuve, hors de tout fichier ; null s'il manque au pack la matière ou une texture.</summary>
            public Material Dressed(string name)
            {
                var template = AssetDatabase.LoadAssetAtPath<Material>(TemplatePath);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
                Texture2D normals = NormalsPath != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(NormalsPath) : null;
                if (template == null || texture == null || (NormalsPath != null && normals == null))
                    return null;
                var material = new Material(template) { name = name };
                material.SetTexture(AlbedoMap, texture);
                material.SetTexture(NormalMap, normals);
                return material;
            }
        }

        /// <summary>Un repère : sa place (en mètres, l'entrée vers +z) et le quart de tour de son avant.</summary>
        public readonly struct Spot
        {
            public readonly Vector3 Place;
            public readonly float Yaw;

            public Spot(float x, float y, float z, float yaw)
            {
                Place = new Vector3(x, y, z);
                Yaw = yaw;
            }
        }

        /// <summary>
        /// Un objet de la composition : son prefab du pack, sa place (en mètres, l'entrée vers +z), son quart de tour, son
        /// inclinaison vers l'avant, son échelle, s'il fait l'entrée, et si sa place est celle du milieu de sa boîte
        /// plutôt que celle de son origine (une rangée de pierres du pack a son origine à un bout).
        /// </summary>
        public readonly struct Piece
        {
            public readonly string Path;
            public readonly Vector3 Place;
            public readonly float Yaw;
            public readonly float Pitch;
            public readonly float Scale;
            public readonly bool AtTheDoor;
            public readonly bool Centered;

            public Piece(string path, float x, float z, float yaw = 0f, float y = 0f, float scale = 1f,
                float pitch = 0f, bool atTheDoor = false, bool centered = false)
            {
                Path = Pack + path + ".prefab";
                Place = new Vector3(x, y, z);
                Yaw = yaw;
                Pitch = pitch;
                Scale = scale;
                AtTheDoor = atTheDoor;
                Centered = centered;
            }

            /// <summary>
            /// Pose une instance de son prefab à sa place, sous ce parent ; box, la boîte de ses maillages mesurée sur le
            /// prefab (PackMeasures), sert à la poser par son milieu.
            /// </summary>
            public void Pose(Transform instance, Transform parent, Bounds box)
            {
                instance.SetParent(parent, false);
                instance.localPosition = Place;
                instance.localRotation = Quaternion.Euler(Pitch, Yaw, 0f);
                instance.localScale = Vector3.one * Scale;
                if (!Centered)
                    return;
                // Posé par le milieu de sa boîte : son origine recule d'autant, tournée avec lui, au sol.
                Vector3 middle = instance.localRotation * (box.center * Scale);
                instance.localPosition -= new Vector3(middle.x, 0f, middle.z);
            }
        }

        public static readonly PackComposition[] All =
        {
            // Le cercle de pierres du pack (6,2 m, un cercle de place) réduit au quart fait le foyer. Pas de flamme : un
            // effet de particules dans ces modèles fait planter Unity après une campagne PlayMode (SAVOIR, 2.9). Sur le
            // tapis (2,59 m de long), la Chimère dort les pieds vers l'entrée, la tête sur le coussin, au sud. Le stock,
            // une pile de bûches et un tas de pierres, est au fond, du côté du coffre.
            new PackComposition("Campement", ContentBuilder.CampPath, new Spot(-1.75f, 0.2f, 0.65f, 180f), null,
                new Piece("Environment/SM_Env_Stone_Ring_01", 0f, 0.1f, scale: 0.25f),
                new Piece("Props/SM_Prop_Picnic_Rug_01", -1.75f, -0.1f),
                new Piece("Props/SM_Prop_Pillow_01", -1.75f, -0.95f, y: 0.05f),
                new Piece("Props/SM_Prop_Cooking_Pot_01", 0.95f, -0.75f, 30f),
                new Piece("Props/SM_Prop_Chest_01", 1.75f, 0.55f, -90f),
                new Piece("Props/SM_Prop_Sack_01", 1.45f, -0.35f),
                new Piece("Environment/SM_Env_Stepping_Stone_01", -0.35f, 1.75f, atTheDoor: true),
                new Piece("Environment/SM_Env_Stepping_Stone_02", 0.35f, 1.8f, 15f, atTheDoor: true),
                new Piece("Props/SM_Prop_Wood_Pile_02", 0.9f, -1.75f),
                new Piece("Environment/SM_Env_Rock_Pile_05", 2.25f, -1.95f, 25f, scale: 0.22f)),
            // La Place ronde (décisions 193, 196 et 203) : un disque de 23,6 m de galets de rivière du pack sur la terre (sa
            // texture tous les 3 m), le dessus à 3 cm du sol, la jupe à 15 cm dessous ; ses petits cailloux à cheval sur
            // son bord, qui portent son diamètre à 24 m.
            new PackComposition("Place", ContentBuilder.PlacePath, null,
                new Floor(PackMaterials + "Buildings/Floor_Tiles_02.mat", PackTextures + "Terrain/River_Rocks_02.png",
                    PackTextures + "Normals/River_Rocks_02_Normals.png", 23.6f, 0.03f, 0.15f, 3f),
                PlaceBorder.For(24f))
        };
    }
}
