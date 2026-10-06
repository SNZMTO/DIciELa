using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Une marque de la Place (décision 204) : une place de charrette que le joueur pose dans la zone, en mode
    /// construction. Là où se gare la charrette qu'elle appelle, l'avant ainsi tourné, et la place qu'elle prend : celle
    /// de la plus grande charrette du pack. On passe à travers : elle ne prend pas de maille au maillage. Ce qu'elle
    /// appelle et ce qu'elle demande, l'économie le tient (une place de charrette, <c>CartSpot</c>).
    /// </summary>
    public sealed class PlaceMark
    {
        internal PlaceMark(int id, RotatedRect footprint, Vector2 position)
        {
            Id = id;
            Footprint = footprint;
            Position = position;
        }

        /// <summary>Son numéro, à partir de 1, dans l'ordre où les marques ont été posées : celui des ordres.</summary>
        public int Id { get; }

        /// <summary>La place qu'elle prend, telle que le joueur la voit : son milieu, sa largeur, sa longueur, son cap.</summary>
        public RotatedRect Footprint { get; }

        /// <summary>
        /// Où se gare la charrette, au sol, en mètres : son avant au bord avant de la marque, son timon tenu au bout,
        /// comme la vue la pose.
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>Vers où pointe l'avant de la charrette garée, son timon, en degrés depuis le nord vers l'est.</summary>
        public float HeadingDegrees => Footprint.RotationDegrees;
    }
}
