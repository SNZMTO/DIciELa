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
    /// La planche d'essais (SAVOIR, 2.11) : des Chimères composées à la main, photographiées de face (comme dans
    /// l'atelier), de trois quarts d'en haut (comme en ville) et de dos, une par ligne. La liste est un fichier texte,
    /// Temp/Planches/Essais.txt, une Chimère par ligne : l'identifiant de la tête, celui du corps, puis les noms des
    /// fiches de ses pièces, séparés par des points-virgules (« grenouille_a;grenouille_a;Oreilles_Cochon »). Pour juger
    /// une combinaison sans passer par l'écran de création.
    /// </summary>
    internal static class TrialSheet
    {
        private const string List = "Temp/Planches/Essais.txt";
        private const string Sheet = "Temp/Planches/Essais.jpg";

        [MenuItem("DICIELA/Album/Planche d'essais, d'après Temp/Planches/Essais.txt")]
        private static void Write()
        {
            if (!File.Exists(List))
            {
                Debug.LogError("DICIELA : pas de liste d'essais dans " + List + ".");
                return;
            }
            var catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);
            var photos = new List<Texture2D>();
            using (var studio = new Studio(480))
            {
                foreach (string line in File.ReadAllLines(List))
                {
                    string[] parts = line.Split(';');
                    if (parts.Length < 2)
                        continue;
                    HeadDefinition head = Find(catalog.Heads, parts[0].Trim());
                    BodyDefinition body = Find(catalog.Bodies, parts[1].Trim());
                    if (head == null || body == null)
                    {
                        Debug.LogError("DICIELA : essai illisible : « " + line + " ».");
                        continue;
                    }
                    var worn = new List<WornAttachment>();
                    for (int i = 2; i < parts.Length; i++)
                    {
                        foreach (AttachmentDefinition piece in catalog.Attachments)
                        {
                            if (piece != null && piece.name == parts[i].Trim())
                                worn.Add(new WornAttachment(piece, piece.Palette));
                        }
                    }
                    var holder = new GameObject("Essai");
                    studio.Add(holder);
                    Dresser.Dress(new Appearance(head, head.Palette, body, body.Palette, worn), holder.transform);
                    var target = new Vector3(0f, 1.55f, 0.05f);
                    photos.Add(studio.Shoot(target, new Vector3(0f, 0.12f, 1f), 0.95f));
                    photos.Add(studio.Shoot(target, new Vector3(0.8f, 1.1f, 1.2f), 0.95f));
                    photos.Add(studio.Shoot(target, new Vector3(0.3f, 0.2f, -1f), 0.95f));
                    Object.DestroyImmediate(holder);
                }
                studio.Save(photos, 3, Sheet);
            }
            Debug.Log("DICIELA : planche d'essais écrite dans " + Path.GetFullPath(Sheet) + ".");
        }

        private static T Find<T>(IReadOnlyList<T> fiches, string id) where T : ScriptableObject
        {
            foreach (T fiche in fiches)
            {
                if (fiche is HeadDefinition head && head.Id == id || fiche is BodyDefinition body && body.Id == id)
                    return fiche;
            }
            return null;
        }
    }
}
