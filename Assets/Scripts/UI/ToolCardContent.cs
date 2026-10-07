using System.Collections.Generic;
using System.Globalization;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Les puces de la carte de l'outil en main (décision 221), traduites. Un outil de sol : ce qu'il pose (sauf au
    /// mode place de charrette), la taille du pinceau (au pinceau, à la gomme et au tracé), la forme du mode Forme, le
    /// style d'un tracé ou d'un polygone, la symétrie quand il y en a une (l'étoile avec ses branches), et, pour
    /// l'outil de la Place, sa superficie sur celle de son niveau et ses places de charrette sur celles qu'elle permet.
    /// « Retirer » : combien d'arbres et de rochers sont marqués. Déplacer : la construction qu'on tient, sa photo et
    /// son nom.
    /// </summary>
    internal static class ToolCardContent
    {
        public static void Paint(PaintTool tool, PlazaZone plaza, TextTable texts, List<ToolCardChip> chips)
        {
            chips.Clear();
            PaintToolMode mode = tool.Mode;
            PaintOptions options = tool.Options;
            PaintInk ink = tool.Inks.Chosen;
            if (mode != PaintToolMode.CartSpot && !ink.IsNone)
                chips.Add(ink.IsZone
                    ? new ToolCardChip(PaintIconClasses.Zone, null, texts.Get(TextKeys.PlazaZoneInk))
                    : new ToolCardChip("", PaintIconClasses.Picture(ink.Floor), texts.Get(ink.Floor.NameKey)));
            if (mode == PaintToolMode.Brush || mode == PaintToolMode.Eraser || mode == PaintToolMode.Trace)
                chips.Add(new ToolCardChip("", null, texts.Get(TextKeys.PaintBrushSize).Replace(TextKeys.SizeParameter,
                    (options.BrushRadius * 2f).ToString("0.#", CultureInfo.InvariantCulture))));
            if (mode == PaintToolMode.Shape)
                chips.Add(new ToolCardChip(PaintIconClasses.Shape(options.Shape), null,
                    texts.Get(TextKeys.PaintShape(options.Shape.ToString()))));
            if (mode == PaintToolMode.Trace || mode == PaintToolMode.Shape && options.Shape == PaintShape.Polygon)
                chips.Add(new ToolCardChip(PaintIconClasses.Style(options.Style), null,
                    texts.Get(TextKeys.PaintStyle(options.Style.ToString()))));
            if (options.Symmetry != PaintSymmetry.None)
                chips.Add(new ToolCardChip(PaintIconClasses.Symmetry(options.Symmetry), null,
                    Symmetry(options, texts)));
            if (!tool.Entry.LaysTheZone)
                return;
            chips.Add(new ToolCardChip(PaintIconClasses.Zone, null, texts.Get(TextKeys.PlazaArea)
                .Replace(TextKeys.AreaParameter, Whole(plaza.AreaSquareMeters))
                .Replace(TextKeys.MaxParameter, Whole(plaza.MaxAreaSquareMeters))));
            chips.Add(new ToolCardChip(PaintIconClasses.Mode(PaintToolMode.CartSpot), null,
                texts.Get(TextKeys.PlazaMarks)
                .Replace(TextKeys.MarksParameter, plaza.Marks.Count.ToString(CultureInfo.InvariantCulture))
                .Replace(TextKeys.MaxParameter, plaza.MaxMarks.ToString(CultureInfo.InvariantCulture))));
        }

        public static void Clearing(Nature nature, TextTable texts, List<ToolCardChip> chips)
        {
            chips.Clear();
            chips.Add(new ToolCardChip("", null, texts.Get(TextKeys.CardMarked)
                .Replace(TextKeys.CountParameter, nature.Marked.Count.ToString(CultureInfo.InvariantCulture))));
        }

        /// <summary>La construction qu'on tient ; aucune puce tant qu'on n'en a touché aucune.</summary>
        public static void Move(Structure moving, TextTable texts, List<ToolCardChip> chips)
        {
            chips.Clear();
            if (moving != null)
                chips.Add(new ToolCardChip("", moving.Definition.Picture, texts.Get(moving.Definition.NameKey)));
        }

        private static string Symmetry(PaintOptions options, TextTable texts)
        {
            if (options.Symmetry == PaintSymmetry.Star)
                return texts.Get(TextKeys.CardStar).Replace(TextKeys.BranchesParameter,
                    options.StarBranches.ToString(CultureInfo.InvariantCulture));
            return texts.Get(TextKeys.PaintSymmetry(options.Symmetry.ToString()));
        }

        private static string Whole(float squareMeters)
        {
            return Mathf.RoundToInt(squareMeters).ToString(CultureInfo.InvariantCulture);
        }
    }
}
