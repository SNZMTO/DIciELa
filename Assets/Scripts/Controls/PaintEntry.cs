using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Controls
{
    /// <summary>
    /// L'entrée d'un outil de sol (décision 210) : un seul outil, deux entrées. L'outil de la Place pose la zone, ou un sol
    /// dans la zone, et a cinq modes (pinceau, gomme, cercle, rectangle, place de charrette) ; l'outil Chemins pose un sol
    /// partout où l'on marche, et a quatre modes. Chacune dit ses modes, dans l'ordre de la barre et des touches, si elle
    /// pose la zone, où vont ses sols, et la taille de son pinceau au départ.
    /// </summary>
    public sealed class PaintEntry
    {
        /// <summary>L'outil de la Place, pris à sa carte.</summary>
        public static readonly PaintEntry Place = new PaintEntry(FloorScope.Place, true,
            settings => settings.BrushRadiusMeters, PlaceToolMode.Brush, PlaceToolMode.Eraser, PlaceToolMode.Disc,
            PlaceToolMode.Box, PlaceToolMode.CartSpot);

        /// <summary>L'outil Chemins, pris à la roue (touche C).</summary>
        public static readonly PaintEntry Paths = new PaintEntry(FloorScope.Anywhere, false,
            settings => settings.PathBrushRadiusMeters, PlaceToolMode.Brush, PlaceToolMode.Eraser, PlaceToolMode.Disc,
            PlaceToolMode.Box);

        private readonly Func<PlacementSettings, float> _startRadius;

        private PaintEntry(FloorScope scope, bool laysTheZone, Func<PlacementSettings, float> startRadius,
            params PlaceToolMode[] modes)
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

        /// <summary>Ses modes, dans l'ordre de la barre et des touches 1 à 5.</summary>
        public IReadOnlyList<PlaceToolMode> Modes { get; }

        /// <summary>Le rayon du pinceau quand on prend l'outil, d'après ces réglages, en mètres.</summary>
        public float StartBrushRadius(PlacementSettings settings)
        {
            return _startRadius(settings);
        }

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
