using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La carte de l'outil en main (GDD, section 16 ; décision 221), en bas à droite, à côté de l'icône des outils : un
    /// feutre de lin ; à gauche, l'icône de l'outil sur une tuile de sa couleur ; à droite, son nom (et le mode d'un
    /// outil de sol, son icône et son nom), ses puces dans des creux de lin, qui passent à la ligne, puis la raison
    /// d'un refus en brique, ou ce qu'un clic ferait, en douce. Sa largeur est fixe : une raison qui revient à la ligne
    /// dans un bloc qui rétrécit sortirait du cadre. Cachée les mains vides. La roue en fait sa seconde porte
    /// (WheelDoors) : un clic l'ouvre sur les réglages de l'outil en main, et la carte s'efface tant qu'elle est
    /// ouverte. C'est la vue (le bloc « carte-outil » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    public sealed class ToolCard : IToolCardView
    {
        private const string IconClass = "carte-outil-icone";
        private const string ModeIconClass = "carte-outil-mode-icone";
        private const string ChipIconClass = "carte-outil-puce-icone";
        private const string RefusalClass = "refus";
        private const string SoftClass = "douce";
        private static readonly string[] ChipClasses = { "creux", "pan-4", "carte-outil-puce" };

        private readonly VisualElement _card;
        private readonly VisualElement _tile;
        private readonly VisualElement _icon;
        private readonly Label _name;
        private readonly VisualElement _modeLine;
        private readonly VisualElement _modeIcon;
        private readonly Label _mode;
        private readonly VisualElement _chips;
        private readonly Label _reason;
        private string _pastel = "";
        private ToolCardPresenter _presenter;

        public ToolCard(UIDocument document)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            _card = root?.Q("carte-outil");
            _tile = root?.Q("carte-outil-tuile");
            _icon = root?.Q("carte-outil-icone");
            _name = root?.Q<Label>("carte-outil-nom");
            _modeLine = root?.Q("carte-outil-mode");
            _modeIcon = root?.Q("carte-outil-mode-icone");
            _mode = root?.Q<Label>("carte-outil-mode-nom");
            _chips = root?.Q("carte-outil-puces");
            _reason = root?.Q<Label>("carte-outil-raison");
            if (_card == null || _tile == null || _icon == null || _name == null || _modeLine == null ||
                _modeIcon == null || _mode == null || _chips == null || _reason == null)
                throw new InvalidOperationException("La carte de l'outil en main ne trouve pas ses éléments (UXML).");
            ShowNothing();
        }

        /// <summary>Relie la carte aux outils de ce monde ; cachée tant qu'aucun n'est en main.</summary>
        public void Show(TextTable texts, ClearingTool clearing, MoveTool move, PaintTool paths, PaintTool plaza,
            World world)
        {
            Hide();
            _presenter = new ToolCardPresenter(this, texts, clearing, move, paths, plaza, world);
        }

        /// <summary>Cache la carte et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            _chips.Clear();
            ShowNothing();
        }

        public void ShowNothing()
        {
            _card.AddToClassList(StyleClasses.Hidden);
        }

        public void ShowTool(string icon, string pastel, string name, string modeIcon, string mode)
        {
            if (pastel != _pastel)
            {
                if (_pastel.Length > 0)
                    _tile.RemoveFromClassList(_pastel);
                if (pastel.Length > 0)
                    _tile.AddToClassList(pastel);
                _pastel = pastel;
            }
            WheelPicture.Paint(_icon, IconClass, icon, null);
            _name.text = name;
            WheelPicture.Paint(_modeIcon, ModeIconClass, modeIcon, null);
            _mode.text = mode;
            _modeLine.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(mode));
            _card.RemoveFromClassList(StyleClasses.Hidden);
        }

        /// <summary>Montre ces puces : les creux déjà faits sont repeints, les autres faits ou cachés.</summary>
        public void ShowChips(IReadOnlyList<ToolCardChip> chips)
        {
            while (_chips.childCount < chips.Count)
                _chips.Add(NewChip());
            for (int i = 0; i < _chips.childCount; i++)
            {
                VisualElement chip = _chips[i];
                chip.EnableInClassList(StyleClasses.Hidden, i >= chips.Count);
                if (i < chips.Count)
                    Paint(chip, chips[i]);
            }
        }

        public void ShowReason(string text, bool refusal)
        {
            _reason.text = text;
            _reason.EnableInClassList(RefusalClass, refusal);
            _reason.EnableInClassList(SoftClass, !refusal);
            _reason.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(text));
        }

        private static VisualElement NewChip()
        {
            Facet chip = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, ChipClasses);
            chip.Add(new VisualElement { pickingMode = PickingMode.Ignore });
            chip.Add(Styled.Text("", "petit"));
            return chip;
        }

        // Le visage d'une puce : son icône ou sa vignette, cachée si elle n'a ni l'une ni l'autre, et son texte.
        private static void Paint(VisualElement chip, ToolCardChip content)
        {
            VisualElement face = chip[0];
            WheelPicture.Paint(face, ChipIconClass, content.Icon, content.Picture);
            face.EnableInClassList(StyleClasses.Hidden, content.Icon.Length == 0 && content.Picture == null);
            ((Label)chip[1]).text = content.Text;
        }
    }
}
