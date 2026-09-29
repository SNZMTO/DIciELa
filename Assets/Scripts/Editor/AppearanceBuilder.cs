using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée ou met à jour l'apparence (GDD, section 6 ; SAVOIR, 2.11), depuis le pack, dans cet ordre : les couleurs ; les
    /// emplacements et les pièces du pack, avec la face de chacune qui touche le personnage ; les seize silhouettes
    /// d'animaux, découpées et mesurées, avec les pièces détachées de leurs têtes (AnimalBuilder) ; puis le réglage de
    /// départ des pièces du pack qui n'en ont pas (AttachmentFitter), mesuré sur la tête du chat ; enfin le catalogue, qui
    /// gagne ce qui lui manque, les pièces détachées des têtes en tête de liste. Réexécutable.
    /// </summary>
    internal static class AppearanceBuilder
    {
        private const string Characters = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters";
        private const string HumanReference = Characters + "/SM_Chr_Male_01.prefab";

        // Les animaux du pack, dans l'ordre de l'écran : le chat d'abord, la Chimère de départ, et la tête de référence des
        // réglages.
        private static readonly (string Pack, string Name)[] Animals =
        {
            ("Cat", "Chat"), ("Pig", "Cochon"), ("Frog", "Grenouille"), ("Bunny", "Lapin"), ("Bird", "Oiseau"),
            ("Racoon", "RatonLaveur"), ("Fox", "Renard"), ("Mouse", "Souris")
        };

        public static void CreateMissing(Catalog catalog)
        {
            List<PaletteDefinition> palettes = PaletteBuilder.CreateMissing();
            var slots = new List<AttachmentSlotDefinition>();
            var rules = new Dictionary<AttachmentSlotDefinition, SlotRule>();
            Dictionary<string, AttachmentSlotDefinition> slotsByName = AttachmentBuilder.CreateSlots(slots, rules);
            List<AttachmentBuilder.PackPiece> packPieces = AttachmentBuilder.CreatePieces(slotsByName, palettes);
            var byMesh = new Dictionary<Mesh, AttachmentDefinition>();
            var geometries = new Dictionary<AttachmentDefinition, PackMeasures.Geometry>();
            foreach (AttachmentBuilder.PackPiece packPiece in packPieces)
            {
                AttachmentDefinition piece = packPiece.Fiche;
                MeshFilter filter = piece.Model.GetComponentInChildren<MeshFilter>(true);
                if (filter != null && filter.sharedMesh != null)
                    byMesh[filter.sharedMesh] = piece;
                PackMeasures.Geometry geometry = PackMeasures.GeometryOf(piece.Model);
                geometries[piece] = geometry;
                SurfaceBaker.ShapeOf(piece, geometry.Positions, geometry.Triangles, piece.Slot.Contact);
            }

            var context = new AnimalContext(palettes, slotsByName, PackPoses.Read(), byMesh);
            var animals = new List<AnimalBuilder.Animal>();
            var ownPieces = new List<AttachmentDefinition>();
            foreach ((string pack, string name) in Animals)
                animals.AddRange(AnimalBuilder.Build(pack, name, context, ownPieces));

            Fit(packPieces, geometries, rules, animals, context, new[] { slotsByName["Oreilles"], slotsByName["Cheveux"] });
            var heads = new List<HeadDefinition>();
            var bodies = new List<BodyDefinition>();
            foreach (AnimalBuilder.Animal animal in animals)
            {
                heads.Add(animal.Head);
                bodies.Add(animal.Body);
            }
            var packFiches = packPieces.ConvertAll(packPiece => packPiece.Fiche);
            AddMissing(catalog, "_heads", heads, false);
            AddMissing(catalog, "_bodies", bodies, false);
            AddMissing(catalog, "_palettes", palettes, false);
            AddMissing(catalog, "_attachmentSlots", slots, false);
            AddMissing(catalog, "_attachments", packFiches, false);
            AddMissing(catalog, "_attachments", ownPieces, true);
        }

        /// <summary>
        /// Le réglage de départ des pièces du pack : la pose du pack sur un animal, sinon sur un humain, sinon la règle de
        /// leur emplacement ; mesurés sur le chat, la première silhouette.
        /// </summary>
        private static void Fit(List<AttachmentBuilder.PackPiece> packPieces,
            Dictionary<AttachmentDefinition, PackMeasures.Geometry> geometries,
            Dictionary<AttachmentSlotDefinition, SlotRule> rules, List<AnimalBuilder.Animal> animals,
            AnimalContext context, AttachmentSlotDefinition[] underHats)
        {
            AnimalBuilder.Animal reference = animals[0];
            var humanModel = EditorAssets.Load<GameObject>(HumanReference);
            HumanHead human = HumanHead.Of(humanModel);
            Dictionary<string, Vector3> bones = PackMeasures.Bones(reference.Model);
            Vector3 tailRoot = bones["pelvis"];
            foreach (PackPoses.Pose pose in context.Poses.On(reference.Model))
            {
                if (pose.Bone == "pelvis")
                    tailRoot = pose.Pivot;
            }
            var wearers = new Dictionary<GameObject, HumanHead> { { humanModel, human } };
            foreach (AttachmentBuilder.PackPiece packPiece in packPieces)
            {
                AttachmentDefinition piece = packPiece.Fiche;
                SlotRule rule = rules[piece.Slot];
                AttachmentSlotDefinition[] hides = packPiece.CoversHead ? underHats : null;
                if (context.Poses.TryFind(piece.Model, false, out PackPoses.Pose onAnimal))
                {
                    AnimalBuilder.Animal wearer = animals.Find(animal => animal.Model == onAnimal.Character);
                    AttachmentFitter.FitToPose(piece, onAnimal.Pivot, wearer.Head, wearer.Body);
                }
                else if (packPiece.Designed == AttachmentBuilder.Design.HumanHead ||
                         packPiece.Designed == AttachmentBuilder.Design.HumanWrap)
                {
                    // Le porteur humain que le pack lui donne, sinon l'humain de référence : elle se tient autour de son
                    // os Head.
                    GameObject character = context.Poses.TryFind(piece.Model, true, out PackPoses.Pose onHuman)
                        ? onHuman.Character
                        : humanModel;
                    if (!wearers.TryGetValue(character, out HumanHead wearer))
                        wearers[character] = wearer = HumanHead.Of(character);
                    AttachmentFitter.FitHuman(piece, rule, packPiece.Designed == AttachmentBuilder.Design.HumanWrap,
                        wearer, reference.Measures, reference.Head, geometries[piece], hides);
                }
                else
                {
                    AttachmentFitter.FitByRule(piece, rule, packPiece.Designed == AttachmentBuilder.Design.HumanPoint,
                        geometries[piece], reference.Measures, reference.Head, human.Measures, bones, tailRoot, hides);
                }
            }
        }

        /// <summary>
        /// Ajoute à une liste du catalogue ce qui lui manque, à la suite (ou en tête, dans l'ordre donné) ; ce qui y est
        /// reste où on l'a mis.
        /// </summary>
        private static void AddMissing<T>(Catalog catalog, string listName, List<T> items, bool atFront) where T : Object
        {
            var serialized = new SerializedObject(catalog);
            SerializedProperty list = EditorAssets.Property(serialized, listName);
            var present = new HashSet<Object>();
            for (int i = 0; i < list.arraySize; i++)
                present.Add(list.GetArrayElementAtIndex(i).objectReferenceValue);
            int at = 0;
            foreach (T item in items)
            {
                if (item == null || !present.Add(item))
                    continue;
                int index = atFront ? at++ : list.arraySize;
                list.InsertArrayElementAtIndex(index);
                list.GetArrayElementAtIndex(index).objectReferenceValue = item;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }
    }
}
