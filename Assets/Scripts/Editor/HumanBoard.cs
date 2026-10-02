using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La planche des humains du pack (GDD, section 16 : les habitants ordinaires sont les humains du pack), en fichier,
    /// sans Play (Studio) : chaque fiche de personnage (VillagerContentBuilder), telle que le pack la fait, de face puis
    /// de trois quarts dos. En haut, les hommes et le boucher ; en bas, les femmes et la tavernière, chaque rang de
    /// gauche à droite. Elle sert à choisir qui sera quel premier habitant.
    /// </summary>
    internal static class HumanBoard
    {
        private const string SheetPath = "Temp/Planches/Humains.jpg";
        private const int Cell = 360;
        private const int Columns = 10;
        // Le cadre d'une photo : le milieu du personnage, et de quoi le voir en pied.
        private static readonly Vector3 Middle = new Vector3(0f, 0.95f, 0f);
        private const float RadiusMeters = 1.05f;

        [MenuItem("DICIELA/Album/Planche des humains du pack (fichier)")]
        private static void Write()
        {
            var photos = new List<Texture2D>();
            using (var studio = new Studio(Cell))
            {
                foreach (string path in VillagerContentBuilder.HumanPaths())
                {
                    var human = EditorAssets.Load<CharacterDefinition>(path);
                    if (human == null || human.Model == null)
                        continue;
                    var character = Object.Instantiate(human.Model);
                    studio.Add(character);
                    photos.Add(studio.Shoot(Middle, new Vector3(0.35f, 0.25f, 1f), RadiusMeters));
                    photos.Add(studio.Shoot(Middle, new Vector3(-0.7f, 0.25f, -1f), RadiusMeters));
                    Object.DestroyImmediate(character);
                }
                studio.Save(photos, Columns, SheetPath);
            }
            Debug.Log("DICIELA : planche des humains du pack écrite : " + SheetPath + ".");
        }
    }
}
