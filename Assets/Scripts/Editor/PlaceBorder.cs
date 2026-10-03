using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les petits cailloux qui entourent la Place ronde (GDD, section 11 ; décisions 193 et 196), en objets du pack : des
    /// rangées de pierres (SM_Bld_Stone_Edge, réduites : des cailloux d'une quinzaine de centimètres) posées bout à bout,
    /// chacune tangente au cercle, à cheval sur le bord du dallage, par tronçons d'environ 1,6 m ; et quelques galets
    /// (SM_Env_Stepping_Stone, réduits) semés juste en dedans. Le tirage est fixe : la même bordure à chaque composition.
    /// Tout tient dans l'empreinte, que la préparation mesure sur la composition.
    /// </summary>
    internal static class PlaceBorder
    {
        private const float Scale = 0.6f;
        // Le milieu des rangées, en dedans du bord de l'empreinte ; un peu levées, pour dépasser des galets.
        private const float Inset = 0.2f;
        private const float Lift = 0.03f;
        private const int Pebbles = 12;
        private const int Seed = 193;

        // Les rangées du pack et leur longueur, mesurée sur le pack (planche de la Place) : 2,67 m, 2,84 m, et la demie, 1,35 m.
        private static readonly string[] Rows = { "Buildings/SM_Bld_Stone_Edge_01", "Buildings/SM_Bld_Stone_Edge_02" };
        private static readonly float[] RowLengths = { 2.67f, 2.84f };
        private const string HalfRow = "Buildings/SM_Bld_Stone_Edge_Half_01";
        private const float HalfLength = 1.35f;

        // Les galets : le gris et le beige du pack (le rouge, le quatrième, jure avec les galets).
        private static readonly string[] Stones =
        {
            "Environment/SM_Env_Stepping_Stone_01", "Environment/SM_Env_Stepping_Stone_02",
            "Environment/SM_Env_Stepping_Stone_03", "Environment/SM_Env_Stepping_Stone_05"
        };

        /// <summary>La bordure d'une Place ronde de ce diamètre, centrée sur son origine.</summary>
        public static PackComposition.Piece[] For(float diameter)
        {
            var random = new Random(Seed);
            var pieces = new List<PackComposition.Piece>();
            float radius = diameter * 0.5f - Inset;
            float around = 2f * Mathf.PI * radius;

            // Les rangées, longues puis demies, tant qu'elles tiennent sur le tour ; le reste se partage en joints.
            var run = new List<float>();
            var paths = new List<string>();
            float length = 0f;
            for (int i = 0; length + RowLengths[i % 2] * Scale <= around; i++)
            {
                run.Add(RowLengths[i % 2] * Scale);
                paths.Add(Rows[i % 2]);
                length += RowLengths[i % 2] * Scale;
            }
            while (length + HalfLength * Scale <= around)
            {
                int at = random.Next(run.Count + 1);
                run.Insert(at, HalfLength * Scale);
                paths.Insert(at, HalfRow);
                length += HalfLength * Scale;
            }
            float joint = (around - length) / run.Count;
            float arc = 0f;
            for (int i = 0; i < run.Count; i++)
            {
                float angle = (arc + run[i] * 0.5f) / radius;
                arc += run[i] + joint;
                // Le long de la rangée, l'axe x du pack, suit la tangente au cercle.
                float yaw = Mathf.Atan2(-Mathf.Cos(angle), -Mathf.Sin(angle)) * Mathf.Rad2Deg +
                            (random.Next(2) == 0 ? 0f : 180f) + Jitter(random, 3f);
                pieces.Add(new PackComposition.Piece(paths[i], Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, yaw,
                    Lift, Scale, centered: true));
            }

            // Quelques galets juste en dedans des rangées, au hasard autour du cercle.
            for (int i = 0; i < Pebbles; i++)
            {
                float angle = (float)random.NextDouble() * 2f * Mathf.PI;
                float r = radius - 0.45f - (float)random.NextDouble() * 0.3f;
                float scale = 0.25f + (float)random.NextDouble() * 0.1f;
                pieces.Add(new PackComposition.Piece(Stones[random.Next(Stones.Length)], Mathf.Cos(angle) * r,
                    Mathf.Sin(angle) * r, random.Next(360), 0f, scale, centered: true));
            }
            return pieces.ToArray();
        }

        // Un écart tiré entre -spread et +spread.
        private static float Jitter(Random random, float spread)
        {
            return ((float)random.NextDouble() * 2f - 1f) * spread;
        }
    }
}
