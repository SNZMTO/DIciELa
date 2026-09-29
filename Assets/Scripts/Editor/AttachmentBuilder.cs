using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les emplacements et les pièces du pack (GDD, section 6 ; SAVOIR, 2.11), depuis toutes les pièces à accrocher de
    /// Prefabs/Characters/Chr_Attach. Chaque emplacement reçoit son os, son repère et son contact, déduits du pack et
    /// réécrits à chaque passage ; chaque pièce, son modèle et sa couleur d'origine s'ils lui manquent. Leur réglage de
    /// départ (décalage, taille) vient après, quand les têtes sont mesurées (AttachmentFitter).
    /// </summary>
    internal static class AttachmentBuilder
    {
        private const string Folder = "Assets/Data/Apparence";
        private const string Pieces = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters/Chr_Attach";
        // Les pièces du pack sont numérotées à partir de 01 ; aucune série ne va au-delà.
        private const int MaxPieceNumber = 20;
        private const int AllHuman = int.MaxValue;
        // Savoir du pack : les coiffures 1 à 13 sont dessinées pour les humains, la 14 (la huppe) pour l'oiseau.
        private const int HumanHair = 13;

        /// <summary>Pour qui une pièce du pack est dessinée, et autour de quoi.</summary>
        public enum Design
        {
            /// <summary>Pour une tête d'animal, ou pour le corps : à sa taille.</summary>
            Animal,

            /// <summary>Pour une tête humaine, autour de son os Head, où le pack la pose (chapeaux, oreilles).</summary>
            HumanHead,

            /// <summary>Pour une tête humaine, autour de son os Head, en l'enveloppant (cheveux, barbe).</summary>
            HumanWrap
        }

        /// <summary>Une pièce du pack, pour qui elle est dessinée, et si elle couvre le haut du crâne.</summary>
        public readonly struct PackPiece
        {
            public PackPiece(AttachmentDefinition fiche, Design design, bool coversHead)
            {
                Fiche = fiche;
                Designed = design;
                CoversHead = coversHead;
            }

            public AttachmentDefinition Fiche { get; }
            public Design Designed { get; }
            public bool CoversHead { get; }
        }

        // Les emplacements, dans l'ordre des lignes de l'écran : leur os, leur repère, leur contact, la règle de départ de
        // leurs pièces que le pack ne pose nulle part (un chapeau entre de 2 cm dans le crâne, des cheveux de 1 cm), si
        // leurs pièces vont par paires (les oreilles, en deux moitiés, qui se posent par leur base) et si l'on ne peut pas
        // s'en passer (la queue, les oreilles).
        private static readonly (string Name, string Bone, AttachmentFrame Frame, ContactAxis Contact, SlotRule Rule,
            bool Pairs, bool Required)[]
            Slots =
            {
                ("Queue", "pelvis", AttachmentFrame.Body, ContactAxis.None, new SlotRule(SlotRule.Place.TailRoot, 0f),
                    false, true),
                ("Oreilles", "head", AttachmentFrame.Ears, ContactAxis.Down, new SlotRule(SlotRule.Place.Ears, 0f),
                    true, true),
                ("Cheveux", "head", AttachmentFrame.Crown, ContactAxis.Down, new SlotRule(SlotRule.Place.Crown, 0.01f),
                    false, false),
                ("Chapeau", "head", AttachmentFrame.Crown, ContactAxis.Down, new SlotRule(SlotRule.Place.Crown, 0.02f),
                    false, false),
                ("Visage", "head", AttachmentFrame.Face, ContactAxis.Back, new SlotRule(SlotRule.Place.Eyes, 0f),
                    false, false),
                ("Barbe", "head", AttachmentFrame.Face, ContactAxis.Back, new SlotRule(SlotRule.Place.Nose, 0f),
                    false, false),
                ("Cou", "neck_01", AttachmentFrame.Neck, ContactAxis.Back, new SlotRule(SlotRule.Place.UnderHead, 0f),
                    false, false),
                ("Dos", "spine_03", AttachmentFrame.Body, ContactAxis.Forward, new SlotRule(SlotRule.Place.Back, 0f),
                    false, false),
                ("Ceinture", "pelvis", AttachmentFrame.Body, ContactAxis.Back, new SlotRule(SlotRule.Place.Bone, 0f),
                    false, false)
            };

        // Les pièces : leur nom dans le pack, leur emplacement, leur nom de fichier, jusqu'à quel numéro elles sont
        // dessinées pour une tête humaine et autour de quoi (savoir du pack : les chapeaux 1 à 5, la barbe et les oreilles,
        // autour de l'os Head ; les chapeaux 6 à 8, la huppe 14 et le reste, à la taille des animaux), et si elles couvrent le haut du crâne (les chapeaux et le casque),
        // qui cachent alors d'office les oreilles et les cheveux (les touffes, la huppe), et si elles couvrent le dos (les
        // capes), qui cachent alors la petite cape que certains corps portent d'office, et si elles se teignent comme le poil
        // (la barbe : FurCopy).
        private static readonly (string Pack, string Slot, string Name, int HumanUpTo, Design Human, bool CoversHead,
            bool CoversBack, bool Fur)[]
            Kinds =
            {
                ("Tail", "Queue", "Queue", 0, Design.Animal, false, false, false),
                ("Pig_Ear", "Oreilles", "OreillesCochon", AllHuman, Design.HumanHead, false, false, false),
                ("Raccon_Ear", "Oreilles", "OreillesRaton", AllHuman, Design.HumanHead, false, false, false),
                ("Goblin_Ear", "Oreilles", "OreillesGobelin", AllHuman, Design.HumanHead, false, false, false),
                ("Hair", "Cheveux", "Cheveux", 0, Design.Animal, false, false, false),
                ("Hat", "Chapeau", "Chapeau", 5, Design.HumanHead, true, false, false),
                ("Helmet", "Chapeau", "Casque", 0, Design.Animal, true, false, false),
                ("Crown", "Chapeau", "Couronne", 0, Design.Animal, false, false, false),
                ("Flower", "Chapeau", "Fleur", 0, Design.Animal, false, false, false),
                ("Bow", "Chapeau", "Noeud", 0, Design.Animal, false, false, false),
                ("Glasses", "Visage", "Lunettes", 0, Design.Animal, false, false, false),
                ("Monocle", "Visage", "Monocle", 0, Design.Animal, false, false, false),
                ("Beard", "Barbe", "Barbe", AllHuman, Design.HumanWrap, false, false, true),
                ("Scarf", "Cou", "Echarpe", 0, Design.Animal, false, false, false),
                ("Bandana", "Cou", "Bandana", 0, Design.Animal, false, false, false),
                ("Cape", "Dos", "Cape", 0, Design.Animal, false, true, false),
                ("Backpack", "Dos", "SacADos", 0, Design.Animal, false, false, false),
                ("Pouch", "Ceinture", "Bourse", 0, Design.Animal, false, false, false),
                ("Canteen", "Ceinture", "Gourde", 0, Design.Animal, false, false, false),
                ("Potion", "Ceinture", "Potion", 0, Design.Animal, false, false, false)
            };

        /// <summary>
        /// Les emplacements dans l'ordre de l'écran, par nom de fichier ; os, repère et contact réécrits ; rules reçoit la
        /// règle de départ de chacun.
        /// </summary>
        public static Dictionary<string, AttachmentSlotDefinition> CreateSlots(List<AttachmentSlotDefinition> slots,
            Dictionary<AttachmentSlotDefinition, SlotRule> rules)
        {
            var byName = new Dictionary<string, AttachmentSlotDefinition>();
            foreach ((string name, string bone, AttachmentFrame frame, ContactAxis contact, SlotRule rule, bool pairs,
                         bool required) in Slots)
            {
                string id = name.ToLowerInvariant();
                AttachmentSlotDefinition slot = EditorAssets.CreateIfMissing<AttachmentSlotDefinition>(
                    Folder + "/Emplacements/" + name + ".asset", fiche =>
                    {
                        EditorAssets.Property(fiche, "_id").stringValue = id;
                        EditorAssets.Property(fiche, "_nameKey").stringValue = "emplacement." + id;
                    });
                EditorAssets.Set(slot, fiche =>
                {
                    EditorAssets.Property(fiche, "_bone").stringValue = bone;
                    EditorAssets.Property(fiche, "_frame").enumValueIndex = (int)frame;
                    EditorAssets.Property(fiche, "_contact").enumValueIndex = (int)contact;
                    EditorAssets.Property(fiche, "_pairs").boolValue = pairs;
                    EditorAssets.Property(fiche, "_required").boolValue = required;
                });
                byName[name] = slot;
                rules[slot] = rule;
                slots.Add(slot);
            }
            return byName;
        }

        /// <summary>Les pièces du pack, dans l'ordre de la table ; créées si elles manquent, modèle et couleur remplis.</summary>
        public static List<PackPiece> CreatePieces(Dictionary<string, AttachmentSlotDefinition> slots,
            List<PaletteDefinition> palettes)
        {
            var pieces = new List<PackPiece>();
            foreach ((string pack, string slot, string name, int humanUpTo, Design human, bool coversHead, bool coversBack,
                         bool fur) in Kinds)
            {
                for (int n = 1; n <= MaxPieceNumber; n++)
                {
                    // Les coiffures dessinées pour les humains ne sont pas proposées : seulement celles des animaux.
                    if (pack == "Hair" && n <= HumanHair)
                        continue;
                    string number = n.ToString("00");
                    string path = Pieces + "/SM_Chr_Attach_" + pack + "_" + number + ".prefab";
                    if (!EditorAssets.Exists(path))
                        continue;
                    var model = EditorAssets.Load<GameObject>(path);
                    string id = (name + "_" + number).ToLowerInvariant();
                    AttachmentDefinition piece = EditorAssets.CreateIfMissing<AttachmentDefinition>(
                        Folder + "/Pieces/" + name + "_" + number + ".asset", fiche =>
                        {
                            EditorAssets.Property(fiche, "_id").stringValue = id;
                            EditorAssets.Property(fiche, "_nameKey").stringValue = "piece." + id;
                        });
                    EditorAssets.AssignIfMissing(piece, "_slot", slots[slot]);
                    // Une paire d'oreilles du pack se copie en deux moitiés : la fiche porte la copie.
                    if (slots[slot].Pairs)
                        EditorAssets.Set(piece, fiche => EditorAssets.Property(fiche, "_model").objectReferenceValue =
                            EarHalves.Copy(model, name + "_" + number));
                    else if (fur)
                        EditorAssets.Set(piece, fiche => EditorAssets.Property(fiche, "_model").objectReferenceValue =
                            FurCopy.Copy(model, name + "_" + number, palettes));
                    else
                        EditorAssets.AssignIfMissing(piece, "_model", model);
                    EditorAssets.Set(piece, fiche => EditorAssets.Property(fiche, "_coversBack").boolValue = coversBack);
                    EditorAssets.AssignIfMissing(piece, "_palette",
                        PaletteBuilder.PaletteOf(model.GetComponentInChildren<Renderer>(true), palettes));
                    pieces.Add(new PackPiece(piece, n <= humanUpTo ? human : Design.Animal, coversHead));
                }
            }
            return pieces;
        }
    }
}
