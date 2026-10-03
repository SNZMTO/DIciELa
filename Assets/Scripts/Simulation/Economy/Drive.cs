using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Une charrette à conduire, prise par un charretier (ARCHITECTURE, section 7) : celle d'un entrepôt, appelée par une
    /// place de charrette de la Place. La prendre a lié l'une à l'autre ; elle finit garée, ou rendue : la charrette
    /// retourne alors à son entrepôt, et la place en appellera une autre.
    /// </summary>
    public sealed class Drive
    {
        internal Drive(Cart cart, CartSpot spot, Vector2Int stand)
        {
            Cart = cart;
            Spot = spot;
            Stand = stand;
        }

        public Cart Cart { get; }
        public CartSpot Spot { get; }

        /// <summary>La maille où se tient le charretier quand la charrette est garée : devant elle, au bout du timon.</summary>
        public Vector2Int Stand { get; }

        /// <summary>Vrai une fois la charrette attelée : il la tire.</summary>
        public bool IsHitched { get; private set; }

        /// <summary>Vrai une fois garée ou rendue : elle ne lie plus rien.</summary>
        public bool IsOver { get; private set; }

        internal void MarkHitched()
        {
            IsHitched = true;
        }

        internal void MarkOver()
        {
            IsOver = true;
        }
    }
}
