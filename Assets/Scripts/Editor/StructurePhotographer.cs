using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Photographie chaque construction du catalogue pour sa carte dans la barre (GDD, section 16) : son modèle préparé,
    /// de trois quarts, face à sa porte, sur le lin de la carte, en triple résolution, les coins coupés comme la
    /// vignette de la maquette. Hors de la scène ouverte (Studio). Réexécutable : les photos sont refaites et reliées à
    /// leur fiche.
    /// </summary>
    internal static class StructurePhotographer
    {
        private const string Folder = "Assets/UI/Vignettes";
        // La vignette d'une carte, en pixels de l'interface (la carte de 188 px, moins son rembourrage), et son pan.
        private const int Width = 168;
        private const int Height = 120;
        private const float Cut = 4f;
        private const int Resolution = 3;
        private const int Samples = 4;
        // De trois quarts, face à la porte, qui est au nord ; la construction remplit la hauteur de la vignette.
        private static readonly Vector3 Direction = new Vector3(0.6f, 0.55f, 1f);
        private const float Margin = 1.08f;

        [MenuItem("DICIELA/Interface/Photographier les constructions")]
        public static void PhotographAll()
        {
            var catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);
            Color linen = UssPalette.Read().Get("lin");
            EditorAssets.EnsureFolder(Folder);
            int shots = 0;
            using (var studio = new Studio(Width * Resolution, linen))
            {
                foreach (StructureFamilyDefinition family in catalog.Families)
                {
                    foreach (StructureDefinition structure in family.Structures)
                    {
                        if (structure == null || structure.Model == null)
                            continue;
                        string path = Folder + "/" + structure.Id + ".png";
                        InterfaceImage.Save(path, Width * Resolution, Height * Resolution, Shoot(studio, structure.Model),
                            true);
                        EditorAssets.Set(structure, fiche =>
                            EditorAssets.Property(fiche, "_picture").objectReferenceValue =
                                AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                        shots++;
                    }
                }
            }
            Debug.Log("DICIELA : " + shots + " constructions photographiées dans " + Folder + ".");
        }

        // La photo carrée, recadrée au milieu à la taille de la vignette, les coins coupés.
        private static Color[] Shoot(Studio studio, GameObject model)
        {
            GameObject subject = Object.Instantiate(model);
            studio.Add(subject);
            Bounds bounds = BoundsOf(subject);
            Texture2D photo = studio.Shoot(bounds.center, Direction, Framing(bounds) * Margin);
            Object.DestroyImmediate(subject);

            int width = Width * Resolution;
            int height = Height * Resolution;
            Color[] pixels = photo.GetPixels(0, (photo.height - height) / 2, width, height);
            Object.DestroyImmediate(photo);
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                    pixels[row * width + column].a = Coverage(column, row, width, height);
            }
            return pixels;
        }

        // Le rayon du cadrage au plus juste : chaque coin de la construction, vu dans l'axe de la caméra, tient dans la
        // largeur de la photo et dans la hauteur de la vignette, qui en est la bande du milieu.
        private static float Framing(Bounds bounds)
        {
            Vector3 forward = -Direction.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right);
            float band = (float)Height / Width;
            float radius = 0f;
            for (int corner = 0; corner < 8; corner++)
            {
                var offset = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f,
                    (corner & 4) == 0 ? -1f : 1f);
                Vector3 point = Vector3.Scale(bounds.extents, offset);
                radius = Mathf.Max(radius, Mathf.Abs(Vector3.Dot(point, right)), Mathf.Abs(Vector3.Dot(point, up)) / band);
            }
            return radius;
        }

        private static Bounds BoundsOf(GameObject subject)
        {
            Renderer[] renderers = subject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(subject.transform.position, Vector3.one);
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        // La part du pixel dans le rectangle aux coins coupés (plusieurs points par pixel, pour des pans lissés).
        private static float Coverage(int column, int row, int width, int height)
        {
            float cut = Cut * Resolution;
            int inside = 0;
            for (int sy = 0; sy < Samples; sy++)
            {
                for (int sx = 0; sx < Samples; sx++)
                {
                    float x = column + (sx + 0.5f) / Samples;
                    float y = row + (sy + 0.5f) / Samples;
                    if (x + y >= cut && width - x + y >= cut && x + height - y >= cut && width - x + height - y >= cut)
                        inside++;
                }
            }
            return inside / (float)(Samples * Samples);
        }

    }
}
