using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Un arbre ou un rocher de la carte (GDD, section 2) : sa sorte, sa place, la place que prend son pied au sol, et son
    /// état : debout, marqué pour être retiré, retiré. Debout, il prend ses mailles : on ne le traverse pas et rien ne se
    /// pose dessus. Il porte sur le maillage l'opposé de son numéro, pour ne jamais se confondre avec une construction.
    /// </summary>
    public sealed class NatureItem
    {
        internal NatureItem(int id, NatureSpot spot)
        {
            Id = id;
            Definition = spot.Definition;
            Placement = spot.Placement;
            Scale = spot.Scale;
            Footprint = new RotatedRect(spot.Placement.Center,
                new Vector2(spot.Definition.WidthMeters, spot.Definition.DepthMeters) * spot.Scale,
                spot.Placement.RotationDegrees);
        }

        /// <summary>Son numéro dans ce monde, à partir de 1, dans l'ordre de la carte.</summary>
        public int Id { get; }

        public NatureDefinition Definition { get; }

        /// <summary>Sa place (le pied) et son angle.</summary>
        public Placement Placement { get; }

        /// <summary>Sa taille, en fois celle de sa fiche.</summary>
        public float Scale { get; }

        /// <summary>La place que prend son pied au sol.</summary>
        public RotatedRect Footprint { get; }

        /// <summary>Ce que porte le maillage sous lui : l'opposé de son numéro, toujours négatif.</summary>
        public int Occupant => -Id;

        /// <summary>Vrai s'il est marqué pour être retiré, et pas encore retiré.</summary>
        public bool IsMarked { get; private set; }

        /// <summary>Vrai une fois retiré : il a quitté la carte, ses mailles sont libres.</summary>
        public bool IsRemoved { get; private set; }

        internal void SetMarked(bool marked)
        {
            IsMarked = marked;
        }

        internal void BecomeRemoved()
        {
            IsRemoved = true;
            IsMarked = false;
        }
    }
}
