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
    /// dans un bloc qui rétrécit sortirait du cadre. Cachée les mains vides ; elle glisse quand on prend ou lâche un
    /// outil, et ses nombres défilent (ToolCardChips ; décision 225). La roue en fait sa seconde porte (WheelDoors) :
    /// un clic l'ouvre sur les réglages de l'outil en main, et la carte s'efface tant qu'elle est ouverte. C'est la vue
    /// (le bloc « carte-outil » de Hud.uxml) ; son présentateur décide.
    /// </summary>
    public sealed class ToolCard : IToolCardView
    {
        private const string IconClass = "carte-outil-icone";
        private const string ModeIconClass = "carte-outil-mode-icone";
        private const string RefusalClass = "refus";
        private const string SoftClass = "douce";
        private const string EnterClass = "carte-outil-entree";
        private const string LeaveClass = "carte-outil-sortie";
        // Le temps de glisser hors de l'écran, avant de se cacher (la transition de « carte-outil-sortie »).
        private const long LeaveMs = 160;

        private readonly VisualElement _card;
        private readonly VisualElement _tile;
        private readonly VisualElement _icon;
        private readonly Label _name;
        private readonly VisualElement _modeLine;
        private readonly VisualElement _modeIcon;
        private readonly Label _mode;
        private readonly ToolCardChips _chips;
        private readonly Label _reason;
        private string _pastel = "";
        private ToolCardPresenter _presenter;
        private IVisualElementScheduledItem _leaving;

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
            VisualElement chips = root?.Q("carte-outil-puces");
            _reason = root?.Q<Label>("carte-outil-raison");
            if (_card == null || _tile == null || _icon == null || _name == null || _modeLine == null ||
                _modeIcon == null || _mode == null || chips == null || _reason == null)
                throw new InvalidOperationException("La carte de l'outil en main ne trouve pas ses éléments (UXML).");
            _chips = new ToolCardChips(chips);
            HideNow();
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
            HideNow();
        }

        /// <summary>Les mains vides : la carte glisse hors de l'écran, puis se cache.</summary>
        public void ShowNothing()
        {
            if (_card.ClassListContains(StyleClasses.Hidden) || _leaving != null)
                return;
            _card.AddToClassList(LeaveClass);
            _leaving = _card.schedule.Execute(HideNow).StartingIn(LeaveMs);
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
            if (_leaving != null)
            {
                // Un outil repris pendant qu'elle part : elle revient, sans repartir de loin.
                _leaving.Pause();
                _leaving = null;
                _card.RemoveFromClassList(LeaveClass);
            }
            else if (_card.ClassListContains(StyleClasses.Hidden))
            {
                _card.RemoveFromClassList(StyleClasses.Hidden);
                HudMotion.Enter(_card, EnterClass);
            }
        }

        public void ShowChips(IReadOnlyList<ToolCardChip> chips)
        {
            _chips.Show(chips);
        }

        public void ShowReason(string text, bool refusal)
        {
            _reason.text = text;
            _reason.EnableInClassList(RefusalClass, refusal);
            _reason.EnableInClassList(SoftClass, !refusal);
            _reason.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(text));
        }

        // Se cache tout de suite, sans glisser.
        private void HideNow()
        {
            _leaving?.Pause();
            _leaving = null;
            _card.RemoveFromClassList(LeaveClass);
            _card.AddToClassList(StyleClasses.Hidden);
        }
    }
}
