using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Mesure les cartes de surface (SurfaceMap) par des rayons sur des triangles (MeshRay) : pour chaque case d'une grille
    /// posée dans un plan, un rayon venu de loin le long du troisième axe, et la place du premier triangle touché. Le
    /// dessus d'une tête se prend d'en haut, sa face de devant, le dos d'un corps de derrière ; la face d'une pièce qui
    /// touche se prend du côté d'où vient ce qu'elle touche (le dessous d'un chapeau d'en bas).
    /// </summary>
    internal static class SurfaceBaker
    {
        private const float Far = 10f;
        // La face d'une pièce qui touche se relève en autant de cases au plus sur son plus grand côté.
        private const int ShapeCells = 24;

        /// <summary>La surface d'une tête ou d'un corps, vue de ce côté, sur ce rectangle du plan, en cases de cell.</summary>
        public static SurfaceMap Surface(Vector3[] positions, List<int> triangles, ContactAxis axis, Rect area, float cell)
        {
            // Le dessus se prend d'en haut, la face de devant, le dos de derrière.
            Vector3 from = axis == ContactAxis.Down ? Vector3.down : axis == ContactAxis.Back ? Vector3.back : Vector3.forward;
            return Bake(positions, triangles, axis, area, cell, from);
        }

        /// <summary>La face d'une pièce qui touche la surface de ce contact, sur sa boîte, en cells cases au plus par côté.</summary>
        public static SurfaceMap Shape(Vector3[] positions, List<int> triangles, ContactAxis axis, Bounds box, int cells)
        {
            // Le dessous se prend d'en bas, le dos de derrière, le devant de devant.
            Vector3 from = axis == ContactAxis.Down ? Vector3.up : axis == ContactAxis.Back ? Vector3.forward : Vector3.back;
            Vector2 min = Plane(axis, box.min), max = Plane(axis, box.max);
            float cell = Mathf.Max(max.x - min.x, max.y - min.y) / cells;
            return Bake(positions, triangles, axis, Rect.MinMaxRect(min.x, min.y, max.x, max.y), Mathf.Max(cell, 0.005f),
                from);
        }

        /// <summary>Relève la face d'une pièce qui touche la surface de son contact, dans ses axes ; rien sans contact.</summary>
        public static void ShapeOf(AttachmentDefinition piece, Vector3[] positions, List<int> triangles, ContactAxis axis)
        {
            if (axis == ContactAxis.None || positions.Length == 0)
                return;
            var box = new Bounds(positions[0], Vector3.zero);
            foreach (Vector3 position in positions)
                box.Encapsulate(position);
            SurfaceMap shape = Shape(positions, triangles, axis, box, ShapeCells);
            EditorAssets.Set(piece, fiche => EditorAssets.Property(fiche, "_shape").boxedValue = shape);
        }

        // Pour chaque case, un rayon lancé le long de direction depuis loin derrière lui ; la case garde la coordonnée,
        // sur l'axe du rayon, du premier triangle touché.
        private static SurfaceMap Bake(Vector3[] positions, List<int> triangles, ContactAxis axis, Rect area, float cell,
            Vector3 direction)
        {
            var ray = new MeshRay(positions, triangles);
            int columns = Mathf.Max(1, Mathf.CeilToInt(area.width / cell));
            int rows = Mathf.Max(1, Mathf.CeilToInt(area.height / cell));
            var values = new float[columns * rows];
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector2 point = area.min + new Vector2((column + 0.5f) * cell, (row + 0.5f) * cell);
                    Vector3 origin = Space(axis, point) - direction * Far;
                    values[row * columns + column] = ray.TryHit(origin, direction, out float distance)
                        ? Along(axis, origin + direction * distance)
                        : SurfaceMap.Empty;
                }
            }
            return new SurfaceMap(area.min, cell, columns, rows, values);
        }

        /// <summary>Le point dans le plan de la carte : x et z pour le dessus, x et y pour la face et le dos.</summary>
        public static Vector2 Plane(ContactAxis axis, Vector3 point)
        {
            return axis == ContactAxis.Down ? new Vector2(point.x, point.z) : new Vector2(point.x, point.y);
        }

        private static Vector3 Space(ContactAxis axis, Vector2 point)
        {
            return axis == ContactAxis.Down ? new Vector3(point.x, 0f, point.y) : new Vector3(point.x, point.y, 0f);
        }

        private static float Along(ContactAxis axis, Vector3 point)
        {
            return axis == ContactAxis.Down ? point.y : point.z;
        }
    }
}
