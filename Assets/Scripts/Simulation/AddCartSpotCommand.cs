using UnityEngine;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « poser une place de charrette » (GDD, section 11 ; décision 204) : une marque sur la Place, à ce milieu,
    /// l'avant de la charrette tourné vers ce cap. Il se vérifie par la même fonction que l'aperçu
    /// (PlazaZone.CheckMark) ; posée, elle appelle une charrette.
    /// </summary>
    public sealed class AddCartSpotCommand : Command
    {
        public AddCartSpotCommand(Vector2 position, float headingDegrees)
        {
            Position = position;
            HeadingDegrees = headingDegrees;
        }

        public Vector2 Position { get; }
        public float HeadingDegrees { get; }

        internal override void Execute(World world)
        {
            world.Plaza.AddMark(Position, HeadingDegrees);
        }
    }
}
