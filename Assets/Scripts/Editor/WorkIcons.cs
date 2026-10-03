using System.Collections.Generic;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les icônes du travail, à l'encre sur 20 unités : les tâches d'un poste (porter, bâtir, défricher, conduire),
    /// l'outil Déplacer (quatre flèches) et l'icône des outils, qui croise le marteau de « bâtir », retourné, avec la
    /// hache de « défricher ». Chaque outil n'est dessiné qu'une fois. La hache, la charrette et les outils sont des
    /// remplaçants simples : la maquette n'en a pas. Le menu des images de l'interface les génère avec les autres
    /// (InterfaceIcons).
    /// </summary>
    internal static class WorkIcons
    {
        private const float Size = 20f;
        private const string Ink = "encre";
        private const string Light = "#FBF3E4";

        public static IEnumerable<InterfaceIcons.Icon> All()
        {
            yield return new InterfaceIcons.Icon("porter", Size, new[]
            {
                F(Ink, 2f, 6f, 6f, 2f, 18f, 2f, 18f, 14f, 14f, 18f, 2f, 18f),
                Faded(0.55f, 3f, 7f, 13f, 7f, 13f, 8f, 3f, 8f),
                Faded(0.4f, 13f, 7f, 17f, 3f, 17f, 4f, 14f, 7f, 14f, 17f, 13f, 17f)
            });
            yield return new InterfaceIcons.Icon("batir", Size, Hammer());
            yield return new InterfaceIcons.Icon("defricher", Size, Axe());
            // La charrette, de profil : sa caisse, son timon levé vers l'avant, sa roue et son moyeu.
            yield return new InterfaceIcons.Icon("conduire", Size, new[]
            {
                F(Ink, 2f, 5f, 14f, 5f, 14f, 11f, 2f, 11f), F(Ink, 13.5f, 8f, 19f, 5.6f, 19.6f, 7f, 13.5f, 9.8f),
                F(Ink, 12f, 14f, 10.8f, 16.8f, 8f, 18f, 5.2f, 16.8f, 4f, 14f, 5.2f, 11.2f, 8f, 10f, 10.8f, 11.2f),
                Faded(0.45f, 3f, 6f, 13f, 6f, 13f, 7f, 3f, 7f), Faded(0.6f, 7f, 13f, 9f, 13f, 9f, 15f, 7f, 15f)
            });
            // Garnir : un panier, son anse et son osier.
            yield return new InterfaceIcons.Icon("garnir", Size, new[]
            {
                F(Ink, 5f, 9f, 6.6f, 9f, 7.2f, 5.2f, 10f, 3.4f, 12.8f, 5.2f, 13.4f, 9f, 15f, 9f, 14.4f, 4.2f, 10f, 1.6f,
                    5.6f, 4.2f),
                F(Ink, 2f, 8f, 18f, 8f, 18f, 10.2f, 2f, 10.2f), F(Ink, 3.2f, 10.2f, 16.8f, 10.2f, 15f, 18f, 5f, 18f),
                Faded(0.45f, 5.5f, 12f, 14.5f, 12f, 14.2f, 13f, 5.8f, 13f),
                Faded(0.45f, 6.2f, 15f, 13.8f, 15f, 13.5f, 16f, 6.5f, 16f)
            });
            // Déplacer : quatre flèches qui partent du milieu.
            yield return new InterfaceIcons.Icon("deplacer", Size, new[]
            {
                F(Ink, 9.1f, 3.5f, 10.9f, 3.5f, 10.9f, 16.5f, 9.1f, 16.5f),
                F(Ink, 3.5f, 9.1f, 16.5f, 9.1f, 16.5f, 10.9f, 3.5f, 10.9f),
                F(Ink, 10f, 0.8f, 13.6f, 4.6f, 6.4f, 4.6f), F(Ink, 10f, 19.2f, 6.4f, 15.4f, 13.6f, 15.4f),
                F(Ink, 0.8f, 10f, 4.6f, 6.4f, 4.6f, 13.6f), F(Ink, 19.2f, 10f, 15.4f, 13.6f, 15.4f, 6.4f),
                F(Ink, 8.2f, 8.2f, 11.8f, 8.2f, 11.8f, 11.8f, 8.2f, 11.8f),
                Faded(0.5f, 9.4f, 9.4f, 10.6f, 9.4f, 10.6f, 10.6f, 9.4f, 10.6f)
            });
            var tools = new List<InterfaceIcons.Face>(Mirrored(Hammer()));
            tools.AddRange(Axe());
            yield return new InterfaceIcons.Icon("outils", Size, tools.ToArray());
        }

        // Le marteau, la tête en haut à droite.
        private static InterfaceIcons.Face[] Hammer()
        {
            return new[]
            {
                F(Ink, 13f, 9f, 11f, 7f, 2f, 16f, 4f, 18f), F(Ink, 11f, 2f, 18f, 9f, 15f, 12f, 8f, 5f),
                Faded(0.45f, 11f, 3f, 16f, 8f, 15f, 9f, 10f, 4f)
            };
        }

        // La hache, au manche aussi épais que celui du marteau.
        private static InterfaceIcons.Face[] Axe()
        {
            return new[]
            {
                F(Ink, 2.5f, 16.5f, 11.6f, 7.4f, 13.6f, 9.4f, 4.5f, 18.5f),
                F(Ink, 9.4f, 3.6f, 12.9f, 1.3f, 18.6f, 6.5f, 17.5f, 11.7f, 14f, 10f, 12.3f, 7.1f),
                Faded(0.45f, 14f, 3.1f, 17.5f, 6.5f, 16.6f, 9.4f, 15.2f, 5.4f)
            };
        }

        // Ces faces retournées de gauche à droite.
        private static InterfaceIcons.Face[] Mirrored(InterfaceIcons.Face[] faces)
        {
            var mirrored = new InterfaceIcons.Face[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                var points = (float[])faces[i].Points.Clone();
                for (int p = 0; p < points.Length; p += 2)
                    points[p] = Size - points[p];
                mirrored[i] = new InterfaceIcons.Face(faces[i].Color, faces[i].Opacity, points);
            }
            return mirrored;
        }

        private static InterfaceIcons.Face F(string color, params float[] points)
        {
            return new InterfaceIcons.Face(color, 1f, points);
        }

        // Un reflet clair, à demi transparent.
        private static InterfaceIcons.Face Faded(float opacity, params float[] points)
        {
            return new InterfaceIcons.Face(Light, opacity, points);
        }
    }
}
