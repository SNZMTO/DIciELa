using Diciela.Data;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Où une pièce touche le personnage le long de l'axe de son emplacement (SAVOIR, 2.11) : on parcourt la carte de la
    /// surface (le dessus du crâne, l'avant de la face, le dos), et, sous chaque case, la carte de la face de la pièce qui
    /// la regarde, à sa taille ; la pièce s'arrête à la première case qui la bloque. Une paire d'oreilles peut être
    /// écartée : chacune de ses moitiés, décalée de son côté. Quelques centaines de cases, sans lire aucun maillage : les
    /// modèles du pack ne sont pas lisibles en jeu.
    /// </summary>
    public static class Contact
    {
        // Un point de la face d'une paire touchait sa tête de référence s'il en était à moins de tant de mètres.
        private const float BaseGap = 0.03f;

        /// <summary>
        /// La coordonnée, sur l'axe du contact, du pivot de la pièce quand elle touche, les deux autres étant celles de
        /// position ; faux si la pièce et la surface ne se font face nulle part. spread : de combien chaque moitié de la
        /// pièce (x positif, x négatif) s'écarte du milieu, en mètres (négatif, elles se rapprochent).
        /// </summary>
        public static bool TryTouch(ContactAxis axis, SurfaceMap surface, SurfaceMap shape, Vector3 position,
            Vector3 scale, out float along, float spread = 0f)
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
                    Vector2 relative = surface.CenterOf(column, row) - pivot;
                    // La moitié droite, écartée de spread, puis la gauche ; sans écart, la pièce entière, une fois.
                    for (int side = 0; side < (spread == 0f ? 1 : 2); side++)
                    {
                        float x = spread == 0f ? relative.x : relative.x - (side == 0 ? spread : -spread);
                        if (spread != 0f && (side == 0 ? x < 0f : x > 0f))
                            continue;
                        float facing = shape.Sample(new Vector2(x, relative.y) / inPlane);
                        if (facing <= SurfaceMap.Empty)
                            continue;
                        // La pièce vient de dessus ou de devant (le plus grand décalage bloque), ou de derrière (le plus
                        // petit).
                        float need = at - onAxis * facing;
                        best = forward ? Mathf.Min(best, need) : Mathf.Max(best, need);
                        touched = true;
                    }
                }
            }
            along = best;
            return touched;
        }

        /// <summary>
        /// La hauteur du pivot d'une paire posée par sa base, en descendant sur le dessus d'une tête : les points de sa face
        /// qui touchaient (à BaseGap près) le dessus de sa tête de référence, où elle était posée à referencePivot, restent
        /// au contact sur la tête choisie. Aucun ne flotte : l'un touche, les autres s'enfoncent ; sur sa tête de
        /// référence, elle ne bouge pas. spread : l'écart de ses moitiés, comme pour TryTouch ; referenceSpread, leur écart
        /// sur sa tête de référence. uneven : de combien le point qui s'enfonce le plus s'enfonce (zéro : la base porte à
        /// plat). Faux si aucun de ces points ne se retrouve au-dessus de la tête choisie.
        /// </summary>
        public static bool TryRest(SurfaceMap surface, SurfaceMap reference, SurfaceMap shape, Vector3 position,
            Vector3 referencePivot, Vector3 scale, float spread, float referenceSpread, out float along, out float uneven)
        {
            along = 0f;
            uneven = 0f;
            if (surface == null || surface.IsEmpty || reference == null || reference.IsEmpty || shape == null ||
                shape.IsEmpty || scale.x <= 0f || scale.y <= 0f || scale.z <= 0f)
                return false;

            var pivot = new Vector2(position.x, position.z);
            var referencePlane = new Vector2(referencePivot.x, referencePivot.z);
            var inPlane = new Vector2(scale.x, scale.z);
            float lowest = float.MaxValue, highest = float.MinValue, deepest = float.MinValue;
            for (int row = 0; row < surface.Rows; row++)
            {
                for (int column = 0; column < surface.Columns; column++)
                {
                    float at = surface.At(column, row);
                    if (at <= SurfaceMap.Empty)
                        continue;
                    Vector2 relative = surface.CenterOf(column, row) - pivot;
                    for (int side = 0; side < (spread == 0f ? 1 : 2); side++)
                    {
                        float x = spread == 0f ? relative.x : relative.x - (side == 0 ? spread : -spread);
                        if (spread != 0f && (side == 0 ? x < 0f : x > 0f))
                            continue;
                        var point = new Vector2(x, relative.y);
                        float facing = shape.Sample(point / inPlane);
                        float under = reference.Sample(referencePlane + point + Vector2.right * (Mathf.Sign(x) * referenceSpread));
                        if (facing <= SurfaceMap.Empty || under <= SurfaceMap.Empty)
                            continue;
                        // Sur sa tête de référence, ce point de la base touchait-il ?
                        float gap = referencePivot.y + scale.y * facing - under;
                        if (gap > BaseGap)
                            continue;
                        lowest = Mathf.Min(lowest, at - scale.y * facing);
                        highest = Mathf.Max(highest, at - scale.y * facing);
                        deepest = Mathf.Max(deepest, gap);
                    }
                }
            }
            if (lowest == float.MaxValue)
                return false;
            along = lowest + deepest;
            uneven = highest - lowest;
            return true;
        }
    }
}
