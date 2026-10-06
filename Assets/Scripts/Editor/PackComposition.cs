using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Un modèle fait d'objets du pack tels quels (SAVOIR, 2.9 ; décisions 170 et 172) : son nom, la fiche de la
    /// construction dont il devient le modèle source, ses objets, chacun à sa place, et, s'il en a une, sa couche (un
    /// repère « Couche » : les pieds de qui dort, son avant vers la tête). Celui du jeu : le campement, qui fait tout au
    /// départ (un foyer, une couche où dort la Chimère, un coffre, un sac, une marmite, et son stock : une pile de bûches
    /// et un tas de pierres, au fond, près du coffre ; deux pierres plates sous un objet « Door » font son entrée). Les
    /// rochers du pack sont à l'échelle du paysage : le foyer et le tas de pierres en sont des réductions.
    /// </summary>
    internal sealed class PackComposition
    {
        private const string Pack = "Assets/Synty/PolygonFantasyVillage/Prefabs/";

        private PackComposition(string name, string structurePath, Spot? couch, params Piece[] pieces)
        {
            Name = name;
            StructurePath = structurePath;
            Couch = couch;
            Pieces = pieces;
        }

        public string Name { get; }
        public string StructurePath { get; }
        public Spot? Couch { get; }
        public Piece[] Pieces { get; }

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
            new PackComposition("Campement", ContentBuilder.CampPath, new Spot(-1.75f, 0.2f, 0.65f, 180f),
                new Piece("Environment/SM_Env_Stone_Ring_01", 0f, 0.1f, scale: 0.25f),
                new Piece("Props/SM_Prop_Picnic_Rug_01", -1.75f, -0.1f),
                new Piece("Props/SM_Prop_Pillow_01", -1.75f, -0.95f, y: 0.05f),
                new Piece("Props/SM_Prop_Cooking_Pot_01", 0.95f, -0.75f, 30f),
                new Piece("Props/SM_Prop_Chest_01", 1.75f, 0.55f, -90f),
                new Piece("Props/SM_Prop_Sack_01", 1.45f, -0.35f),
                new Piece("Environment/SM_Env_Stepping_Stone_01", -0.35f, 1.75f, atTheDoor: true),
                new Piece("Environment/SM_Env_Stepping_Stone_02", 0.35f, 1.8f, 15f, atTheDoor: true),
                new Piece("Props/SM_Prop_Wood_Pile_02", 0.9f, -1.75f),
                new Piece("Environment/SM_Env_Rock_Pile_05", 2.25f, -1.95f, 25f, scale: 0.22f))
        };
    }
}
