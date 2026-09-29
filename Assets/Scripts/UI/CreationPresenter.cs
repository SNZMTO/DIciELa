using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using Diciela.View;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de l'écran de création (GDD, section 6 ; ARCHITECTURE, section 11) : il relie la vue à ses choix
    /// (CreationChoices) et à la table des textes, habille l'aperçu à chaque choix, et, à la validation, envoie l'ordre
    /// « créer la Chimère » avec l'apparence choisie, une seule fois, puis le dit.
    /// </summary>
    public sealed class CreationPresenter : IDisposable
    {
        private readonly ICreationView _view;
        private readonly Catalog _catalog;
        private readonly TextTable _texts;
        private readonly AppearancePreview _preview;
        private readonly World _world;
        private readonly Action _created;
        private readonly CreationChoices _choices;
        private bool _confirmed;

        public CreationPresenter(ICreationView view, Catalog catalog, TextTable texts, AppearancePreview preview,
            World world, Action created)
        {
            if (view == null || catalog == null || catalog.ChimeraKind == null || ReferenceEquals(texts, null) ||
                preview == null || ReferenceEquals(world, null))
                throw new ArgumentException("L'écran de création a besoin de sa vue, du catalogue et de la sorte de la " +
                                            "Chimère, des textes, de l'aperçu et du monde.");

            _view = view;
            _catalog = catalog;
            _texts = texts;
            _preview = preview;
            _world = world;
            _created = created;
            _choices = new CreationChoices(catalog);

            var labels = new List<string>();
            for (int row = 0; row < _choices.Rows; row++)
                labels.Add(texts.Get(_choices.LabelKey(row)));
            _view.Stepped += OnStepped;
            _view.Recolored += OnRecolored;
            _view.Confirmed += OnConfirmed;
            _view.ShowRows(texts.Get(TextKeys.CreationTitle), texts.Get(TextKeys.CreationConfirm),
                texts.Get(TextKeys.CreationPrevious), texts.Get(TextKeys.CreationNext), labels);
            Show();
        }

        /// <summary>L'apparence choisie en ce moment.</summary>
        public Appearance Current => _choices.Current();

        public void Dispose()
        {
            _view.Stepped -= OnStepped;
            _view.Recolored -= OnRecolored;
            _view.Confirmed -= OnConfirmed;
        }

        private void OnStepped(int row, int direction)
        {
            if (_confirmed)
                return;
            _choices.Step(row, direction);
            Show();
        }

        private void OnRecolored(int row, int direction)
        {
            if (_confirmed)
                return;
            _choices.Recolor(row, direction);
            Show();
        }

        private void OnConfirmed()
        {
            if (_confirmed)
                return;
            _confirmed = true;
            _world.Submit(new CreateChimeraCommand(_catalog.ChimeraKind, Current));
            _created?.Invoke();
        }

        private void Show()
        {
            for (int row = 0; row < _choices.Rows; row++)
            {
                string piece = _choices.PieceKey(row);
                string palette = _choices.PaletteKey(row);
                _view.ShowChoice(row, piece.Length > 0 ? _texts.Get(piece) : _texts.Get(TextKeys.CreationNone),
                    palette.Length > 0 ? _texts.Get(palette) : "");
            }
            _preview.Show(Current);
        }
    }
}
