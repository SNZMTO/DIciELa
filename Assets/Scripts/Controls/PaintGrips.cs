using Diciela.Data;
using Diciela.Simulation;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Ce que le joueur prend et glisse avec un outil de sol (décision 216) : un point posé d'un geste pointé
    /// (PaintPoints), ou l'épingle de la symétrie (PaintPin) ; un point posé passe devant l'épingle. Un appui pris ici ne
    /// commence aucun trait.
    /// </summary>
    internal sealed class PaintGrips
    {
        public PaintGrips(PaintDrawing drawing, World world, Camera camera, PlacementSettings settings)
        {
            var handles = new PaintHandles(camera, world.Map.Relief, settings);
            Points = new PaintPoints(drawing, handles);
            Pin = new PaintPin(world, camera, handles, settings);
        }

        /// <summary>Les points posés du geste pointé, et ses clics.</summary>
        public PaintPoints Points { get; }

        /// <summary>L'épingle de la symétrie.</summary>
        public PaintPin Pin { get; }

        /// <summary>L'appui à ce point de l'écran : vrai s'il prend un point posé ou l'épingle.</summary>
        public bool Press(Vector2 screen, PaintOptions options)
        {
            return Points.Press(screen) || Pin.Press(screen, options);
        }

        /// <summary>Le pointeur est à ce point du sol : ce qui est tenu le suit ; vrai si quelque chose était tenu.</summary>
        public bool Drag(Vector2 ground, PaintOptions options)
        {
            return Points.Drag(ground) || Pin.Drag(ground, options);
        }

        /// <summary>Le relâchement, ou l'abandon : vrai si quelque chose était tenu, qu'il lâche.</summary>
        public bool Release()
        {
            return Points.Release() | Pin.Release();
        }
    }
}
