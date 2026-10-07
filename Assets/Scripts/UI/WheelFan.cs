using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'éventail d'une case de la roue (décision 221) : ses options en jetons de feutre, au-delà du plateau, autour du
    /// cap de leur case, à l'écart que donne la feuille de style ; l'option prise est enfoncée, à la couleur de son outil,
    /// les autres de lin ; une pastille dit un nombre (les branches de l'étoile). On y vise au survol, on y clique. Il
    /// refait ses jetons quand leur nombre change, sinon les met à jour sur place.
    /// </summary>
    internal sealed class WheelFan
    {
        private const string AimedClass = "vise";
        private const string LinenClass = "lin";
        private static readonly string[] TokenClasses = { "feutre", "pan-10", "roue-jeton" };

        private readonly VisualElement _box;
        private readonly List<FeltButton> _tokens = new List<FeltButton>();
        private readonly List<string> _pastels = new List<string>();

        public WheelFan(VisualElement box)
        {
            _box = box;
        }

        /// <summary>Levé quand le pointeur entre sur un jeton : son rang.</summary>
        public event Action<int> Hovered;

        /// <summary>Levé quand on clique un jeton : son rang.</summary>
        public event Action<int> Clicked;

        public int Count => _tokens.Count;

        /// <summary>Montre ces options autour de ce cap, sur ce cercle, écartées de step degrés, à ce pastel.</summary>
        public void Show(IReadOnlyList<WheelOption> options, float slotDegrees, float radius, float stepDegrees,
            string pastel)
        {
            if (options.Count != _tokens.Count)
                Rebuild(options.Count);
            for (int i = 0; i < options.Count; i++)
            {
                Vector2 at = WheelGeometry.OnCircle(
                    WheelGeometry.OptionDegrees(slotDegrees, i, options.Count, stepDegrees), radius);
                FeltButton token = _tokens[i];
                token.style.left = at.x;
                token.style.top = at.y;
                Paint(i, options[i], pastel);
            }
        }

        /// <summary>Marque le jeton visé (-1 : aucun).</summary>
        public void Aim(int option)
        {
            for (int i = 0; i < _tokens.Count; i++)
                _tokens[i].EnableInClassList(AimedClass, i == option);
        }

        public void Clear()
        {
            _box.Clear();
            _tokens.Clear();
            _pastels.Clear();
        }

        public void SetPickable(bool pickable)
        {
            foreach (FeltButton token in _tokens)
                token.pickingMode = pickable ? PickingMode.Position : PickingMode.Ignore;
        }

        private void Rebuild(int count)
        {
            Clear();
            for (int i = 0; i < count; i++)
            {
                int index = i;
                FeltButton token = Styled.With(new FeltButton(), TokenClasses);
                token.Add(new VisualElement { pickingMode = PickingMode.Ignore });
                Facet badge = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, "creux", "pan-4",
                    "roue-pastille");
                badge.Add(Styled.Text("", "touche-texte"));
                token.Add(badge);
                token.RegisterCallback<PointerEnterEvent>(_ => Hovered?.Invoke(index));
                token.Clicked += () => Clicked?.Invoke(index);
                _box.Add(token);
                _tokens.Add(token);
                _pastels.Add("");
            }
        }

        // Le visage d'un jeton : son icône ou sa vignette, sa pastille, son feutre (enfoncé et à la couleur de l'outil
        // s'il est pris, de lin sinon).
        private void Paint(int index, WheelOption option, string pastel)
        {
            FeltButton token = _tokens[index];
            string face = option.IsOn ? pastel : LinenClass;
            if (_pastels[index] != face)
            {
                if (_pastels[index].Length > 0)
                    token.RemoveFromClassList(_pastels[index]);
                if (face.Length > 0)
                    token.AddToClassList(face);
                _pastels[index] = face;
            }
            token.EnableInClassList(StyleClasses.Open, option.IsOn);
            WheelPicture.Paint(token[0], "roue-jeton-icone", option.Icon, option.Picture);
            var badge = (Facet)token[1];
            ((Label)badge[0]).text = option.Badge;
            badge.EnableInClassList(StyleClasses.Hidden, option.Badge.Length == 0);
        }
    }
}
