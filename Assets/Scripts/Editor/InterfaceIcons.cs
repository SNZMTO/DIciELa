using System.Collections.Generic;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les icônes de l'interface « fusion », telles que dessinées dans la maquette : des polygones à plat, en facettes
    /// comme le pack. Une couleur est un code (#RRGGBB) ou le nom d'une couleur de la feuille de style (encre). Un
    /// contour (sa largeur, sa couleur) se trace sous toutes les faces.
    /// </summary>
    internal static class InterfaceIcons
    {
        internal sealed class Face
        {
            public readonly string Color;
            public readonly float Opacity;
            public readonly float[] Points;

            public Face(string color, float opacity, params float[] points)
            {
                Color = color;
                Opacity = opacity;
                Points = points;
            }
        }

        internal sealed class Icon
        {
            public readonly string Name;
            public readonly float Size;
            public readonly float OutlineWidth;
            public readonly string OutlineColor;
            public readonly Face[] Outlined;
            public readonly Face[] Faces;

            public Icon(string name, float size, Face[] faces, float outlineWidth = 0f, string outlineColor = null,
                Face[] outlined = null)
            {
                Name = name;
                Size = size;
                Faces = faces;
                OutlineWidth = outlineWidth;
                OutlineColor = outlineColor;
                Outlined = outlined ?? new Face[0];
            }
        }

        private static Face F(string color, params float[] points) => new Face(color, 1f, points);

        public static IEnumerable<Icon> All()
        {
            yield return new Icon("bois", 36f, new[]
            {
                Log(11f, 25f), Log(25f, 25f), Log(18f, 12.5f)
            }.Flatten());
            yield return new Icon("pierre", 36f, new[]
            {
                F("#665A78", 3.7f, 28.9f, 8.1f, 14.6f, 16.9f, 6.9f, 27.9f, 9.1f, 33.4f, 20.1f, 31.2f, 30f, 18f, 33.3f),
                F("#C9C1D2", 5f, 28f, 9f, 15f, 18f, 19f), F("#E6E0EB", 9f, 15f, 17f, 8f, 18f, 19f),
                F("#D7D0DF", 17f, 8f, 27f, 10f, 18f, 19f), F("#B9B0C4", 27f, 10f, 32f, 20f, 18f, 19f),
                F("#9F95AC", 32f, 20f, 30f, 29f, 18f, 19f), F("#887D96", 30f, 29f, 18f, 32f, 18f, 19f),
                F("#AAA0B7", 18f, 32f, 5f, 28f, 18f, 19f)
            });
            yield return new Icon("maison", 36f, new[]
            {
                F("#8E5A30", 2.5f, 18.8f, 18f, 3.5f, 33.5f, 18.8f, 29.5f, 18.8f, 29.5f, 32.5f, 6.5f, 32.5f, 6.5f, 18.8f),
                F("#FFF7E8", 8f, 17f, 28f, 17f, 28f, 31f, 8f, 31f), F("#EAD6B7", 18f, 17f, 28f, 17f, 28f, 31f, 18f, 31f),
                F("#8E5A30", 15f, 22f, 21f, 22f, 21f, 31f, 15f, 31f), F("#E98B7B", 4f, 18f, 18f, 5f, 18f, 18f),
                F("#C9695E", 18f, 5f, 32f, 18f, 18f, 18f)
            });
            yield return new Icon("lit", 22f, new[]
            {
                F("#8E5A30", 1f, 3f, 7f, 3f, 7f, 8f, 21f, 8f, 21f, 20f, 18f, 20f, 18f, 18f, 7f, 18f, 7f, 20f, 1f, 20f),
                F("#B97B43", 2f, 4f, 6f, 4f, 6f, 19f, 2f, 19f), F("#FFF3DF", 7f, 9f, 11f, 9f, 11f, 13f, 7f, 13f),
                F("#EAD6B7", 7f, 13f, 11f, 13f, 11f, 15f, 7f, 15f), F("#F4B9CB", 11f, 9f, 20f, 9f, 20f, 14f, 11f, 14f),
                F("#E59AB0", 11f, 14f, 20f, 14f, 20f, 17f, 11f, 17f), F("#EAD6B7", 7f, 15f, 11f, 15f, 11f, 17f, 7f, 17f)
            });
            yield return Sun();
            foreach (Icon work in WorkIcons.All())
                yield return work;
            foreach (Icon row in CreationRowIcons.All())
                yield return row;
            foreach (Icon paintIcon in PaintIcons.All())
                yield return paintIcon;
            // Les flèches d'un choix : deux chevrons à l'encre.
            yield return new Icon("precedent", 20f, new[] { F("encre", 12.5f, 3.5f, 15f, 6f, 11f, 10f, 15f, 14f, 12.5f, 16.5f, 6f, 10f) });
            yield return new Icon("suivant", 20f, new[] { F("encre", 7.5f, 3.5f, 14f, 10f, 7.5f, 16.5f, 5f, 14f, 9f, 10f, 5f, 6f) });
            yield return new Icon("croix", 36f, new[]
            {
                F("encre", 10f, 12.5f, 12.5f, 10f, 26f, 23.5f, 23.5f, 26f),
                F("encre", 23.5f, 10f, 26f, 12.5f, 12.5f, 26f, 10f, 23.5f)
            });
        }

        // Une bûche vue de bout : l'écorce, le bois, son ombre, le cœur.
        private static Face[] Log(float x, float y)
        {
            return new[]
            {
                F("#8E5A30", Shift(x, y, 7.39f, 3.06f, 3.06f, 7.39f, -3.06f, 7.39f, -7.39f, 3.06f, -7.39f, -3.06f,
                    -3.06f, -7.39f, 3.06f, -7.39f, 7.39f, -3.06f)),
                F("#EDBB7E", Shift(x, y, 5.54f, 2.3f, 2.3f, 5.54f, -2.3f, 5.54f, -5.54f, 2.3f, -5.54f, -2.3f, -2.3f,
                    -5.54f, 2.3f, -5.54f, 5.54f, -2.3f)),
                F("#DDA468", Shift(x, y, 5.54f, -2.3f, 5.54f, 2.3f, 2.3f, 5.54f, -2.3f, 5.54f, 0f, 0f)),
                F("#B97B43", Shift(x, y, 2.03f, 0.84f, 0.84f, 2.03f, -0.84f, 2.03f, -2.03f, 0.84f, -2.03f, -0.84f,
                    -0.84f, -2.03f, 0.84f, -2.03f, 2.03f, -0.84f))
            };
        }

        private static Icon Sun()
        {
            Face[] rays =
            {
                F("#D9912E", 34f, 18f, 26.5f, 15.2f, 26.5f, 20.8f), F("#D9912E", 18f, 34f, 15.2f, 26.5f, 20.8f, 26.5f),
                F("#E9A23B", 2f, 18f, 9.5f, 15.2f, 9.5f, 20.8f), F("#E9A23B", 18f, 2f, 15.2f, 9.5f, 20.8f, 9.5f),
                F("#D9912E", 29.3f, 29.3f, 22.3f, 26.3f, 26.3f, 22.3f), F("#E9A23B", 6.7f, 29.3f, 9.7f, 22.3f, 13.7f, 26.3f),
                F("#E9A23B", 6.7f, 6.7f, 13.7f, 9.7f, 9.7f, 13.7f), F("#D9912E", 29.3f, 6.7f, 26.3f, 13.7f, 22.3f, 9.7f)
            };
            Face core = F("#C77F2A", 25.7f, 21.2f, 21.2f, 25.7f, 14.8f, 25.7f, 10.3f, 21.2f, 10.3f, 14.8f, 14.8f, 10.3f,
                21.2f, 10.3f, 25.7f, 14.8f);
            var faces = new List<Face>(rays)
            {
                core,
                F("#F8D46A", 24.47f, 20.68f, 20.68f, 24.47f, 15.32f, 24.47f, 11.53f, 20.68f, 11.53f, 15.32f, 15.32f,
                    11.53f, 20.68f, 11.53f, 24.47f, 15.32f),
                F("#FFE7A0", 11.53f, 20.68f, 11.53f, 15.32f, 15.32f, 11.53f, 20.68f, 11.53f, 18f, 18f)
            };
            var outlined = new List<Face>(rays) { core };
            return new Icon("soleil", 36f, faces.ToArray(), 3f, "#8E5A30", outlined.ToArray());
        }

        private static float[] Shift(float x, float y, params float[] offsets)
        {
            var points = new float[offsets.Length];
            for (int i = 0; i < offsets.Length; i += 2)
            {
                points[i] = x + offsets[i];
                points[i + 1] = y + offsets[i + 1];
            }
            return points;
        }

        private static Face[] Flatten(this Face[][] groups)
        {
            var all = new List<Face>();
            foreach (Face[] group in groups)
                all.AddRange(group);
            return all.ToArray();
        }
    }
}
