using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée les emplacements et les pièces qui manquent (GDD, section 6), depuis le pack : ses pièces à accrocher
    /// (Prefabs/Characters/Chr_Attach), sauf les cheveux, la barbe et les boucles d'oreilles, faits pour les humains.
    /// Chaque emplacement reçoit son os ; chaque pièce, un point d'accroche de départ, mesuré sur le premier corps
    /// (SAVOIR, 2.11) : sur la tête, sa base un peu sous le haut du crâne ; sur la face, son dos contre l'avant de la
    /// face ; ailleurs, la place d'un os, ou le point que le pack donne lui-même. Le développeur l'affine ensuite dans la
    /// fiche. Réexécutable : une fiche qui existe n'est jamais touchée, sauf pour ce qui lui manque ; un point réglé à la
    /// main reste.
    /// </summary>
    internal static class AttachmentBuilder
    {
        private const string Folder = "Assets/Data/Apparence";
        private const string Pieces = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters/Chr_Attach";
        // Les pièces du pack sont numérotées à partir de 01 ; aucune série ne va au-delà.
        private const int MaxPieceNumber = 20;
        // Une pièce de tête s'enfonce d'au plus ce tiers de sa hauteur : une couronne reste dehors, un chapeau coiffe.
        private const float HeadSinkPerHeight = 0.3f;
        // Une pièce de la face entre de 5 cm dans le museau, qui dépasse des yeux.
        private const float FaceSinkMeters = 0.05f;

        // La queue du chat, que le pack pose lui-même (Tail_02, sur SM_Chr_Cat_Female_01).
        private static readonly Vector3 TailRoot = new Vector3(0f, 0.618f, -0.139f);

        private enum Fit
        {
            Pack,
            Bone,
            Head,
            Face
        }

        // Les emplacements, dans l'ordre des lignes de l'écran : l'os où s'accrochent leurs pièces, et comment se prend
        // leur point de départ, avec sa mesure : l'enfoncement au plus sous le haut du crâne, ou la hauteur sous les yeux.
        private static readonly (string Name, string Bone, Fit Fit, float Meters)[] Slots =
        {
            ("Queue", "pelvis", Fit.Pack, 0f),
            ("Oreilles", "head", Fit.Head, 0.1f),
            ("Museau", "head", Fit.Face, 0.12f),
            ("Tete", "head", Fit.Head, 0.18f),
            ("Visage", "head", Fit.Face, 0f),
            ("Cou", "neck_01", Fit.Bone, 0f),
            ("Dos", "spine_03", Fit.Bone, 0f),
            ("Ceinture", "pelvis", Fit.Bone, 0f)
        };

        // Les pièces : leur nom dans le pack, leur emplacement, leur nom de fichier.
        private static readonly (string Pack, string Slot, string Name)[] Kinds =
        {
            ("Tail", "Queue", "Queue"),
            ("Pig_Ear", "Oreilles", "OreillesCochon"),
            ("Raccon_Ear", "Oreilles", "OreillesRaton"),
            ("Goblin_Ear", "Oreilles", "OreillesGobelin"),
            ("Snout", "Museau", "Museau"),
            ("Hat", "Tete", "Chapeau"),
            ("Helmet", "Tete", "Casque"),
            ("Crown", "Tete", "Couronne"),
            ("Flower", "Tete", "Fleur"),
            ("Bow", "Tete", "Noeud"),
            ("Glasses", "Visage", "Lunettes"),
            ("Monocle", "Visage", "Monocle"),
            ("Scarf", "Cou", "Echarpe"),
            ("Bandana", "Cou", "Bandana"),
            ("Cape", "Dos", "Cape"),
            ("Backpack", "Dos", "SacADos"),
            ("Pouch", "Ceinture", "Bourse"),
            ("Canteen", "Ceinture", "Gourde"),
            ("Potion", "Ceinture", "Potion")
        };

        /// <summary>Les emplacements dans l'ordre de l'écran, puis les pièces dans le leur ; mesurés sur ce personnage.</summary>
        public static void CreateMissing(GameObject character, List<AttachmentSlotDefinition> slots,
            List<AttachmentDefinition> pieces)
        {
            Dictionary<string, Vector3> bones = PackMeasures.Measure(character, out PackMeasures.Head head);
            var slotsByName = new Dictionary<string, AttachmentSlotDefinition>();
            var fits = new Dictionary<string, (string Bone, Fit Fit, float Meters)>();
            foreach ((string name, string bone, Fit fit, float meters) in Slots)
            {
                AttachmentSlotDefinition slot = EditorAssets.CreateIfMissing<AttachmentSlotDefinition>(
                    Folder + "/Emplacements/" + name + ".asset", fiche =>
                    {
                        EditorAssets.Property(fiche, "_id").stringValue = name.ToLowerInvariant();
                        EditorAssets.Property(fiche, "_nameKey").stringValue = "emplacement." + name.ToLowerInvariant();
                    });
                if (string.IsNullOrEmpty(slot.Bone))
                    EditorAssets.Set(slot, fiche => EditorAssets.Property(fiche, "_bone").stringValue = bone);
                slotsByName[name] = slot;
                fits[name] = (bone, fit, meters);
                slots.Add(slot);
            }

            foreach ((string pack, string slot, string name) in Kinds)
            {
                for (int n = 1; n <= MaxPieceNumber; n++)
                {
                    string number = n.ToString("00");
                    string path = Pieces + "/SM_Chr_Attach_" + pack + "_" + number + ".prefab";
                    if (!EditorAssets.Exists(path))
                        continue;
                    string id = (name + "_" + number).ToLowerInvariant();
                    AttachmentDefinition piece = EditorAssets.CreateIfMissing<AttachmentDefinition>(
                        Folder + "/Pieces/" + name + "_" + number + ".asset", fiche =>
                        {
                            EditorAssets.Property(fiche, "_id").stringValue = id;
                            EditorAssets.Property(fiche, "_nameKey").stringValue = "piece." + id;
                        });
                    EditorAssets.AssignIfMissing(piece, "_slot", slotsByName[slot]);
                    EditorAssets.AssignIfMissing(piece, "_model", EditorAssets.Load<GameObject>(path));
                    // Aucune pièce ne se pose aux pieds : un point nul est un point à remplir.
                    if (piece.Anchor == Vector3.zero)
                    {
                        Vector3 anchor = AnchorOf(piece.Model, fits[slot], bones, head);
                        EditorAssets.Set(piece, fiche => EditorAssets.Property(fiche, "_anchor").vector3Value = anchor);
                    }
                    pieces.Add(piece);
                }
            }
        }

        /// <summary>Le point de départ d'une pièce, selon la règle de son emplacement.</summary>
        private static Vector3 AnchorOf(GameObject model, (string Bone, Fit Fit, float Meters) rule,
            Dictionary<string, Vector3> bones, PackMeasures.Head head)
        {
            switch (rule.Fit)
            {
                case Fit.Pack:
                    return TailRoot;
                case Fit.Bone:
                    return bones[rule.Bone];
            }

            Bounds box = PackMeasures.BoxOf(model);
            if (rule.Fit == Fit.Head)
            {
                float sink = Mathf.Min(rule.Meters, box.size.y * HeadSinkPerHeight);
                return new Vector3(0f, head.Top - sink - box.min.y, head.Middle - box.center.z);
            }
            return new Vector3(0f, bones["eyes"].y - rule.Meters - box.center.y,
                head.Front - FaceSinkMeters - box.min.z);
        }
    }
}
