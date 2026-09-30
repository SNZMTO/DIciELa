using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Un fanion du stock (GDD, section 16) : un lin pendu à la tringle, une bande de brique en haut, la pointe en bas,
    /// son ombre dure dessous. Il se dessine en vectoriel, net à toute taille ; ses couleurs et ses mesures viennent de
    /// la feuille de style (--face, --bande, --bande-hauteur, --pointe, --ombre, --ombre-y). Il ne se clique pas.
    /// </summary>
    [UxmlElement]
    public partial class Pennant : VisualElement
    {
        private static readonly CustomStyleProperty<Color> FaceProperty = new CustomStyleProperty<Color>("--face");
        private static readonly CustomStyleProperty<Color> BandProperty = new CustomStyleProperty<Color>("--bande");
        private static readonly CustomStyleProperty<float> BandHeightProperty =
            new CustomStyleProperty<float>("--bande-hauteur");
        private static readonly CustomStyleProperty<float> PointProperty = new CustomStyleProperty<float>("--pointe");
        private static readonly CustomStyleProperty<Color> ShadowProperty = new CustomStyleProperty<Color>("--ombre");
        private static readonly CustomStyleProperty<float> ShadowOffsetProperty =
            new CustomStyleProperty<float>("--ombre-y");

        private Color _face;
        private Color _band;
        private float _bandHeight;
        private float _point;
        private Color _shadow;
        private float _shadowOffset;

        public Pennant()
        {
            pickingMode = PickingMode.Position;
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            generateVisualContent += Draw;
        }

        private void OnStylesResolved(CustomStyleResolvedEvent resolved)
        {
            ICustomStyle style = resolved.customStyle;
            style.TryGetValue(FaceProperty, out _face);
            style.TryGetValue(BandProperty, out _band);
            style.TryGetValue(BandHeightProperty, out _bandHeight);
            style.TryGetValue(PointProperty, out _point);
            style.TryGetValue(ShadowProperty, out _shadow);
            style.TryGetValue(ShadowOffsetProperty, out _shadowOffset);
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            Vector2 size = layout.size;
            if (size.x <= 0f || size.y <= 0f)
                return;

            Painter2D painter = context.painter2D;
            float point = Mathf.Min(_point, size.y * 0.5f);
            if (_shadow.a > 0f)
                Fill(painter, size, point, _shadowOffset, _shadow);
            Fill(painter, size, point, 0f, _face);
            if (_band.a > 0f && _bandHeight > 0f)
            {
                painter.fillColor = _band;
                painter.BeginPath();
                painter.MoveTo(Vector2.zero);
                painter.LineTo(new Vector2(size.x, 0f));
                painter.LineTo(new Vector2(size.x, _bandHeight));
                painter.LineTo(new Vector2(0f, _bandHeight));
                painter.ClosePath();
                painter.Fill();
            }
        }

        /// <summary>Le fanion, descendu de dy pixels : un rectangle dont le bas se referme en pointe.</summary>
        private static void Fill(Painter2D painter, Vector2 size, float point, float dy, Color color)
        {
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0f, dy));
            painter.LineTo(new Vector2(size.x, dy));
            painter.LineTo(new Vector2(size.x, size.y - point + dy));
            painter.LineTo(new Vector2(size.x * 0.5f, size.y + dy));
            painter.LineTo(new Vector2(0f, size.y - point + dy));
            painter.ClosePath();
            painter.Fill();
        }
    }
}
