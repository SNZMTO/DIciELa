using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Un bruit doux et déterministe posé sur le sol (décision 227) : une valeur de -1 à 1 qui varie sans à-coup, à
    /// l'échelle qu'on lui donne, toujours la même au même point pour la même graine. Ce sont des nombres tirés par
    /// hachage aux nœuds d'un quadrillage (HashRandom.Mix ; SAVOIR, 2.4), lissés de l'un à l'autre. Il casse le bord
    /// d'un sol peint, qui n'a plus l'air tracé au compas.
    /// </summary>
    internal static class GroundNoise
    {
        // Le second axe d'un déplacement tire ses nombres d'une autre graine.
        private const int SecondAxis = 0x5BD1E995;
        // Brasse la graine avant de la mêler au nœud : deux graines voisines, deux bruits sans rapport.
        private const ulong SeedSpread = 0x9E3779B97F4A7C15UL;
        private const uint FnvBasis = 2166136261;
        private const uint FnvPrime = 16777619;
        // Un nombre de 24 bits ramené de -1 à 1.
        private const float UnitOf24Bits = 2f / (1 << 24);

        /// <summary>
        /// Le bruit en ce point, de -1 à 1, à cette échelle : à peu près la distance entre deux bosses.
        /// </summary>
        public static float At(Vector2 point, float scaleMeters, int seed)
        {
            float x = point.x / scaleMeters, y = point.y / scaleMeters;
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float tx = Fade(x - x0), ty = Fade(y - y0);
            float bottom = Mathf.Lerp(Lattice(x0, y0, seed), Lattice(x0 + 1, y0, seed), tx);
            float top = Mathf.Lerp(Lattice(x0, y0 + 1, seed), Lattice(x0 + 1, y0 + 1, seed), tx);
            return Mathf.Lerp(bottom, top, ty);
        }

        /// <summary>Un déplacement de ce point, de -1 à 1 sur chaque axe : deux bruits sans rapport.</summary>
        public static Vector2 Warp(Vector2 point, float scaleMeters, int seed)
        {
            return new Vector2(At(point, scaleMeters, seed), At(point, scaleMeters, seed ^ SecondAxis));
        }

        /// <summary>Une graine tirée d'un texte (l'identifiant d'un sol), la même sur toute machine (FNV-1a).</summary>
        public static int SeedOf(string text)
        {
            uint hash = FnvBasis;
            for (int i = 0; i < text.Length; i++)
                hash = (hash ^ text[i]) * FnvPrime;
            return (int)hash;
        }

        // Le nombre tiré à ce nœud du quadrillage, de -1 à 1.
        private static float Lattice(int x, int y, int seed)
        {
            ulong node = ((ulong)(uint)x << 32) | (uint)y;
            ulong bits = HashRandom.Mix(node ^ ((ulong)(uint)seed * SeedSpread));
            return (bits >> 40) * UnitOf24Bits - 1f;
        }

        // Le lissage d'un nœud à l'autre : sans à-coup, ni dans la pente, ni dans sa courbure.
        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }
    }
}
