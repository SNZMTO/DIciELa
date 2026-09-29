using System.Collections.Generic;
using System.IO;
using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les planches des pièces (SAVOIR, 2.11) : chaque pièce du catalogue sur chacune des seize têtes, habillées comme en
    /// jeu (Dresser), la tête avec ses pièces d'office, photographiées de près hors de toute scène (Studio), une planche de
    /// 4 × 4 par pièce, rangée par emplacement dans Temp/Planches (hors Git). Plus une planche des têtes, chacune nue puis avec ses
    /// pièces d'office. Pour juger une pose de près, sans rien toucher à la scène ouverte.
    /// </summary>
    internal static class PieceSheets
    {
        private const string Folder = "Temp/Planches";
        private const int Cell = 320;
        private const int Columns = 4;

        [MenuItem("DICIELA/Album/Planches des pièces, sur chaque tête (fichiers)")]
        private static void CreateAll()
        {
            var catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);
            using (var studio = new Studio(Cell))
            {
                foreach (AttachmentDefinition piece in catalog.Attachments)
                {
                    if (piece == null || piece.Slot == null)
                        continue;
                    var photos = new List<Texture2D>();
                    for (int i = 0; i < catalog.Heads.Count; i++)
                        photos.Add(Photo(studio, catalog, i, piece));
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
            Debug.Log("DICIELA : planches des pièces écrites dans " + Path.GetFullPath(Folder) + ".");
        }

        // La tête i sur le corps du même animal, avec ses pièces d'office, plus cette pièce à la place de celle de son
        // emplacement ; cadrée sur la tête, ou sur le corps pour une pièce du corps.
        private static Texture2D Photo(Studio studio, Catalog catalog, int index, AttachmentDefinition piece)
        {
            HeadDefinition head = catalog.Heads[index];
            var worn = new List<WornAttachment>();
            foreach (AttachmentDefinition own in head.Worn)
            {
                if (piece == null || own.Slot != piece.Slot)
                    worn.Add(new WornAttachment(own, own.Palette));
            }
            if (piece != null)
                worn.Add(new WornAttachment(piece, piece.Palette));
            // Ce qui se porte derrière (le dos, la queue) se photographie de dos ; le reste, de face.
            bool behind = piece != null && (piece.Slot.Contact == ContactAxis.Forward ||
                                            piece.Slot.Frame == AttachmentFrame.Body && piece.Offset.z < 0f);
            return Shoot(studio, catalog, index, worn, piece != null ? piece.Slot.Frame : AttachmentFrame.Crown, behind);
        }

        private static Texture2D Wearing(Studio studio, Catalog catalog, int index, IReadOnlyList<AttachmentDefinition> worn)
        {
            var pieces = new List<WornAttachment>();
            foreach (AttachmentDefinition own in worn)
                pieces.Add(new WornAttachment(own, own.Palette));
            return Shoot(studio, catalog, index, pieces, AttachmentFrame.Crown, false);
        }

        private static Texture2D Shoot(Studio studio, Catalog catalog, int index, List<WornAttachment> worn,
            AttachmentFrame frame, bool fromBehind)
        {
            HeadDefinition head = catalog.Heads[index];
            BodyDefinition body = catalog.Bodies[Mathf.Min(index, catalog.Bodies.Count - 1)];
            var holder = new GameObject("Planche");
            studio.Add(holder);
            Dresser.Dress(new Appearance(head, head.Palette, body, body.Palette, worn), holder.transform);
            bool onBody = frame == AttachmentFrame.Body || frame == AttachmentFrame.Neck;
            Vector3 target = onBody ? new Vector3(0f, frame == AttachmentFrame.Neck ? 1.05f : 0.8f, 0f)
                : new Vector3(0f, 1.62f, 0.05f);
            Vector3 direction = fromBehind ? new Vector3(-1f, 0.35f, -1.6f) : new Vector3(1f, 0.3f, 1.7f);
            Texture2D photo = studio.Shoot(target, direction, onBody ? 0.8f : 0.72f);
            Object.DestroyImmediate(holder);
            return photo;
        }
    }
}
