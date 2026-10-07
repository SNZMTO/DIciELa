using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// La roue d'un outil de sol (GDD, section 16 ; décision 221), le seul endroit qui sait quelle case fait quoi : 1
    /// pinceau, 2 gomme, 3 tracé (droit ou en courbe), 4 forme (cercle, rectangle, polygone), 5 place de charrette (l'outil
    /// de la Place seulement), 6 outils, 7 symétrie (et les branches de l'étoile), 8 sol (la zone, puis les sols que la
    /// ville sait faire, chacun sa vignette). Prendre un mode le met en main avec ses derniers réglages, une option le
    /// règle aussi ; la symétrie et le sol se règlent sans changer de mode. Au clavier, le même chiffre deux fois passe à
    /// l'option suivante ; dans la roue, la molette change la taille du pinceau ou les branches de l'étoile.
    /// </summary>
    internal sealed class PaintRing
    {
        private const int Back = 5;
        private const int Symmetry = 6;
        private const int Ink = 7;
        private const string BackIcon = "roue-retour";
        private static readonly PaintToolMode[] Modes =
            { PaintToolMode.Brush, PaintToolMode.Eraser, PaintToolMode.Trace, PaintToolMode.Shape, PaintToolMode.CartSpot };
        private static readonly PaintTraceStyle[] Styles = PaintRingOptions.Styles;
        private static readonly PaintShape[] Shapes = PaintRingOptions.Shapes;
        private static readonly PaintSymmetry[] Symmetries = PaintRingOptions.SymmetryValues;

        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly PaintRingOptions _options;

        public PaintRing(TextTable texts, IToolKeys keys)
        {
            _texts = texts;
            _keys = keys;
            _options = new PaintRingOptions(texts);
        }

        /// <summary>Les cases de la roue de cet outil, à la couleur de son outil.</summary>
        public List<WheelSlot> Slots(PaintTool tool, string pastel)
        {
            var slots = new List<WheelSlot>();
            for (int place = 0; place < Modes.Length; place++)
            {
                PaintToolMode mode = Modes[place];
                if (!tool.Entry.Allows(mode))
                    continue;
                string name = mode.ToString();
                slots.Add(new WheelSlot(place, PaintIconClasses.Mode(mode), null, pastel, Key(place),
                    _texts.Get(TextKeys.PaintMode(name)), _texts.Get(TextKeys.PaintModeDetail(name)), tool.Mode == mode,
                    _options.Of(tool, mode)));
            }
            slots.Add(new WheelSlot(Back, BackIcon, null, pastel, Key(Back), _texts.Get(TextKeys.WheelBack),
                _texts.Get(TextKeys.WheelBackDetail), false));
            slots.Add(new WheelSlot(Symmetry, PaintIconClasses.Symmetry(tool.Options.Symmetry), null, pastel,
                Key(Symmetry), _texts.Get(TextKeys.WheelSymmetry), _texts.Get(TextKeys.WheelSymmetryDetail), false,
                _options.Symmetries(tool)));
            PaintInk chosen = tool.Inks.Chosen;
            slots.Add(new WheelSlot(Ink, chosen.IsZone ? PaintIconClasses.Zone : "",
                chosen.Floor != null ? PaintIconClasses.Picture(chosen.Floor) : null, pastel, Key(Ink),
                _texts.Get(TextKeys.WheelInk), _texts.Get(TextKeys.WheelInkDetail), false, _options.Inks(tool)));
            return slots;
        }

        /// <summary>Prend cette case (option -1) ou cette option ; faux pour la case Outils, qui ramène aux outils.</summary>
        public bool Pick(PaintTool tool, int place, int option)
        {
            if (place == Back)
                return false;
            if (place < Modes.Length)
            {
                tool.Choose(Modes[place]);
                if (place == 2 && option >= 0 && option < Styles.Length)
                    tool.Options.Choose(Styles[option]);
                else if (place == 3 && option >= 0 && option < Shapes.Length)
                    tool.Options.Choose(Shapes[option]);
            }
            else if (place == Symmetry && option >= 0 && option < Symmetries.Length)
                tool.Options.Choose(Symmetries[option]);
            else if (place == Ink && option >= 0 && option < _options.InksOf(tool).Count)
                tool.ChooseInk(_options.InksOf(tool)[option]);
            else
                Key(tool, place);
            return true;
        }

        /// <summary>Le chiffre de cette case : la prendre, ou, déjà prise, passer à son option suivante ; faux pour Outils.</summary>
        public bool Key(PaintTool tool, int place)
        {
            if (place == Back)
                return false;
            if (place < Modes.Length)
            {
                PaintToolMode mode = Modes[place];
                if (tool.Mode == mode && mode == PaintToolMode.Trace)
                    tool.Options.Choose(Next(Styles, tool.Options.Style));
                else if (tool.Mode == mode && mode == PaintToolMode.Shape)
                    tool.Options.Choose(Next(Shapes, tool.Options.Shape));
                else
                    tool.Choose(mode);
            }
            else if (place == Symmetry)
                tool.Options.Choose(Next(Symmetries, tool.Options.Symmetry));
            else if (place == Ink && _options.InksOf(tool).Count > 0)
            {
                List<PaintInk> inks = _options.InksOf(tool);
                tool.ChooseInk(inks[(inks.IndexOf(tool.Inks.Chosen) + 1) % inks.Count]);
            }
            return true;
        }

        /// <summary>La molette sur cette case ou cette option : vrai si elle a réglé quelque chose.</summary>
        public bool Scroll(PaintTool tool, int place, int option, int notches)
        {
            if (place == 0 || place == 1)
            {
                tool.ResizeBrush(notches);
                return true;
            }
            if (place != Symmetry || option < 0 || option >= Symmetries.Length || Symmetries[option] != PaintSymmetry.Star)
                return false;
            tool.Options.ChooseStar(tool.Options.StarBranches + notches);
            return true;
        }

        private string Key(int place)
        {
            return _keys.KeyOfCard(place);
        }

        private static T Next<T>(T[] values, T current)
        {
            return values[(Array.IndexOf(values, current) + 1) % values.Length];
        }
    }
}
