using System.Collections.Generic;
using System.Globalization;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que la barre montre d'un outil de sol (décisions 206 et 210), traduit : ses modes, dans l'ordre de son entrée,
    /// chacun son nom, son usage et sa touche ; ce qu'il peut poser, la zone d'abord (l'outil de la Place), puis les sols
    /// que la ville sait faire, chacun la texture de son calque pour vignette ; ses valeurs : pour l'outil de la Place, la
    /// superficie et les places de charrette, et pour les deux la taille du pinceau, qu'au pinceau et à la gomme.
    /// </summary>
    internal static class PlaceBarContent
    {
        public static List<PlaceBarMode> Modes(PlaceTool tool, TextTable texts, IToolKeys keys)
        {
            var modes = new List<PlaceBarMode>();
            IReadOnlyList<PlaceToolMode> entry = tool.Entry.Modes;
            for (int i = 0; i < entry.Count; i++)
            {
                string name = entry[i].ToString();
                modes.Add(new PlaceBarMode(entry[i], texts.Get(TextKeys.PlaceMode(name)),
                    texts.Get(TextKeys.PlaceModeDetail(name)), keys.KeyOfCard(i)));
            }
            return modes;
        }

        public static List<PlaceBarInk> Inks(PlaceTool tool, TextTable texts)
        {
            var inks = new List<PlaceBarInk>();
            if (tool.Entry.LaysTheZone)
                inks.Add(new PlaceBarInk(null, texts.Get(TextKeys.PlaceZoneInk), texts.Get(TextKeys.PlaceZoneInkDetail),
                    null));
            foreach (FloorDefinition floor in tool.Inks.Known)
            {
                Texture2D picture = floor.Layer != null ? floor.Layer.diffuseTexture : null;
                inks.Add(new PlaceBarInk(floor, texts.Get(floor.NameKey), texts.Get(TextKeys.FloorInkDetail), picture));
            }
            return inks;
        }

        /// <summary>
        /// La superficie, les places de charrette et la taille du pinceau ; vides quand l'outil ne les a pas.
        /// </summary>
        public static (string Area, string Marks, string Size) Values(PlaceTool tool, PlaceZone place, TextTable texts)
        {
            bool zone = tool.Entry.LaysTheZone;
            string area = zone
                ? texts.Get(TextKeys.PlaceArea).Replace(TextKeys.AreaParameter, Whole(place.AreaSquareMeters))
                    .Replace(TextKeys.MaxParameter, Whole(place.MaxAreaSquareMeters))
                : "";
            string marks = zone
                ? texts.Get(TextKeys.PlaceMarks)
                    .Replace(TextKeys.MarksParameter, place.Marks.Count.ToString(CultureInfo.InvariantCulture))
                    .Replace(TextKeys.MaxParameter, place.MaxMarks.ToString(CultureInfo.InvariantCulture))
                : "";
            bool brush = tool.Mode == PlaceToolMode.Brush || tool.Mode == PlaceToolMode.Eraser;
            string size = brush
                ? texts.Get(TextKeys.PlaceBrushSize).Replace(TextKeys.SizeParameter,
                    (tool.BrushRadius * 2f).ToString("0.#", CultureInfo.InvariantCulture))
                : "";
            return (area, marks, size);
        }

        private static string Whole(float squareMeters)
        {
            return Mathf.RoundToInt(squareMeters).ToString(CultureInfo.InvariantCulture);
        }
    }
}
