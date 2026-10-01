using System;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la roue des outils (GDD, section 16 ; décision 168). Construire ouvre le panneau Construire, en
    /// posant l'outil « Retirer » s'il était en main, ou le ferme s'il est ouvert. Retirer ferme le panneau et prend
    /// l'outil en main, ou le lâche s'il l'était. La bulle du milieu dit le nom et l'usage de l'outil visé ; un outil
    /// actif reste enfoncé. Les touches se lisent sur les liaisons, l'icône dit celle de la roue dans son infobulle.
    /// </summary>
    public sealed class ToolWheelPresenter : IDisposable
    {
        private readonly IToolWheelView _view;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly ClearingTool _clearing;
        private readonly IBuildMenu _build;

        public ToolWheelPresenter(IToolWheelView view, TextTable texts, IToolKeys keys, ClearingTool clearing,
            IBuildMenu build)
        {
            if (view == null || ReferenceEquals(texts, null) || keys == null || ReferenceEquals(clearing, null) ||
                build == null)
                throw new ArgumentException("La roue des outils a besoin de sa vue, des textes, des touches, de " +
                                            "l'outil « Retirer » et du panneau Construire.");

            _view = view;
            _texts = texts;
            _keys = keys;
            _clearing = clearing;
            _build = build;
            _view.Chosen += OnChosen;
            _view.Pointed += OnPointed;
            _clearing.ActiveChanged += ShowActive;
            _build.OpenChanged += ShowActive;
            _view.ShowKeys(keys.WheelKey, keys.KeyOf(ToolChoice.Build), keys.KeyOf(ToolChoice.Clear));
            _view.ShowLabel("", "");
            ShowActive();
        }

        public void Dispose()
        {
            _view.Chosen -= OnChosen;
            _view.Pointed -= OnPointed;
            _clearing.ActiveChanged -= ShowActive;
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
            if (choice == ToolChoice.Build)
            {
                if (_build.IsOpen)
                {
                    _build.Close();
                    return;
                }
                _clearing.Escape();
                _build.Open();
                return;
            }

            _build.Close();
            if (!_clearing.Escape())
                _clearing.Take();
        }

        private void OnPointed(ToolChoice? choice)
        {
            if (!choice.HasValue)
            {
                _view.ShowLabel("", "");
                return;
            }
            bool build = choice.Value == ToolChoice.Build;
            _view.ShowLabel(_texts.Get(build ? TextKeys.BuildTool : TextKeys.ClearingTool),
                _texts.Get(build ? TextKeys.BuildToolDetail : TextKeys.ClearingToolDetail));
        }

        private void ShowActive()
        {
            _view.ShowActive(_build.IsOpen, _clearing.IsActive);
        }
    }
}
