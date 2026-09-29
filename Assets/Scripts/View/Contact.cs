using System.Collections.Generic;
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
        /// La coordonnée, sur l'axe du contact (descendre sur le crâne ou reculer contre la face), du pivot d'une pièce
        /// posée par sa base : les points de sa face qui touchaient (à BaseGap près) sa tête de référence, où elle était
        /// posée à referencePivot, restent au contact sur la tête choisie. Sur le crâne, aucun ne flotte : l'un touche, les
        /// autres s'enfoncent, ce qui ne se voit pas. Contre la face, souvent bombée, la moitié touche ou s'enfonce, l'autre
        /// effleure : un museau à dos plat ne s'y enfouit pas. Sur sa tête de référence, elle ne bouge pas. spread : l'écart de ses
        /// moitiés, comme pour TryTouch ; referenceSpread, leur écart sur sa tête de référence. uneven : de combien le point
        /// qui s'enfonce le plus s'enfonce (zéro : la base porte à plat). Faux si aucun de ces points ne se retrouve devant
        /// la tête choisie.
        /// </summary>
        public static bool TryRest(ContactAxis axis, SurfaceMap surface, SurfaceMap reference, SurfaceMap shape,
            Vector3 position, Vector3 referencePivot, Vector3 scale, float spread, float referenceSpread, out float along,
            out float uneven)
        {
            along = 0f;
            uneven = 0f;
            if ((axis != ContactAxis.Down && axis != ContactAxis.Back) || surface == null || surface.IsEmpty ||
                reference == null || reference.IsEmpty || shape == null || shape.IsEmpty || scale.x <= 0f ||
                scale.y <= 0f || scale.z <= 0f)
                return false;

            bool down = axis == ContactAxis.Down;
            var pivot = new Vector2(position.x, down ? position.z : position.y);
            var referencePlane = new Vector2(referencePivot.x, down ? referencePivot.z : referencePivot.y);
            var inPlane = new Vector2(scale.x, down ? scale.z : scale.y);
            float onAxis = down ? scale.y : scale.z;
            float referenceAlong = down ? referencePivot.y : referencePivot.z;
            var needs = new List<float>();
            var gaps = new List<float>();
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
                        float gap = referenceAlong + onAxis * facing - under;
                        if (gap > BaseGap)
                            continue;
                        needs.Add(at - onAxis * facing);
                        gaps.Add(gap);
                    }
                }
            }
            if (needs.Count == 0)
                return false;
            needs.Sort();
            gaps.Sort();
            // Le point le plus bas touche (sur le crâne), ou celui du milieu (contre la face) ; sur la tête de référence, cela
            // redonne la place de la pièce, le point qui touchait le plus loin de la tête.
            along = down ? needs[0] + gaps[gaps.Count - 1] : needs[needs.Count / 2] + gaps[(gaps.Count - 1) / 2];
            uneven = needs[needs.Count - 1] - needs[0];
            return true;
        }
    }
}
