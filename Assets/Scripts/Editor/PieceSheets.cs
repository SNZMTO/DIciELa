using System.Collections.Generic;
using System.IO;
using System.Text;
using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les planches des pièces (SAVOIR, 2.11) : chaque pièce du catalogue sur les huit animaux (leurs têtes A : A et B ne
    /// diffèrent que par les cils et les oreilles du lapin), chacun sur son corps, habillé comme en jeu (Dresser), la tête
    /// avec ses pièces d'office, photographié de près hors de toute scène (Studio) de trois quarts puis de profil : une
    /// planche de 4 × 4 par pièce, deux animaux par ligne, rangée par emplacement dans Temp/Planches (hors Git). Plus les
    /// mesures de chaque pièce sur chaque animal (PieceMeasures, Mesures.csv), et une planche des seize têtes, chacune nue
    /// puis avec ses pièces d'office. Pour juger une pose de près, sans rien toucher à la scène ouverte.
    /// </summary>
    internal static class PieceSheets
    {
        private const string Folder = "Temp/Planches";
        private const int Cell = 320;
        private const int Columns = 4;
        // Les têtes A : une sur deux dans le catalogue.
        private const int AnimalStride = 2;

        [MenuItem("DICIELA/Album/Planches des pièces, sur chaque tête (fichiers)")]
        private static void CreateAll()
        {
            var catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);
            var measures = new StringBuilder(PieceMeasures.Header);
            using (var studio = new Studio(Cell))
            {
                foreach (AttachmentDefinition piece in catalog.Attachments)
                {
                    if (piece == null || piece.Slot == null)
                        continue;
                    var photos = new List<Texture2D>();
                    for (int i = 0; i < catalog.Heads.Count; i += AnimalStride)
                        photos.AddRange(Photos(studio, catalog, i, piece, measures));
                    studio.Save(photos, Columns, Folder + "/" + piece.Slot.name + "/" + piece.name + ".jpg");
                }
                var heads = new List<Texture2D>();
                for (int i = 0; i < catalog.Heads.Count; i++)
                {
                    heads.Add(Wearing(studio, catalog, i, new AttachmentDefinition[0]));
                    heads.Add(Wearing(studio, catalog, i, catalog.Heads[i].Worn));
                }
                studio.Save(heads, Columns, Folder + "/Tetes.jpg");
            }
            File.WriteAllText(Folder + "/Mesures.csv", measures.ToString());
            Debug.Log("DICIELA : planches des pièces écrites dans " + Path.GetFullPath(Folder) + ".");
        }

        // La tête i sur le corps du même animal, avec ses pièces d'office, plus cette pièce à la place de celle de son
        // emplacement ; de trois quarts (de dos pour ce qui se porte derrière) puis de profil, cadrée sur la tête, ou sur
        // le corps pour une pièce du corps. Ses mesures s'ajoutent au tableau.
        private static IEnumerable<Texture2D> Photos(Studio studio, Catalog catalog, int index,
            AttachmentDefinition piece, StringBuilder measures)
        {
            HeadDefinition head = catalog.Heads[index];
            var worn = new List<WornAttachment>();
            foreach (AttachmentDefinition own in head.Worn)
            {
                if (own.Slot != piece.Slot)
                    worn.Add(new WornAttachment(own, own.Palette));
            }
            worn.Add(new WornAttachment(piece, piece.Palette));
            // De dos : ce qui touche le dos, et ce qui se porte derrière le corps sans le toucher (les queues).
            bool behind = piece.Slot.Contact == ContactAxis.Forward || piece.Slot.Contact == ContactAxis.None &&
                          piece.Slot.Frame == AttachmentFrame.Body && piece.Offset.z < 0f;
            AttachmentFrame frame = piece.Slot.Frame;
            GameObject holder = Dressed(studio, catalog, index, worn);
            measures.Append(PieceMeasures.Row(holder.transform, piece, head, catalog.Bodies[index]));
            var photos = new[]
            {
                Shoot(studio, frame, behind ? new Vector3(-1f, 0.35f, -1.6f) : new Vector3(1f, 0.3f, 1.7f)),
                Shoot(studio, frame, new Vector3(1f, 0.15f, 0f))
            };
            Object.DestroyImmediate(holder);
            return photos;
        }

        private static Texture2D Wearing(Studio studio, Catalog catalog, int index, IReadOnlyList<AttachmentDefinition> worn)
        {
            var pieces = new List<WornAttachment>();
            foreach (AttachmentDefinition own in worn)
                pieces.Add(new WornAttachment(own, own.Palette));
            GameObject holder = Dressed(studio, catalog, index, pieces);
            Texture2D photo = Shoot(studio, AttachmentFrame.Crown, new Vector3(1f, 0.3f, 1.7f));
            Object.DestroyImmediate(holder);
            return photo;
        }

        private static GameObject Dressed(Studio studio, Catalog catalog, int index, List<WornAttachment> worn)
        {
            HeadDefinition head = catalog.Heads[index];
            BodyDefinition body = catalog.Bodies[Mathf.Min(index, catalog.Bodies.Count - 1)];
            var holder = new GameObject("Planche");
            studio.Add(holder);
            Dresser.Dress(new Appearance(head, head.Palette, body, body.Palette, worn), holder.transform);
            return holder;
        }

        private static Texture2D Shoot(Studio studio, AttachmentFrame frame, Vector3 direction)
        {
            bool onBody = frame == AttachmentFrame.Body || frame == AttachmentFrame.Neck;
            Vector3 target = onBody ? new Vector3(0f, frame == AttachmentFrame.Neck ? 1.05f : 0.8f, 0f)
                : new Vector3(0f, 1.62f, 0.05f);
            return studio.Shoot(target, direction, onBody ? 0.8f : 0.72f);
        }
    }
}
