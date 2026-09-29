using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des repères (SAVOIR, 2.11) : chaque tête A nue, sur son corps, de face puis de trois quarts d'en haut,
    /// ses repères marqués d'une boule : le haut du crâne en rouge, les deux oreilles en bleu, les yeux en vert, le bas de
    /// la tête en jaune. Pour voir d'un coup d'œil où chaque tête pose ses pièces. Écrite avec les planches des pièces.
    /// </summary>
    internal static class LandmarkSheet
    {
        private const float MarkerMeters = 0.07f;
        private const int AnimalStride = 2;
        private const int Columns = 4;

        [MenuItem("DICIELA/Album/Planche des repères des têtes (fichier)")]
        private static void WriteAlone()
        {
            using (var studio = new Studio(320))
                Write(studio, EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath), "Temp/Planches/Reperes.jpg");
        }

        /// <summary>Photographie les repères des têtes et écrit la planche à ce chemin.</summary>
        public static void Write(Studio studio, Catalog catalog, string path)
        {
            var colors = new Dictionary<AttachmentFrame, Material>
            {
                { AttachmentFrame.Crown, Paint(Color.red) }, { AttachmentFrame.Ears, Paint(Color.blue) },
                { AttachmentFrame.Face, Paint(Color.green) }, { AttachmentFrame.Neck, Paint(Color.yellow) }
            };
            var photos = new List<Texture2D>();
            for (int i = 0; i < catalog.Heads.Count; i += AnimalStride)
            {
                HeadDefinition head = catalog.Heads[i];
                BodyDefinition body = catalog.Bodies[Mathf.Min(i, catalog.Bodies.Count - 1)];
                var holder = new GameObject("Repères");
                studio.Add(holder);
                Dresser.Dress(new Appearance(head, head.Palette, body, body.Palette, new List<WornAttachment>()),
                    holder.transform);
                Mark(holder.transform, head.PointOf(AttachmentFrame.Crown), colors[AttachmentFrame.Crown]);
                Mark(holder.transform, head.EarRight, colors[AttachmentFrame.Ears]);
                Mark(holder.transform, Vector3.Scale(head.EarRight, new Vector3(-1f, 1f, 1f)), colors[AttachmentFrame.Ears]);
                Mark(holder.transform, head.PointOf(AttachmentFrame.Face), colors[AttachmentFrame.Face]);
                Mark(holder.transform, head.PointOf(AttachmentFrame.Neck), colors[AttachmentFrame.Neck]);
                var target = new Vector3(0f, 1.5f, 0.05f);
                photos.Add(studio.Shoot(target, new Vector3(0f, 0.15f, 1f), 0.85f));
                photos.Add(studio.Shoot(target, new Vector3(1f, 1.3f, 1.2f), 0.85f));
                Object.DestroyImmediate(holder);
            }
            studio.Save(photos, Columns, path);
            foreach (Material material in colors.Values)
                Object.DestroyImmediate(material);
        }

        private static void Mark(Transform holder, Vector3 point, Material material)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.SetParent(holder, false);
            marker.transform.localPosition = point;
            marker.transform.localScale = Vector3.one * MarkerMeters;
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Material Paint(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", color);
            return material;
        }
    }
}
