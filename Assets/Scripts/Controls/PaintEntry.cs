using System.Collections.Generic;
using Diciela.Simulation.Land;

namespace Diciela.Controls
{
    /// <summary>
    /// L'entrée d'un outil de sol (décision 210) : un seul outil, deux entrées. L'outil de la Place pose la zone, ou un sol
    /// dans la zone, et a cinq modes (pinceau, gomme, cercle, rectangle, place de charrette) ; l'outil Chemins pose un sol
    /// partout où l'on marche, et a quatre modes. Chacune dit ses modes, dans l'ordre de la barre et des touches, si elle
    /// pose la zone, et où vont ses sols.
    /// </summary>
    public sealed class PaintEntry
    {
        /// <summary>L'outil de la Place, pris à sa carte.</summary>
        public static readonly PaintEntry Place = new PaintEntry(FloorScope.Place, true, PlaceToolMode.Brush,
            PlaceToolMode.Eraser, PlaceToolMode.Disc, PlaceToolMode.Box, PlaceToolMode.CartSpot);

        /// <summary>L'outil Chemins, pris à la roue (touche C).</summary>
        public static readonly PaintEntry Paths = new PaintEntry(FloorScope.Anywhere, false, PlaceToolMode.Brush,
            PlaceToolMode.Eraser, PlaceToolMode.Disc, PlaceToolMode.Box);

        private PaintEntry(FloorScope scope, bool laysTheZone, params PlaceToolMode[] modes)
        {
            Scope = scope;
            LaysTheZone = laysTheZone;
            Modes = modes;
        }

        /// <summary>Où ses sols se tracent et s'effacent : partout, ou dans la zone de la Place.</summary>
        public FloorScope Scope { get; }

        /// <summary>Vrai si elle pose aussi la zone de la Place.</summary>
        public bool LaysTheZone { get; }

        /// <summary>Ses modes, dans l'ordre de la barre et des touches 1 à 5.</summary>
        public IReadOnlyList<PlaceToolMode> Modes { get; }

        public bool Allows(PlaceToolMode mode)
        {
            for (int i = 0; i < Modes.Count; i++)
            {
                if (Modes[i] == mode)
                    return true;
            }
            return false;
        }
    }
}
