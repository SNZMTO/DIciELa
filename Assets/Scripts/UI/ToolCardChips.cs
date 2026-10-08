using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La rangée de puces de la carte de l'outil en main (décisions 221 et 223) : des creux de lin, faits à la demande et
    /// repeints sur place, cachés quand ils ne servent pas ; chacun son icône ou sa vignette, et son texte. Quand le
    /// nombre en tête d'un texte change et que la suite reste la même (« 312 / 450 m² »), il défile jusqu'à sa valeur
    /// (décision 225).
    /// </summary>
    internal sealed class ToolCardChips
    {
        private const string ChipIconClass = "carte-outil-puce-icone";
        private static readonly string[] ChipClasses = { "creux", "pan-4", "carte-outil-puce" };

        private readonly VisualElement _row;
        private readonly List<IVisualElementScheduledItem> _rolls = new List<IVisualElementScheduledItem>();

        public ToolCardChips(VisualElement row)
        {
            _row = row;
        }

        /// <summary>Montre ces puces : les creux déjà faits sont repeints, les autres faits ou cachés.</summary>
        public void Show(IReadOnlyList<ToolCardChip> chips)
        {
            while (_row.childCount < chips.Count)
            {
                _row.Add(NewChip());
                _rolls.Add(null);
            }
            for (int i = 0; i < _row.childCount; i++)
            {
                VisualElement chip = _row[i];
                chip.EnableInClassList(StyleClasses.Hidden, i >= chips.Count);
                if (i < chips.Count)
                    Paint(i, chips[i]);
            }
        }

        public void Clear()
        {
            foreach (IVisualElementScheduledItem roll in _rolls)
                roll?.Pause();
            _rolls.Clear();
            _row.Clear();
        }

        private static VisualElement NewChip()
        {
            Facet chip = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, ChipClasses);
            chip.Add(new VisualElement { pickingMode = PickingMode.Ignore });
            chip.Add(Styled.Text("", "petit"));
            return chip;
        }

        // Le visage d'une puce : son icône ou sa vignette, cachée si elle n'en a pas, et son texte.
        private void Paint(int index, ToolCardChip content)
        {
            VisualElement chip = _row[index];
            VisualElement face = chip[0];
            content.Face.Paint(face, ChipIconClass);
            face.EnableInClassList(StyleClasses.Hidden, content.Face.IsEmpty);
            Write(index, (Label)chip[1], content.Text);
        }

        // Le texte d'une puce : son nombre défile s'il change seul, sinon il s'écrit d'un coup.
        private void Write(int index, Label label, string text)
        {
            if (label.text == text)
                return;
            _rolls[index]?.Pause();
            _rolls[index] = null;
            if (Number(label.text, out int from, out string before) && Number(text, out int to, out string after) &&
                before == after)
                _rolls[index] = HudMotion.Roll(label, from, to, after);
            else
                label.text = text;
        }

        // Le nombre entier en tête de ce texte, et ce qui le suit ; faux s'il ne commence pas par un chiffre.
        private static bool Number(string text, out int number, out string rest)
        {
            int digits = 0;
            while (digits < text.Length && char.IsDigit(text[digits]))
                digits++;
            rest = text.Substring(digits);
            number = 0;
            return digits > 0 && digits < 10 &&
                   int.TryParse(text.Substring(0, digits), NumberStyles.None, CultureInfo.InvariantCulture, out number);
        }
    }
}
