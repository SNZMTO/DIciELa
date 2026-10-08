using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les carrés d'un champ lissé (CellField ; décision 227), pour la méthode des « carrés qui marchent » (marching
    /// squares) : chaque carré est entre quatre coins, comptés dans le sens des aiguilles d'une montre vu d'en haut (0
    /// en bas à gauche, 1 en haut à gauche, 2 en haut à droite, 3 en bas à droite), son côté k allant du coin k au
    /// suivant. Un coin est dedans quand le champ y atteint le niveau du contour ; un côté entre un coin dedans et un
    /// coin dehors est coupé là où le champ passe ce niveau. Coins et côtés ont chacun leur clé, la même pour les
    /// carrés qui les partagent : ContourMesh y pose ses sommets, ContourLines y chaîne ses traits.
    /// </summary>
    internal static class ContourSquares
    {
        // Trois clés par coin : le coin, son côté vers la droite, son côté vers le haut.
        private const int KeysPerCorner = 3;
        private const int RightSide = 1;
        private const int UpSide = 2;

        /// <summary>Combien de coins de ce carré sont dedans, notés dans inside, rangé comme les coins.</summary>
        public static int Classify(CellField field, int i, int j, bool[] inside)
        {
            int count = 0;
            for (int corner = 0; corner < 4; corner++)
            {
                Vector2Int at = CornerAt(i, j, corner);
                inside[corner] = field.Value(at.x, at.y) >= CellField.Edge;
                count += inside[corner] ? 1 : 0;
            }
            return count;
        }

        /// <summary>Vrai pour une selle : deux coins dedans, en diagonale.</summary>
        public static bool IsSaddle(int count, bool[] inside)
        {
            return count == 2 && inside[0] == inside[2];
        }

        /// <summary>Le milieu d'un carré est dedans si la moyenne de ses coins l'est : une selle s'y relie.</summary>
        public static bool CenterInside(CellField field, int i, int j)
        {
            float sum = field.Value(i, j) + field.Value(i, j + 1) + field.Value(i + 1, j + 1) + field.Value(i + 1, j);
            float mean = sum * 0.25f;
            return mean >= CellField.Edge;
        }

        /// <summary>La clé de ce coin du carré, et sa place sur le sol.</summary>
        public static long CornerKey(CellField field, int i, int j, int corner, out Vector2 position)
        {
            Vector2Int at = CornerAt(i, j, corner);
            position = field.Corner(at.x, at.y);
            return Key(field, at.x, at.y, 0);
        }

        /// <summary>La clé du côté k de ce carré : celle de son coin le plus bas à gauche, et son sens.</summary>
        public static long SideKey(CellField field, int i, int j, int side)
        {
            Vector2Int from = CornerAt(i, j, side), to = CornerAt(i, j, (side + 1) % 4);
            Vector2Int low = Vector2Int.Min(from, to);
            return Key(field, low.x, low.y, from.x == to.x ? UpSide : RightSide);
        }

        /// <summary>
        /// Le point du côté k de ce carré où le champ passe le niveau du contour, entre ses deux coins.
        /// </summary>
        public static Vector2 SidePoint(CellField field, int i, int j, int side)
        {
            Vector2Int from = CornerAt(i, j, side), to = CornerAt(i, j, (side + 1) % 4);
            float a = field.Value(from.x, from.y), b = field.Value(to.x, to.y);
            float t = Mathf.Abs(b - a) < 1e-6f ? 0.5f : Mathf.Clamp01((CellField.Edge - a) / (b - a));
            return Vector2.Lerp(field.Corner(from.x, from.y), field.Corner(to.x, to.y), t);
        }

        private static Vector2Int CornerAt(int i, int j, int corner)
        {
            return corner switch
            {
                0 => new Vector2Int(i, j),
                1 => new Vector2Int(i, j + 1),
                2 => new Vector2Int(i + 1, j + 1),
                _ => new Vector2Int(i + 1, j)
            };
        }

        private static long Key(CellField field, int i, int j, int kind)
        {
            return ((long)j * field.CornersX + i) * KeysPerCorner + kind;
        }
    }
}
