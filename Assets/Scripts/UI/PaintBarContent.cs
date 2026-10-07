using System;
using System.Collections.Generic;
using System.Globalization;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que la barre montre d'un outil de sol (décisions 206, 210 et 216), traduit : ses modes, dans l'ordre de son
    /// entrée, chacun son nom, son usage et sa touche ; les formes de son mode Forme ; ce qu'il peut poser, la zone d'abord
    /// (l'outil de la Place), puis les sols que la ville sait faire, chacun la texture de son calque pour vignette ; ses
    /// valeurs : pour l'outil de la Place, la superficie et les places de charrette, et pour les deux la taille du
    /// pinceau, au pinceau, à la gomme et au tracé.
    /// </summary>
    internal static class PaintBarContent
    {
        public static List<PaintBarChoice<PaintToolMode>> Modes(PaintTool tool, TextTable texts, IToolKeys keys)
        {
            var modes = new List<PaintBarChoice<PaintToolMode>>();
            IReadOnlyList<PaintToolMode> entry = tool.Entry.Modes;
            for (int i = 0; i < entry.Count; i++)
            {
                string name = entry[i].ToString();
                modes.Add(new PaintBarChoice<PaintToolMode>(entry[i], texts.Get(TextKeys.PaintMode(name)),
                    texts.Get(TextKeys.PaintModeDetail(name)), keys.KeyOfCard(i)));
            }
            return modes;
        }

        /// <summary>Les styles d'un tracé ou d'un polygone, dans leur ordre, chacun son nom et son usage ; sans touche.</summary>
        public static List<PaintBarChoice<PaintTraceStyle>> Styles(TextTable texts)
        {
            var styles = new List<PaintBarChoice<PaintTraceStyle>>();
            foreach (PaintTraceStyle style in (PaintTraceStyle[])Enum.GetValues(typeof(PaintTraceStyle)))
            {
                string name = style.ToString();
                styles.Add(new PaintBarChoice<PaintTraceStyle>(style, texts.Get(TextKeys.PaintStyle(name)),
                    texts.Get(TextKeys.PaintStyleDetail(name)), ""));
            }
            return styles;
        }

        /// <summary>Les formes du mode Forme, dans leur ordre, chacune son nom et son usage ; sans touche.</summary>
        public static List<PaintBarChoice<PaintShape>> Shapes(TextTable texts)
        {
            var shapes = new List<PaintBarChoice<PaintShape>>();
            foreach (PaintShape shape in (PaintShape[])Enum.GetValues(typeof(PaintShape)))
            {
                string name = shape.ToString();
                shapes.Add(new PaintBarChoice<PaintShape>(shape, texts.Get(TextKeys.PaintShape(name)),
                    texts.Get(TextKeys.PaintShapeDetail(name)), ""));
            }
            return shapes;
        }

        public static List<PaintBarInk> Inks(PaintTool tool, TextTable texts)
        {
            var inks = new List<PaintBarInk>();
            if (tool.Entry.LaysTheZone)
                inks.Add(new PaintBarInk(PaintInk.Zone, texts.Get(TextKeys.PlazaZoneInk),
                    texts.Get(TextKeys.PlazaZoneInkDetail), null));
            foreach (FloorDefinition floor in tool.Inks.Known)
            {
                Texture2D picture = floor.Layer != null ? floor.Layer.diffuseTexture : null;
                inks.Add(new PaintBarInk(PaintInk.Of(floor), texts.Get(floor.NameKey), texts.Get(TextKeys.FloorInkDetail),
                    picture));
            }
            return inks;
        }

        /// <summary>
        /// La superficie, les places de charrette et la taille du pinceau ; vides quand l'outil ne les a pas.
        /// </summary>
        public static (string Area, string Marks, string Size) Values(PaintTool tool, PlazaZone plaza, TextTable texts)
        {
            bool zone = tool.Entry.LaysTheZone;
            string area = zone
                ? texts.Get(TextKeys.PlazaArea).Replace(TextKeys.AreaParameter, Whole(plaza.AreaSquareMeters))
                    .Replace(TextKeys.MaxParameter, Whole(plaza.MaxAreaSquareMeters))
                : "";
            string marks = zone
                ? texts.Get(TextKeys.PlazaMarks)
                    .Replace(TextKeys.MarksParameter, plaza.Marks.Count.ToString(CultureInfo.InvariantCulture))
                    .Replace(TextKeys.MaxParameter, plaza.MaxMarks.ToString(CultureInfo.InvariantCulture))
                : "";
            PaintToolMode mode = tool.Mode;
            bool brush = mode == PaintToolMode.Brush || mode == PaintToolMode.Eraser || mode == PaintToolMode.Trace;
            string size = brush
                ? texts.Get(TextKeys.PaintBrushSize).Replace(TextKeys.SizeParameter,
                    (tool.Options.BrushRadius * 2f).ToString("0.#", CultureInfo.InvariantCulture))
                : "";
            return (area, marks, size);
        }

        private static string Whole(float squareMeters)
        {
            return Mathf.RoundToInt(squareMeters).ToString(CultureInfo.InvariantCulture);
        }
    }
}
