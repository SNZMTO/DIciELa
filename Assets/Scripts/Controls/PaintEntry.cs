using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Controls
{
    /// <summary>
    /// L'entrée d'un outil de sol (décisions 210 et 216) : un seul outil, deux entrées. L'outil de la Place pose la
    /// zone, ou un sol dans la zone, et a cinq modes (pinceau, gomme, tracé, forme, place de charrette) ; l'outil
    /// Chemins pose un sol partout où l'on marche, et a les quatre premiers. Chacune dit ses modes, dans l'ordre de
    /// leurs cases dans la roue et de leurs chiffres, si elle pose la zone, où vont ses sols, et la taille de son
    /// pinceau au départ.
    /// </summary>
    public sealed class PaintEntry
    {
        /// <summary>L'outil de la Place, pris à sa carte.</summary>
        public static readonly PaintEntry Plaza = new PaintEntry(FloorScope.Plaza, true,
            settings => settings.BrushRadiusMeters, PaintToolMode.Brush, PaintToolMode.Eraser, PaintToolMode.Trace,
            PaintToolMode.Shape, PaintToolMode.CartSpot);

        /// <summary>L'outil Chemins, pris à la roue (touche C).</summary>
        public static readonly PaintEntry Paths = new PaintEntry(FloorScope.Anywhere, false,
            settings => settings.PathBrushRadiusMeters, PaintToolMode.Brush, PaintToolMode.Eraser, PaintToolMode.Trace,
            PaintToolMode.Shape);

        private readonly Func<PlacementSettings, float> _startRadius;

        private PaintEntry(FloorScope scope, bool laysTheZone, Func<PlacementSettings, float> startRadius,
            params PaintToolMode[] modes)
        {
            Scope = scope;
            LaysTheZone = laysTheZone;
            _startRadius = startRadius;
            Modes = modes;
        }

        /// <summary>Où ses sols se tracent et s'effacent : partout, ou dans la zone de la Place.</summary>
        public FloorScope Scope { get; }

        /// <summary>Vrai si elle pose aussi la zone de la Place.</summary>
        public bool LaysTheZone { get; }

        /// <summary>Ses modes, dans l'ordre de leurs cases dans la roue et de leurs chiffres, 1 à 5.</summary>
        public IReadOnlyList<PaintToolMode> Modes { get; }

        /// <summary>Le rayon du pinceau quand on prend l'outil, d'après ces réglages, en mètres.</summary>
        public float StartBrushRadius(PlacementSettings settings)
        {
            return _startRadius(settings);
        }

        public bool Allows(PaintToolMode mode)
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
