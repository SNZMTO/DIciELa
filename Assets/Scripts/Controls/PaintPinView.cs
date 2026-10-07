using System.Collections.Generic;
using Diciela.Simulation.Land;
using Diciela.View;

namespace Diciela.Controls
{
    /// <summary>
    /// L'épingle de la symétrie, vue (décision 216) : un rond au centre des copies, et l'axe du miroir, les deux axes du
    /// double miroir ou les rayons de l'étoile ; rien sans symétrie.
    /// </summary>
    internal sealed class PaintPinView
    {
        private readonly List<float> _axes = new List<float>();

        public void Show(PaintToolPreview preview, Relief relief, PaintOptions options)
        {
            _axes.Clear();
            PaintSymmetry symmetry = options.Symmetry;
            int count = symmetry == PaintSymmetry.Mirror ? 1
                : symmetry == PaintSymmetry.DoubleMirror ? 2
                : symmetry == PaintSymmetry.Star ? options.StarCopies
                : 0;
            float share = symmetry == PaintSymmetry.Star ? 360f / count : 90f;
            for (int i = 0; i < count; i++)
                _axes.Add(options.AxisDegrees + share * i);
            preview.ShowPin(relief, count > 0 && options.HasPin, options.PinCenter, _axes, symmetry == PaintSymmetry.Star);
        }
    }
}
