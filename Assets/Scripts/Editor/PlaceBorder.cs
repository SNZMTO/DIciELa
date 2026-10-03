using System;
using System.Collections.Generic;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les petits cailloux qui bordent la Place (GDD, section 11 ; décision 193), en objets du pack : sur chaque côté,
    /// des rangées de pierres (SM_Bld_Stone_Edge, réduites : des cailloux d'une quinzaine de centimètres) posées bout à
    /// bout un peu en dedans du bord, par tronçons d'environ 1,6 m ; à chaque coin, deux galets (SM_Env_Stepping_Stone,
    /// réduits) ; et quelques galets semés juste en dedans des rangées. Le tirage est fixe : la même bordure à chaque
    /// composition. Tout tient dans l'empreinte, que la préparation mesure sur la composition.
    /// </summary>
    internal static class PlaceBorder
    {
        private const float Scale = 0.6f;
        // Le milieu des rangées, en dedans du bord ; un peu levées, pour dépasser du dallage d'une dizaine de centimètres.
        private const float Inset = 0.2f;
        private const float Lift = 0.03f;
        // La place laissée à chaque coin, en mètres, le long de chaque côté.
        private const float CornerRoom = 0.5f;
        private const int Seed = 193;

        // Les rangées du pack et leur longueur, mesurée sur le pack (planche de la Place) : 2,67 m, 2,84 m, et la demie, 1,35 m.
        private static readonly string[] Rows = { "Buildings/SM_Bld_Stone_Edge_01", "Buildings/SM_Bld_Stone_Edge_02" };
        private static readonly float[] RowLengths = { 2.67f, 2.84f };
        private const string HalfRow = "Buildings/SM_Bld_Stone_Edge_Half_01";
        private const float HalfLength = 1.35f;

        // Les galets : le gris et le beige du pack (le rouge, le quatrième, jure avec le dallage).
        private static readonly string[] Pebbles =
        {
            "Environment/SM_Env_Stepping_Stone_01", "Environment/SM_Env_Stepping_Stone_02",
            "Environment/SM_Env_Stepping_Stone_03", "Environment/SM_Env_Stepping_Stone_05"
        };

        /// <summary>La bordure d'une Place de size mètres de côté, centrée sur son origine.</summary>
        public static PackComposition.Piece[] For(float size)
        {
            var random = new Random(Seed);
            var pieces = new List<PackComposition.Piece>();
            float half = size * 0.5f;
            for (int side = 0; side < 4; side++)
                AddSide(side, half, random, pieces);
            for (int corner = 0; corner < 4; corner++)
            {
                float x = corner == 0 || corner == 3 ? -1f : 1f;
                float z = corner < 2 ? -1f : 1f;
                pieces.Add(Pebble(random, x * (half - 0.35f), z * (half - 0.3f), 0.45f));
                pieces.Add(Pebble(random, x * (half - 0.7f), z * (half - 0.25f), 0.35f));
            }
            return pieces.ToArray();
        }

        // Un côté : 0 au sud, puis l'est, le nord, l'ouest. Les rangées, longues et demies, tiennent entre les coins.
        private static void AddSide(int side, float half, Random random, List<PackComposition.Piece> pieces)
        {
            float room = 2f * (half - CornerRoom);
            var run = new List<int>();
            float length = 0f;
            for (int i = 0; length + RowLengths[i % 2] * Scale <= room; i++)
            {
                run.Add(i % 2);
                length += RowLengths[i % 2] * Scale;
            }
            while (length + HalfLength * Scale <= room)
            {
                run.Insert(random.Next(run.Count + 1), -1);
                length += HalfLength * Scale;
            }

            bool alongX = side % 2 == 0;
            float across = (side == 0 || side == 3 ? -1f : 1f) * (half - Inset);
            float at = -length * 0.5f;
            foreach (int row in run)
            {
                float piece = (row < 0 ? HalfLength : RowLengths[row]) * Scale;
                float middle = at + piece * 0.5f;
                at += piece;
                float yaw = (alongX ? 0f : 90f) + (random.Next(2) == 0 ? 0f : 180f) + Jitter(random, 3f);
                string path = row < 0 ? HalfRow : Rows[row];
                pieces.Add(alongX
                    ? new PackComposition.Piece(path, middle, across, yaw, Lift, Scale, centered: true)
                    : new PackComposition.Piece(path, across, middle, yaw, Lift, Scale, centered: true));
            }

            // Trois galets juste en dedans des rangées, au hasard le long du côté.
            for (int i = 0; i < 3; i++)
            {
                float along = Jitter(random, half - 1.2f);
                float inside = (half - Inset - 0.45f - (float)random.NextDouble() * 0.3f) * Math.Sign(across);
                float scale = 0.25f + (float)random.NextDouble() * 0.1f;
                pieces.Add(alongX ? Pebble(random, along, inside, scale) : Pebble(random, inside, along, scale));
            }
        }

        private static PackComposition.Piece Pebble(Random random, float x, float z, float scale)
        {
            return new PackComposition.Piece(Pebbles[random.Next(Pebbles.Length)], x, z, random.Next(360), 0f, scale,
                centered: true);
        }

        // Un écart tiré entre -spread et +spread.
        private static float Jitter(Random random, float spread)
        {
            return ((float)random.NextDouble() * 2f - 1f) * spread;
        }
    }
}
