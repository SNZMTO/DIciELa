using UnityEngine;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Un cap au sol, en degrés depuis le nord (+z) vers l'est, comme l'angle d'une pose et celui de la caméra : sa
    /// direction, et le cap d'une direction.
    /// </summary>
    public static class Heading
    {
        /// <summary>La direction de ce cap, de longueur 1, sur le plan XZ (y du vecteur = z du monde).</summary>
        public static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
        }

        /// <summary>Le cap de cette direction, de 0 (compris) à 360 (exclu) ; 0 pour une direction nulle.</summary>
        public static float Of(Vector2 direction)
        {
            return Mathf.Repeat(Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg, 360f);
        }
    }
}
