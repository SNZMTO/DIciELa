using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La matière d'une face (décision 163) : la trame d'un lin, le grain d'un feutre, peints sur la face, sous son
    /// relief, découpés à ses coins. Tout vient de la feuille de style (FacetLook) ; une face sans trame ni grain n'en
    /// peint pas. Peints au redessin seulement (un changement de style), jamais à chaque image.
    /// </summary>
    internal static class FacetMatter
    {
        public static void Draw(Painter2D painter, Rect box, FacetLook look)
        {
            if (look.IsWoven)
            {
                DrawWeave(painter, box, look.Weft, look.WeftStep, true);
                DrawWeave(painter, box, look.Warp, look.WarpStep, false);
            }
            if (look.HasGrain)
                DrawGrain(painter, box, look);
        }

        // La trame d'un lin : un fil d'un pixel tous les tant de pixels, en travers puis en long ; un chemin et un
        // remplissage par sens, quel que soit le nombre de fils.
        private static void DrawWeave(Painter2D painter, Rect box, Color thread, float step, bool across)
        {
            if (thread.a <= 0f || step < 1f)
                return;
            painter.fillColor = thread;
            painter.BeginPath();
            float end = across ? box.yMax : box.xMax;
            for (float at = (across ? box.yMin : box.xMin) + step; at < end; at += step)
            {
                Rect line = across ? new Rect(box.xMin, at, box.width, 1f) : new Rect(at, box.yMin, 1f, box.height);
                Square(painter, line);
            }
            painter.Fill();
        }

        // Le grain d'un feutre : des points d'un pixel, clairs (son reflet) et sombres (sa lèvre), un par tant de pixels
        // carrés, semés au hasard mais toujours aux mêmes places (un hachage de leur rang) : un redessin rend le même
        // feutre. Un chemin et un remplissage par teinte.
        private static void DrawGrain(Painter2D painter, Rect box, FacetLook look)
        {
            int count = Mathf.FloorToInt(box.width * box.height / look.GrainSpacing);
            for (int shade = 0; shade < 2; shade++)
            {
                Color tint = shade == 0 ? look.Shine : look.Lip;
                tint.a = look.Grain;
                painter.fillColor = tint;
                painter.BeginPath();
                for (int i = shade; i < count; i += 2)
                {
                    uint hash = Hash(i);
                    float x = box.xMin + (hash & 0xFFFF) / 65535f * (box.width - 1f);
                    float y = box.yMin + (hash >> 16) / 65535f * (box.height - 1f);
                    Square(painter, new Rect(x, y, 1f, 1f));
                }
                painter.Fill();
            }
        }

        private static void Square(Painter2D painter, Rect square)
        {
            painter.MoveTo(new Vector2(square.xMin, square.yMin));
            painter.LineTo(new Vector2(square.xMax, square.yMin));
            painter.LineTo(new Vector2(square.xMax, square.yMax));
            painter.LineTo(new Vector2(square.xMin, square.yMax));
            painter.ClosePath();
        }

        private static uint Hash(int index)
        {
            unchecked
            {
                uint hash = (uint)index * 2654435761u;
                hash ^= hash >> 15;
                hash *= 0x5bd1e995;
                hash ^= hash >> 13;
                return hash;
            }
        }
    }
}
