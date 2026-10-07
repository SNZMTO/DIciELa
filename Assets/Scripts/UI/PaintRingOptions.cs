using System;
using System.Collections.Generic;
using System.Globalization;
using Diciela.Controls;
using Diciela.Data;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Les éventails de la roue d'un outil de sol (décision 221), traduits : les styles d'un tracé, les formes du mode
    /// Forme, les symétries (l'étoile porte le nombre de ses branches en pastille), et ce que l'outil peut poser, la zone
    /// d'abord (l'outil de la Place), puis les sols que la ville sait faire, chacun la texture de son calque pour vignette ;
    /// chaque option prise l'est dans l'outil.
    /// </summary>
    internal sealed class PaintRingOptions
    {
        public const string ZoneIcon = "place-icone-superficie";
        public static readonly PaintTraceStyle[] Styles = (PaintTraceStyle[])Enum.GetValues(typeof(PaintTraceStyle));
        public static readonly PaintShape[] Shapes = (PaintShape[])Enum.GetValues(typeof(PaintShape));
        public static readonly PaintSymmetry[] SymmetryValues = (PaintSymmetry[])Enum.GetValues(typeof(PaintSymmetry));

        private readonly TextTable _texts;
        private readonly List<PaintInk> _inks = new List<PaintInk>();

        public PaintRingOptions(TextTable texts)
        {
            _texts = texts;
        }

        /// <summary>Les options d'un mode : les styles du tracé, les formes du mode Forme ; aucune pour les autres.</summary>
        public IReadOnlyList<WheelOption> Of(PaintTool tool, PaintToolMode mode)
        {
            var options = new List<WheelOption>();
            if (mode == PaintToolMode.Trace)
            {
                foreach (PaintTraceStyle style in Styles)
                    options.Add(new WheelOption("place-style-" + style.ToString().ToLowerInvariant(), null,
                        _texts.Get(TextKeys.PaintStyle(style.ToString())),
                        _texts.Get(TextKeys.PaintStyleDetail(style.ToString())), tool.Options.Style == style));
            }
            else if (mode == PaintToolMode.Shape)
            {
                foreach (PaintShape shape in Shapes)
                    options.Add(new WheelOption("place-forme-" + shape.ToString().ToLowerInvariant(), null,
                        _texts.Get(TextKeys.PaintShape(shape.ToString())),
                        _texts.Get(TextKeys.PaintShapeDetail(shape.ToString())), tool.Options.Shape == shape));
            }
            return options;
        }

        /// <summary>Les symétries, dans leur ordre ; l'étoile porte le nombre de ses branches.</summary>
        public IReadOnlyList<WheelOption> Symmetries(PaintTool tool)
        {
            var options = new List<WheelOption>();
            string branches = tool.Options.StarBranches.ToString(CultureInfo.InvariantCulture);
            foreach (PaintSymmetry symmetry in SymmetryValues)
            {
                bool star = symmetry == PaintSymmetry.Star;
                string detail = star
                    ? _texts.Get(TextKeys.PaintStarDetail).Replace(TextKeys.BranchesParameter, branches)
                    : _texts.Get(TextKeys.PaintSymmetryDetail(symmetry.ToString()));
                options.Add(new WheelOption(SymmetryIcon(symmetry), null,
                    _texts.Get(TextKeys.PaintSymmetry(symmetry.ToString())), detail, tool.Options.Symmetry == symmetry,
                    star ? branches : ""));
            }
            return options;
        }

        /// <summary>Ce que l'outil peut poser, dans l'ordre de son éventail, chacun sa vignette.</summary>
        public IReadOnlyList<WheelOption> Inks(PaintTool tool)
        {
            var options = new List<WheelOption>();
            foreach (PaintInk ink in InksOf(tool))
            {
                bool chosen = ink.Equals(tool.Inks.Chosen);
                options.Add(ink.IsZone
                    ? new WheelOption(ZoneIcon, null, _texts.Get(TextKeys.PlazaZoneInk),
                        _texts.Get(TextKeys.PlazaZoneInkDetail), chosen)
                    : new WheelOption("", Picture(ink.Floor), _texts.Get(ink.Floor.NameKey),
                        _texts.Get(TextKeys.FloorInkDetail), chosen));
            }
            return options;
        }

        /// <summary>Ce que l'outil peut poser : la zone d'abord (l'outil de la Place), puis ses sols.</summary>
        public List<PaintInk> InksOf(PaintTool tool)
        {
            _inks.Clear();
            if (tool.Entry.LaysTheZone)
                _inks.Add(PaintInk.Zone);
            foreach (FloorDefinition floor in tool.Inks.Known)
                _inks.Add(PaintInk.Of(floor));
            return _inks;
        }

        public static string SymmetryIcon(PaintSymmetry symmetry)
        {
            return "place-symetrie-" + symmetry.ToString().ToLowerInvariant();
        }

        public static Texture2D Picture(FloorDefinition floor)
        {
            return floor.Layer != null ? floor.Layer.diffuseTexture : null;
        }
    }
}
