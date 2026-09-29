using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les fiches d'un animal du pack, en ses deux silhouettes A et B (SAVOIR, 2.11) : chacune découpée (HeadShells,
    /// BodyCutter) en un crâne et un corps ; ses oreilles, ses touffes et son museau détachés en pièces (HeadPartCutter),
    /// une seule fois quand A et B ont les mêmes ; sa tête mesurée (HeadMeasures) et ses surfaces relevées (SurfaceBaker).
    /// Ce qui se déduit du pack (maillages, points, surfaces, pièces portées d'office) est réécrit à chaque passage ; ce
    /// qui se règle à la main (le réglage d'une pièce) ne l'est jamais.
    /// </summary>
    internal static class AnimalBuilder
    {
        private const string Folder = "Assets/Data/Apparence";
        private const string Characters = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters";
        // Les surfaces : le dessus de la tête (x, z), l'avant de la face (x, y), le dos du corps (x, y), en cases de 5 cm.
        private static readonly Rect TopArea = Rect.MinMaxRect(-0.7f, -0.6f, 0.7f, 0.6f);
        private static readonly Rect FrontArea = Rect.MinMaxRect(-0.7f, 0.9f, 0.7f, 2.1f);
        private static readonly Rect BackArea = Rect.MinMaxRect(-0.6f, 0.3f, 0.6f, 1.3f);
        private const float Cell = 0.05f;
        private static readonly (string Pack, string Letter)[] Builds = { ("Female", "A"), ("Male", "B") };
        // Les morceaux détachables, le nom de leur fichier et leur emplacement.
        private static readonly (HeadShells.Kind Kind, string Name, string Slot)[] Parts =
        {
            (HeadShells.Kind.Ears, "Oreilles", "Oreilles"), (HeadShells.Kind.Tufts, "Touffes", "Cheveux"),
            (HeadShells.Kind.Snout, "Museau", "Museau")
        };

        /// <summary>Une silhouette montée : sa tête, son corps, les points de sa tête, son personnage du pack.</summary>
        public readonly struct Animal
        {
            public Animal(HeadDefinition head, BodyDefinition body, HeadMeasures measures, GameObject model)
            {
                Head = head;
                Body = body;
                Measures = measures;
                Model = model;
            }

            public HeadDefinition Head { get; }
            public BodyDefinition Body { get; }
            public HeadMeasures Measures { get; }
            public GameObject Model { get; }
        }

        /// <summary>
        /// Les deux silhouettes de cet animal (pack : Cat ; name : Chat) ; ownPieces reçoit les pièces détachées de ses
        /// têtes.
        /// </summary>
        public static List<Animal> Build(string pack, string name, AnimalContext context,
            List<AttachmentDefinition> ownPieces)
        {
            var cuts = new List<AnimalCut>();
            foreach ((string build, string letter) in Builds)
            {
                var model = EditorAssets.Load<GameObject>(Characters + "/SM_Chr_" + pack + "_" + build + "_01.prefab");
                cuts.Add(AnimalCut.Of(model, name + "_" + letter, BackArea, Cell));
            }

            // A et B partagent une pièce quand elle est la même ; sinon chacune a la sienne, nommée de sa lettre. Comparées
            // avant tout enregistrement, qui peut remplacer un maillage en place.
            var shared = new Dictionary<HeadShells.Kind, bool>();
            foreach ((HeadShells.Kind kind, string _, string _) in Parts)
            {
                shared[kind] = cuts.TrueForAll(other => other.Parts.ContainsKey(kind)) &&
                               cuts.TrueForAll(other => HeadPartCutter.Same(cuts[0].Parts[kind], other.Parts[kind]));
            }
            var animals = new List<Animal>();
            var saved = new Dictionary<string, AttachmentDefinition>();
            for (int i = 0; i < cuts.Count; i++)
            {
                AnimalCut cut = cuts[i];
                string file = name + "_" + Builds[i].Letter;
                PaletteDefinition palette = cut.Palette(context.Palettes);
                HeadDefinition head = Head(file, cut, palette);
                BodyDefinition body = Body(file, cut, palette);
                var worn = new List<AttachmentDefinition>();
                foreach ((HeadShells.Kind kind, string part, string slot) in Parts)
                {
                    if (!cut.Parts.TryGetValue(kind, out HeadPartCutter.Part piece))
                        continue;
                    string partFile = part + "_" + (shared[kind] ? name : file);
                    if (!saved.TryGetValue(partFile, out AttachmentDefinition fiche))
                    {
                        saved[partFile] = fiche = OwnPiece(partFile, piece, cut.Material, context.Slots[slot], palette);
                        ownPieces.Add(fiche);
                    }
                    worn.Add(fiche);
                    AttachmentFitter.FitToPose(fiche, piece.Pivot, head, body);
                }
                SplitPackWorn(cut.Model, context, worn, out List<AttachmentDefinition> bodyWorn);
                EditorAssets.Set(head, fiche => EditorAssets.SetList(EditorAssets.Property(fiche, "_worn"), worn));
                EditorAssets.Set(body, fiche => EditorAssets.SetList(EditorAssets.Property(fiche, "_worn"), bodyWorn));
                animals.Add(new Animal(head, body, cut.Measures, cut.Model));
            }
            return animals;
        }

        private static HeadDefinition Head(string file, AnimalCut cut, PaletteDefinition palette)
        {
            string id = file.ToLowerInvariant();
            HeadDefinition head = EditorAssets.CreateIfMissing<HeadDefinition>(Folder + "/Tetes/" + file + ".asset",
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = id;
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "tete." + id;
                });
            EditorAssets.AssignIfMissing(head, "_mesh", cut.Skull);
            EditorAssets.AssignIfMissing(head, "_palette", palette);
            SurfaceMap top = SurfaceBaker.Surface(cut.Positions, cut.SkullTriangles, ContactAxis.Down, TopArea, Cell);
            SurfaceMap front = SurfaceBaker.Surface(cut.Positions, cut.SkullTriangles, ContactAxis.Back, FrontArea, Cell);
            EditorAssets.Set(head, fiche =>
            {
                EditorAssets.Property(fiche, "_crown").vector3Value = cut.Measures.Crown;
                EditorAssets.Property(fiche, "_face").vector3Value = cut.Measures.Face;
                EditorAssets.Property(fiche, "_ears").vector3Value = cut.Measures.Ears;
                EditorAssets.Property(fiche, "_neck").vector3Value = cut.Measures.Neck;
                EditorAssets.Property(fiche, "_top").boxedValue = top;
                EditorAssets.Property(fiche, "_front").boxedValue = front;
            });
            return head;
        }

        private static BodyDefinition Body(string file, AnimalCut cut, PaletteDefinition palette)
        {
            string id = file.ToLowerInvariant();
            BodyDefinition body = EditorAssets.CreateIfMissing<BodyDefinition>(Folder + "/Corps/" + file + ".asset",
                fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = id;
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "corps." + id;
                });
            EditorAssets.AssignIfMissing(body, "_model", cut.Model);
            EditorAssets.AssignIfMissing(body, "_mesh", cut.Body);
            EditorAssets.AssignIfMissing(body, "_palette", palette);
            EditorAssets.Set(body, fiche =>
            {
                EditorAssets.Property(fiche, "_back").boxedValue = cut.Back;
                EditorAssets.Property(fiche, "_front").boxedValue = cut.Front;
            });
            return body;
        }

        /// <summary>La fiche d'une pièce détachée d'une tête : son maillage enregistré, sa couleur, sa face qui touche.</summary>
        private static AttachmentDefinition OwnPiece(string file, HeadPartCutter.Part part, Material material,
            AttachmentSlotDefinition slot, PaletteDefinition palette)
        {
            // Enregistrée, la pièce peut avoir remplacé en place un maillage déjà là : on relit celui du modèle.
            GameObject model = HeadPartCutter.Save(part, file, material);
            Mesh mesh = model.GetComponent<MeshFilter>().sharedMesh;
            string id = file.ToLowerInvariant();
            AttachmentDefinition piece = EditorAssets.CreateIfMissing<AttachmentDefinition>(
                Folder + "/Pieces/" + file + ".asset", fiche =>
                {
                    EditorAssets.Property(fiche, "_id").stringValue = id;
                    EditorAssets.Property(fiche, "_nameKey").stringValue = "piece." + id;
                });
            EditorAssets.AssignIfMissing(piece, "_slot", slot);
            EditorAssets.AssignIfMissing(piece, "_model", model);
            EditorAssets.AssignIfMissing(piece, "_palette", palette);
            SurfaceBaker.ShapeOf(piece, mesh.vertices, new List<int>(mesh.triangles), slot.Contact);
            return piece;
        }

        /// <summary>
        /// Les pièces du pack que ce personnage porte d'office : sous l'os head, elles vont avec la tête (la huppe) ;
        /// ailleurs, avec le corps (les queues).
        /// </summary>
        private static void SplitPackWorn(GameObject model, AnimalContext context, List<AttachmentDefinition> head,
            out List<AttachmentDefinition> body)
        {
            body = new List<AttachmentDefinition>();
            foreach (PackPoses.Pose pose in context.Poses.On(model))
            {
                if (!context.ByMesh.TryGetValue(pose.Mesh, out AttachmentDefinition piece))
                    continue;
                List<AttachmentDefinition> owner = pose.Bone == "head" ? head : body;
                if (!owner.Contains(piece))
                    owner.Add(piece);
            }
        }
    }
}
