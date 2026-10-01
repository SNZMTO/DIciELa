using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Une face plate de l'interface « fusion », aux coins coupés (GDD, section 16 ; décision 145) : une applique posée à
    /// plat, un feutre en relief qui se clique, un creux qui contient une valeur, une bulle au liseré. Elle se dessine en
    /// vectoriel, nette à toute taille, et tout lui vient de la feuille de style (FacetLook) : un état (le survol,
    /// l'appui, un onglet ouvert) ne fait que changer ses propriétés, et elle se redessine.
    /// </summary>
    [UxmlElement]
    public partial class Facet : VisualElement
    {
        /// <summary>La classe que porte toute face : elle lui donne les mesures communes (Fusion.uss).</summary>
        public const string FacetClass = "facette";

        private FacetLook _look;

        public Facet()
        {
            AddToClassList(FacetClass);
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            generateVisualContent += Draw;
        }

        private void OnStylesResolved(CustomStyleResolvedEvent resolved)
        {
            _look = FacetLook.Read(resolved.customStyle);
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            var box = new Rect(Vector2.zero, layout.size);
            if (box.width <= 0f || box.height <= 0f || _look.Face.a <= 0f)
                return;

            Painter2D painter = context.painter2D;
            float cut = _look.Cut;
            // L'ombre est dure, dessinée sous la face : un filtre d'ombre dans un cadre qui a déjà le sien se rendrait
            // flou sur un grand écran.
            if (_look.HasShadow)
                Chamfer.Fill(painter, new Rect(box.x, box.y + _look.ShadowOffset, box.width, box.height), cut, _look.Shadow);
            if (_look.HasRim)
            {
                Chamfer.Fill(painter, box, cut, _look.Rim);
                float rim = _look.RimWidth;
                box = Chamfer.Inset(box, rim, rim, rim, rim);
                cut = Chamfer.Inner(cut, rim);
            }
            Chamfer.Fill(painter, box, cut, _look.Face);
            painter.BeginPath();
            Chamfer.Trace(painter, box, cut);
            painter.PushClip();
            DrawInside(painter, box);
            if (_look.IsHollow)
                DrawHollow(painter, box);
            if (_look.HasRelief)
                DrawRelief(painter, box);
            painter.PopClip();
            if (_look.IsStitched)
                DrawStitch(painter, box, cut);
            DrawOver(painter, box);
        }

        /// <summary>Ce qu'une face plus précise peint sur la sienne, découpé à ses coins, sous l'ombre d'un creux.</summary>
        protected virtual void DrawInside(Painter2D painter, Rect face)
        {
        }

        /// <summary>Ce qu'une face plus précise peint par-dessus tout, sans découpe (un repère qui dépasse).</summary>
        protected virtual void DrawOver(Painter2D painter, Rect face)
        {
        }

        // Le creux : une ombre intérieure en haut, décalée puis fondue (un pixel par bande), et un filet de lumière en
        // bas, comme un lin enfoncé.
        private void DrawHollow(Painter2D painter, Rect box)
        {
            Color shadow = _look.HollowShadow;
            Band(painter, box.xMin, box.yMin, box.width, _look.HollowOffset, shadow);
            int steps = Mathf.CeilToInt(_look.HollowBlur);
            for (int step = 0; step < steps; step++)
            {
                Color faded = shadow;
                faded.a *= 1f - (step + 0.5f) / _look.HollowBlur;
                Band(painter, box.xMin, box.yMin + _look.HollowOffset + step, box.width, 1f, faded);
            }
            Band(painter, box.xMin, box.yMax - 1f, box.width, 1f, _look.HollowLight);
        }

        // Le relief d'un feutre : la lèvre en bas et le reflet en haut ; enfoncé, la lèvre passe en haut.
        private void DrawRelief(Painter2D painter, Rect box)
        {
            float lip = _look.LipHeight;
            float shine = _look.ShineHeight;
            if (_look.Pressed)
            {
                Band(painter, box.xMin, box.yMin, box.width, lip, _look.Lip);
                Band(painter, box.xMin, box.yMax - shine, box.width, shine, _look.Shine);
            }
            else
            {
                Band(painter, box.xMin, box.yMax - lip, box.width, lip, _look.Lip);
                Band(painter, box.xMin, box.yMin, box.width, shine, _look.Shine);
            }
        }

        // La couture d'un grand bouton : des points de fil tout autour, rentrés du bord ; la lèvre ne se coud pas.
        private void DrawStitch(Painter2D painter, Rect box, float cut)
        {
            float inset = _look.StitchInset;
            float lip = _look.HasRelief ? _look.LipHeight : 0f;
            float top = _look.Pressed ? inset + lip : inset;
            float bottom = _look.Pressed ? inset : inset + lip;
            painter.strokeColor = _look.Stitch;
            painter.lineWidth = _look.StitchWidth;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            painter.SetDashPattern(_look.Dash, _look.Gap);
            painter.BeginPath();
            Chamfer.Trace(painter, Chamfer.Inset(box, inset, top, inset, bottom), Chamfer.Inner(cut, inset));
            painter.Stroke();
            painter.SetDashPattern(default(System.ReadOnlySpan<float>));
        }

        private static void Band(Painter2D painter, float x, float y, float width, float height, Color color)
        {
            if (height <= 0f || color.a <= 0f)
                return;
            Chamfer.Fill(painter, new Rect(x, y, width, height), 0f, color);
        }
    }
}
