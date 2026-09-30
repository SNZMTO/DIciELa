using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La jauge de la journée (GDD, section 16) : un creux où le travail, la soirée et la nuit (hachurée) se suivent, à
    /// leur part de la journée ; ce qui est passé prend la couleur plus sombre de sa phase ; un repère marque l'heure.
    /// Ses couleurs et ses mesures viennent de la feuille de style (--travail, --soiree, --nuit…).
    /// </summary>
    [UxmlElement]
    public partial class DayGauge : Facet
    {
        private static readonly CustomStyleProperty<Color> WorkProperty = new CustomStyleProperty<Color>("--travail");
        private static readonly CustomStyleProperty<Color> WorkPastProperty =
            new CustomStyleProperty<Color>("--travail-passe");
        private static readonly CustomStyleProperty<Color> EveningProperty = new CustomStyleProperty<Color>("--soiree");
        private static readonly CustomStyleProperty<Color> EveningPastProperty =
            new CustomStyleProperty<Color>("--soiree-passee");
        private static readonly CustomStyleProperty<Color> NightProperty = new CustomStyleProperty<Color>("--nuit");
        private static readonly CustomStyleProperty<Color> NightPastProperty =
            new CustomStyleProperty<Color>("--nuit-passee");
        private static readonly CustomStyleProperty<Color> HatchProperty = new CustomStyleProperty<Color>("--hachure");
        private static readonly CustomStyleProperty<float> HatchWidthProperty =
            new CustomStyleProperty<float>("--hachure-trait");
        private static readonly CustomStyleProperty<float> HatchStepProperty =
            new CustomStyleProperty<float>("--hachure-pas");
        private static readonly CustomStyleProperty<Color> SeparatorProperty =
            new CustomStyleProperty<Color>("--separateur");
        private static readonly CustomStyleProperty<float> SeparatorWidthProperty =
            new CustomStyleProperty<float>("--separateur-largeur");
        private static readonly CustomStyleProperty<Color> MarkProperty = new CustomStyleProperty<Color>("--repere");
        private static readonly CustomStyleProperty<float> MarkWidthProperty =
            new CustomStyleProperty<float>("--repere-largeur");
        private static readonly CustomStyleProperty<float> MarkHeightProperty =
            new CustomStyleProperty<float>("--repere-hauteur");
        private static readonly CustomStyleProperty<float> MarkRiseProperty =
            new CustomStyleProperty<float>("--repere-depasse");

        private readonly Color[] _phases = new Color[3];
        private readonly Color[] _pasts = new Color[3];
        private Color _hatch;
        private float _hatchWidth;
        private float _hatchStep;
        private Color _separator;
        private float _separatorWidth;
        private Color _mark;
        private float _markWidth;
        private float _markHeight;
        private float _markRise;
        private float _workEnd;
        private float _eveningEnd;
        private float _progress;

        public DayGauge()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnGaugeStylesResolved);
        }

        /// <summary>Les bornes des phases, en part de la journée : la fin du travail, la fin de la soirée.</summary>
        public void SetPhases(float workEnd, float eveningEnd)
        {
            _workEnd = Mathf.Clamp01(workEnd);
            _eveningEnd = Mathf.Clamp(eveningEnd, _workEnd, 1f);
            MarkDirtyRepaint();
        }

        /// <summary>La part de la journée passée ; la jauge ne se redessine que si le repère bouge d'un quart de pixel.</summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            float width = layout.width;
            if (float.IsNaN(width) || Mathf.Abs((progress - _progress) * width) >= 0.25f || progress < _progress)
            {
                _progress = progress;
                MarkDirtyRepaint();
            }
        }

        private void OnGaugeStylesResolved(CustomStyleResolvedEvent resolved)
        {
            ICustomStyle style = resolved.customStyle;
            _phases[0] = Read(style, WorkProperty);
            _pasts[0] = Read(style, WorkPastProperty);
            _phases[1] = Read(style, EveningProperty);
            _pasts[1] = Read(style, EveningPastProperty);
            _phases[2] = Read(style, NightProperty);
            _pasts[2] = Read(style, NightPastProperty);
            _hatch = Read(style, HatchProperty);
            _hatchWidth = Read(style, HatchWidthProperty);
            _hatchStep = Read(style, HatchStepProperty);
            _separator = Read(style, SeparatorProperty);
            _separatorWidth = Read(style, SeparatorWidthProperty);
            _mark = Read(style, MarkProperty);
            _markWidth = Read(style, MarkWidthProperty);
            _markHeight = Read(style, MarkHeightProperty);
            _markRise = Read(style, MarkRiseProperty);
            MarkDirtyRepaint();
        }

        protected override void DrawInside(Painter2D painter, Rect face)
        {
            float start = 0f;
            for (int phase = 0; phase < _phases.Length; phase++)
            {
                float end = End(phase);
                float past = Mathf.Clamp(_progress, start, end);
                Span(painter, face, start, past, _pasts[phase]);
                Span(painter, face, past, end, _phases[phase]);
                start = end;
            }
            DrawHatch(painter, new Rect(face.x + face.width * _eveningEnd, face.y, face.width * (1f - _eveningEnd),
                face.height));
            for (int phase = 0; phase < _phases.Length - 1; phase++)
                Span(painter, face, End(phase) - _separatorWidth / face.width, End(phase), _separator);
        }

        // La fin de chaque phase : le travail, la soirée, puis la nuit jusqu'au bout.
        private float End(int phase)
        {
            return phase == 0 ? _workEnd : phase == 1 ? _eveningEnd : 1f;
        }

        protected override void DrawOver(Painter2D painter, Rect face)
        {
            if (_mark.a <= 0f || _markWidth <= 0f)
                return;
            float x = face.x + face.width * _progress;
            float top = face.y - _markRise;
            painter.fillColor = _mark;
            painter.BeginPath();
            painter.MoveTo(new Vector2(x - _markWidth * 0.5f, top));
            painter.LineTo(new Vector2(x + _markWidth * 0.5f, top));
            painter.LineTo(new Vector2(x, top + _markHeight));
            painter.ClosePath();
            painter.Fill();
        }

        // La nuit hachurée : des traits à 45°, d'un pas régulier, découpés à la nuit.
        private void DrawHatch(Painter2D painter, Rect night)
        {
            if (_hatch.a <= 0f || _hatchStep <= 0f || night.width <= 0f)
                return;
            painter.BeginPath();
            Chamfer.Trace(painter, night, 0f);
            painter.PushClip();
            painter.fillColor = _hatch;
            float along = _hatchWidth * Mathf.Sqrt(2f);
            for (float x = night.xMin - night.height; x < night.xMax; x += _hatchStep * Mathf.Sqrt(2f))
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, night.yMax));
                painter.LineTo(new Vector2(x + along, night.yMax));
                painter.LineTo(new Vector2(x + along + night.height, night.yMin));
                painter.LineTo(new Vector2(x + night.height, night.yMin));
                painter.ClosePath();
                painter.Fill();
            }
            painter.PopClip();
        }

        private static void Span(Painter2D painter, Rect face, float from, float to, Color color)
        {
            if (to <= from || color.a <= 0f)
                return;
            Chamfer.Fill(painter, new Rect(face.x + face.width * from, face.y, face.width * (to - from), face.height),
                0f, color);
        }

        private static Color Read(ICustomStyle style, CustomStyleProperty<Color> property)
        {
            return style.TryGetValue(property, out Color value) ? value : Color.clear;
        }

        private static float Read(ICustomStyle style, CustomStyleProperty<float> property)
        {
            return style.TryGetValue(property, out float value) ? value : 0f;
        }
    }
}
