using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Un modèle fait d'objets du pack tels quels (SAVOIR, 2.9 ; décision 170) : son nom, la fiche de la construction dont
    /// il devient le modèle source, et ses objets, chacun à sa place. Les deux du jeu : le campement, logis de départ de la
    /// Chimère (un feu, une couche, un coffre, un sac, une marmite ; deux pierres plates sous un objet « Door » font son
    /// entrée), et le tas de départ (une pile de bûches et un tas de pierres : ce qu'il contient). Les rochers du pack sont
    /// à l'échelle du paysage : le foyer et le tas de pierres en sont des réductions.
    /// </summary>
    internal sealed class PackComposition
    {
        private const string Pack = "Assets/Synty/PolygonFantasyVillage/Prefabs/";

        private PackComposition(string name, string structurePath, params Piece[] pieces)
        {
            Name = name;
            StructurePath = structurePath;
            Pieces = pieces;
        }

        public string Name { get; }
        public string StructurePath { get; }
        public Piece[] Pieces { get; }

        /// <summary>
        /// Un objet de la composition : son prefab du pack, sa place (en mètres, l'entrée vers +z), son quart de tour, son
        /// inclinaison vers l'avant, son échelle, et s'il fait l'entrée.
        /// </summary>
        public readonly struct Piece
        {
            public readonly string Path;
            public readonly Vector3 Place;
            public readonly float Yaw;
            public readonly float Pitch;
            public readonly float Scale;
            public readonly bool AtTheDoor;

            public Piece(string path, float x, float z, float yaw = 0f, float y = 0f, float scale = 1f,
                float pitch = 0f, bool atTheDoor = false)
            {
                Path = Pack + path + ".prefab";
                Place = new Vector3(x, y, z);
                Yaw = yaw;
                Pitch = pitch;
                Scale = scale;
                AtTheDoor = atTheDoor;
            }
        }

        public static readonly PackComposition[] All =
        {
            // Le cercle de pierres du pack (6,2 m, un cercle de place) réduit au quart fait le foyer ; la flamme de
            // bougie du pack, agrandie, fait le feu.
            new PackComposition("Campement", ContentBuilder.TentPath,
                new Piece("Environment/SM_Env_Stone_Ring_01", 0f, 0.1f, scale: 0.25f),
                new Piece("FX/FX_Candle_Flame_01", 0f, 0.1f, y: 0.05f, scale: 5f),
                new Piece("Props/SM_Prop_Picnic_Rug_01", -1.75f, -0.1f),
                new Piece("Props/SM_Prop_Pillow_01", -1.75f, -0.95f, y: 0.05f),
                new Piece("Props/SM_Prop_Cooking_Pot_01", 0.95f, -0.75f, 30f),
                new Piece("Props/SM_Prop_Chest_01", 1.75f, 0.55f, -90f),
                new Piece("Props/SM_Prop_Sack_01", 1.45f, -0.35f),
                new Piece("Environment/SM_Env_Stepping_Stone_01", -0.35f, 1.75f, atTheDoor: true),
                new Piece("Environment/SM_Env_Stepping_Stone_02", 0.35f, 1.8f, 15f, atTheDoor: true)),
            new PackComposition("TasDeDepart", TownContentBuilder.PilePath,
                new Piece("Props/SM_Prop_Wood_Pile_02", -0.55f, 0f),
                new Piece("Environment/SM_Env_Rock_Pile_05", 0.8f, -0.2f, 25f, scale: 0.22f))
        };
    }
}
