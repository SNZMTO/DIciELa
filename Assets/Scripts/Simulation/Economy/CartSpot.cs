using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Une place de charrette de la Place (GDD, section 11) : là où se gare la charrette qu'elle appelle, et son cap, tirés
    /// au hasard quand la Place est bâtie (CartSpotLayout), et qui la suivent quand on la déplace. Quelle charrette elle a
    /// appelée, la charrette le dit (<see cref="Cart.Spot"/>).
    /// </summary>
    public sealed class CartSpot
    {
        internal CartSpot(Structure place, int index, Vector2 position, float headingDegrees)
        {
            Place = place;
            Index = index;
            Position = position;
            HeadingDegrees = headingDegrees;
        }

        /// <summary>La Place qui l'offre.</summary>
        public Structure Place { get; }

        /// <summary>Son rang sur la Place, à partir de 0.</summary>
        public int Index { get; }

        /// <summary>Le milieu de la charrette garée, au sol, en mètres.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Vers où pointe l'avant de la charrette garée, son timon, en degrés depuis le nord vers l'est.</summary>
        public float HeadingDegrees { get; private set; }

        /// <summary>Sa Place a changé de place : elle suit.</summary>
        internal void MoveTo(Vector2 position, float headingDegrees)
        {
            Position = position;
            HeadingDegrees = headingDegrees;
        }
    }
}
