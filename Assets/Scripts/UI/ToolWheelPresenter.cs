using System;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la roue des outils (GDD, section 16 ; décision 168). Choisir un outil l'allume, en éteignant
    /// les autres, ou l'éteint s'il l'était : Construire ouvre ou ferme le panneau Construire, Retirer et Déplacer
    /// prennent ou lâchent leur outil. Ce que fait chaque outil n'est écrit qu'ici, dans sa table ; le reste se lit
    /// dans la table des outils (WheelTool). La bulle du milieu dit le nom et l'usage de l'outil visé ; un outil actif
    /// reste enfoncé. Les touches se lisent sur les liaisons, l'icône dit celle de la roue dans son infobulle.
    /// </summary>
    public sealed class ToolWheelPresenter : IDisposable
    {
        private readonly IToolWheelView _view;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly ClearingTool _clearing;
        private readonly IBuildMenu _build;
        private readonly MoveTool _move;
        private readonly Switch[] _switches;

        /// <summary>Ce qu'un outil allume et éteint, et s'il est allumé.</summary>
        private readonly struct Switch
        {
            public readonly Func<bool> IsOn;
            public readonly Action On;
            public readonly Action Off;

            public Switch(Func<bool> isOn, Action on, Action off)
            {
                IsOn = isOn;
                On = on;
                Off = off;
            }
        }

        public ToolWheelPresenter(IToolWheelView view, TextTable texts, IToolKeys keys, ClearingTool clearing,
            IBuildMenu build, MoveTool move)
        {
            if (view == null || ReferenceEquals(texts, null) || keys == null || ReferenceEquals(clearing, null) ||
                build == null || ReferenceEquals(move, null))
                throw new ArgumentException("La roue des outils a besoin de sa vue, des textes, des touches, des " +
                                            "outils « Retirer » et « Déplacer » et du panneau Construire.");

            _view = view;
            _texts = texts;
            _keys = keys;
            _clearing = clearing;
            _build = build;
            _move = move;
            // Au rang du choix de chaque outil (ToolChoice).
            _switches = new[]
            {
                new Switch(() => _build.IsOpen, _build.Open, _build.Close),
                new Switch(() => _clearing.IsActive, _clearing.Take, () => _clearing.Escape()),
                new Switch(() => _move.IsActive, _move.Take, _move.PutDown)
            };
            _view.Chosen += OnChosen;
            _view.Pointed += OnPointed;
            _clearing.HeldChanged += ShowActive;
            _move.HeldChanged += ShowActive;
            _build.OpenChanged += ShowActive;
            _view.ShowWheelKey(keys.WheelKey);
            for (int i = 0; i < _switches.Length; i++)
                _view.ShowToolKey((ToolChoice)i, keys.KeyOf((ToolChoice)i));
            _view.ShowLabel("", "");
            ShowActive();
        }

        public void Dispose()
        {
            _view.Chosen -= OnChosen;
            _view.Pointed -= OnPointed;
            _clearing.HeldChanged -= ShowActive;
            _move.HeldChanged -= ShowActive;
            _build.OpenChanged -= ShowActive;
        }

        /// <summary>L'infobulle de l'icône des outils : son nom, et la touche qui ouvre la roue sous le pointeur.</summary>
        public (string Title, string Detail) IconTooltip()
        {
            return (_texts.Get(TextKeys.ToolsTooltip),
                _texts.Get(TextKeys.ToolsTooltipDetail).Replace(TextKeys.KeyParameter, _keys.WheelKey));
        }

        private void OnChosen(ToolChoice choice)
        {
            Switch chosen = _switches[(int)choice];
            if (chosen.IsOn())
            {
                chosen.Off();
                return;
            }
            for (int i = 0; i < _switches.Length; i++)
            {
                if (i != (int)choice && _switches[i].IsOn())
                    _switches[i].Off();
            }
            chosen.On();
        }

        private void OnPointed(ToolChoice? choice)
        {
            if (!choice.HasValue)
            {
                _view.ShowLabel("", "");
                return;
            }
            WheelTool tool = WheelTool.Of(choice.Value);
            _view.ShowLabel(_texts.Get(tool.NameKey), _texts.Get(tool.DetailKey));
        }

        private void ShowActive()
        {
            for (int i = 0; i < _switches.Length; i++)
                _view.ShowActive((ToolChoice)i, _switches[i].IsOn());
        }
    }
}
