using System.Collections.Generic;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les icônes des lignes de l'écran de création (décision 162) : une silhouette crème, lisible sur le bois brun, une
    /// ombre brune pour ses creux ; des remplaçants simples, chacun nommé « ligne- » et l'identifiant de sa ligne
    /// (CreationChoices.RowId). Le menu des images de l'interface les génère avec les autres (InterfaceIcons).
    /// </summary>
    internal static class CreationRowIcons
    {
        private const string Cream = "#F4E2C6";
        private const string Shade = "#5A3417";

        public static IEnumerable<InterfaceIcons.Icon> All()
        {
            yield return new InterfaceIcons.Icon("ligne-tete", 22f, new[]
            {
                F(Cream, 4f, 9f, 5f, 2f, 9f, 6f, 13f, 6f, 17f, 2f, 18f, 9f, 18f, 14f, 14f, 19f, 8f, 19f, 4f, 14f)
            });
            yield return new InterfaceIcons.Icon("ligne-corps", 22f, new[]
            {
                F(Cream, 7f, 3f, 15f, 3f, 19f, 8f, 16f, 10f, 16f, 19f, 6f, 19f, 6f, 10f, 3f, 8f)
            });
            yield return new InterfaceIcons.Icon("ligne-queue", 22f, new[]
            {
                F(Cream, 4f, 18f, 6f, 12f, 10f, 8f, 15f, 5f, 19f, 4f, 18f, 7f, 14f, 9f, 11f, 12f, 8f, 16f, 7f, 19f)
            });
            yield return new InterfaceIcons.Icon("ligne-oreilles", 22f, new[]
            {
                F(Cream, 3f, 18f, 6f, 3f, 10f, 18f), F(Cream, 12f, 18f, 16f, 3f, 19f, 18f)
            });
            yield return new InterfaceIcons.Icon("ligne-cheveux", 22f, new[]
            {
                F(Cream, 3f, 16f, 5f, 8f, 9f, 4f, 14f, 3f, 18f, 6f, 19f, 12f, 16f, 9f, 13f, 12f, 11f, 8f, 9f, 12f, 7f,
                    10f, 6f, 16f)
            });
            yield return new InterfaceIcons.Icon("ligne-chapeau", 22f, new[]
            {
                F(Cream, 2f, 16f, 20f, 16f, 20f, 18f, 2f, 18f), F(Cream, 6f, 16f, 7f, 6f, 15f, 6f, 16f, 16f),
                F(Shade, 7f, 12f, 15f, 12f, 15f, 14f, 7f, 14f)
            });
            yield return new InterfaceIcons.Icon("ligne-visage", 22f, new[]
            {
                F(Cream, 2f, 8f, 9f, 8f, 9f, 13f, 2f, 13f), F(Cream, 13f, 8f, 20f, 8f, 20f, 13f, 13f, 13f),
                F(Cream, 9f, 9f, 13f, 9f, 13f, 10f, 9f, 10f)
            });
            yield return new InterfaceIcons.Icon("ligne-barbe", 22f, new[]
            {
                F(Cream, 4f, 4f, 18f, 4f, 18f, 10f, 15f, 16f, 11f, 20f, 7f, 16f, 4f, 10f),
                F(Shade, 8f, 7f, 14f, 7f, 14f, 9f, 8f, 9f)
            });
            yield return new InterfaceIcons.Icon("ligne-ceinture", 22f, new[]
            {
                F(Cream, 2f, 9f, 20f, 9f, 20f, 14f, 2f, 14f), F(Cream, 8f, 7f, 14f, 7f, 14f, 16f, 8f, 16f),
                F(Shade, 10f, 9f, 12f, 9f, 12f, 14f, 10f, 14f)
            });
        }

        private static InterfaceIcons.Face F(string color, params float[] points)
        {
            return new InterfaceIcons.Face(color, 1f, points);
        }
    }
}
