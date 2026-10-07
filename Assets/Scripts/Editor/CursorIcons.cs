using System.Collections.Generic;
using static Diciela.EditorTools.InkStrokes;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les silhouettes des curseurs des outils (décision 221) : les dessins à l'encre de leurs outils, sur 20 unités,
    /// posés dans une boîte de 24 qui laisse la place à leur halo blanc, que la couleur de l'outil teinte en jeu. Le
    /// marteau (Construire) et la hache (Retirer) du travail ; le pinceau, la gomme et la place de charrette des outils de
    /// sol ; la main (Déplacer), la plume (le tracé) et le viseur (la forme), dessinés ici. Des remplaçants simples. Le
    /// menu des images de l'interface les génère avec les autres (InterfaceIcons) ; leur pointe, là où l'outil agit, est
    /// dans la table des curseurs de l'interface (ToolCursorShapes).
    /// </summary>
    internal static class CursorIcons
    {
        private const float Size = 24f;
        private const float Margin = 2f;
        private const float Halo = 3f;
        private const string HaloColor = "#FFFFFF";

        public static IEnumerable<InterfaceIcons.Icon> All()
        {
            yield return Cursor("curseur-marteau", WorkIcons.Hammer());
            yield return Cursor("curseur-hache", WorkIcons.Axe());
            yield return Cursor("curseur-main", Hand());
            yield return Cursor("curseur-pinceau", PaintIcons.Brush());
            yield return Cursor("curseur-gomme", PaintIcons.Eraser());
            yield return Cursor("curseur-plume", Quill());
            yield return Cursor("curseur-viseur", Crosshair());
            yield return Cursor("curseur-place-charrette", PaintIcons.Spot());
        }

        // Ces faces, décalées de la marge, cerclées de leur halo.
        private static InterfaceIcons.Icon Cursor(string name, InterfaceIcons.Face[] faces)
        {
            var shifted = new InterfaceIcons.Face[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                var points = (float[])faces[i].Points.Clone();
                for (int p = 0; p < points.Length; p++)
                    points[p] += Margin;
                shifted[i] = new InterfaceIcons.Face(faces[i].Color, faces[i].Opacity, points);
            }
            return new InterfaceIcons.Icon(name, Size, shifted, Halo, HaloColor, shifted);
        }

        // La main ouverte : la paume, quatre doigts levés, le pouce écarté à gauche.
        private static InterfaceIcons.Face[] Hand()
        {
            return new[]
            {
                Polygon(Ink, 1f, 5f, 10f, 15.6f, 10f, 15.6f, 15.5f, 13.5f, 18.8f, 7f, 18.8f, 5f, 15.5f),
                Bar(6.6f, 10f, 6.6f, 5f, 2.3f), Bar(9.3f, 10f, 9.3f, 3.2f, 2.3f), Bar(12f, 10f, 12f, 3.6f, 2.3f),
                Bar(14.5f, 10.5f, 14.5f, 5.8f, 2.1f), Bar(5.8f, 14.5f, 2.8f, 10.8f, 2.3f)
            };
        }

        // La plume, sa pointe en bas à gauche : la barbe, le tuyau, le bec.
        private static InterfaceIcons.Face[] Quill()
        {
            return new[]
            {
                Polygon(Ink, 0.55f, 18f, 2f, 13f, 3.5f, 7.5f, 9f, 6f, 12.5f, 9.5f, 11f, 15f, 7.5f),
                Bar(17.5f, 2.5f, 5.5f, 14.5f, 1.2f), Polygon(Ink, 1f, 3f, 17f, 4.8f, 13.2f, 6.8f, 15.2f)
            };
        }

        // Le viseur : un anneau, quatre traits vers son milieu, un point au milieu.
        private static InterfaceIcons.Face[] Crosshair()
        {
            var faces = new List<InterfaceIcons.Face>(Loop(Circle(10f, 10f, 6f, 20), 1.6f))
            {
                Bar(10f, 1.5f, 10f, 5f, 1.6f), Bar(10f, 15f, 10f, 18.5f, 1.6f), Bar(1.5f, 10f, 5f, 10f, 1.6f),
                Bar(15f, 10f, 18.5f, 10f, 1.6f), Polygon(Ink, 1f, Circle(10f, 10f, 1.3f, 8))
            };
            return faces.ToArray();
        }
    }
}
