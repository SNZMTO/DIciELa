using Diciela.Data;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Où une pièce touche le personnage le long de l'axe de son emplacement (SAVOIR, 2.11) : on parcourt la carte de la
    /// surface (le dessus du crâne, l'avant de la face, le dos), et, sous chaque case, la carte de la face de la pièce qui
    /// la regarde, à sa taille ; la pièce s'arrête à la première case qui la bloque. Quelques centaines de cases, sans
    /// lire aucun maillage : les modèles du pack ne sont pas lisibles en jeu.
    /// </summary>
    public static class Contact
    {
        /// <summary>
        /// La coordonnée, sur l'axe du contact, du pivot de la pièce quand elle touche, les deux autres étant celles de
        /// position ; faux si la pièce et la surface ne se font face nulle part.
        /// </summary>
        public static bool TryTouch(ContactAxis axis, SurfaceMap surface, SurfaceMap shape, Vector3 position,
            Vector3 scale, out float along)
        {
            along = 0f;
            if (axis == ContactAxis.None || surface == null || surface.IsEmpty || shape == null || shape.IsEmpty ||
                scale.x <= 0f || scale.y <= 0f || scale.z <= 0f)
                return false;

            bool forward = axis == ContactAxis.Forward;
            bool down = axis == ContactAxis.Down;
            // Dans le plan de la carte : x et z pour le dessus, x et y pour la face et le dos ; la taille de la pièce selon
            // ces deux axes, et selon le troisième, celui du contact.
            var pivot = new Vector2(position.x, down ? position.z : position.y);
            var inPlane = new Vector2(scale.x, down ? scale.z : scale.y);
            float onAxis = down ? scale.y : scale.z;
            float best = forward ? float.MaxValue : float.MinValue;
            bool touched = false;
            for (int row = 0; row < surface.Rows; row++)
            {
                for (int column = 0; column < surface.Columns; column++)
                {
                    float at = surface.At(column, row);
                    if (at <= SurfaceMap.Empty)
                        continue;
                    float facing = shape.Sample((surface.CenterOf(column, row) - pivot) / inPlane);
                    if (facing <= SurfaceMap.Empty)
                        continue;
                    // La pièce vient de dessus ou de devant (le plus grand décalage bloque), ou de derrière (le plus petit).
                    float need = at - onAxis * facing;
                    best = forward ? Mathf.Min(best, need) : Mathf.Max(best, need);
                    touched = true;
                }
            }
            along = best;
            return touched;
        }
    }
}
