using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.UI;
using Diciela.View;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// L'écran de création dans l'atelier, le début d'une partie neuve (GDD, section 6) : la Chimère à venir sur son
    /// socle, qu'on fait tourner, et l'écran qui la compose, qu'Entrée valide comme son bouton. Le démarrage le montre
    /// à l'ouverture ; à la validation, il prend le portrait de la Chimère, puis le cache pour ouvrir la ville.
    /// </summary>
    internal sealed class CreationStage
    {
        private readonly CreationScreen _screen;
        private readonly AppearancePreview _preview;
        private readonly Atelier _atelier;
        private readonly PreviewControls _previewControls;
        private readonly ConfirmControls _confirmControls;

        public CreationStage(CreationScreen screen, AppearancePreview preview, Atelier atelier,
            PreviewControls previewControls, ConfirmControls confirmControls)
        {
            _screen = screen != null ? screen : throw new ArgumentNullException(nameof(screen));
            _preview = preview != null ? preview : throw new ArgumentNullException(nameof(preview));
            _atelier = atelier != null ? atelier : throw new ArgumentNullException(nameof(atelier));
            _previewControls = previewControls != null
                ? previewControls
                : throw new ArgumentNullException(nameof(previewControls));
            _confirmControls = confirmControls != null
                ? confirmControls
                : throw new ArgumentNullException(nameof(confirmControls));
        }

        /// <summary>Montre l'atelier et l'écran de création de ce monde ; à la validation, appelle created.</summary>
        public void Show(Catalog catalog, TextTable texts, World world, Action created)
        {
            _atelier.Show();
            _preview.Stand(_atelier.Spot);
            _screen.Show(catalog, texts, _preview, world, created);
            _previewControls.Bind(_preview, _screen);
            _confirmControls.Bind(_screen.Confirm);
        }

        /// <summary>
        /// Le portrait de la Chimère telle qu'on l'a créée, de ce nombre de pixels de côté (décision 154), à prendre
        /// avant de cacher l'atelier ; null sans Chimère montrée.
        /// </summary>
        public Texture2D TakePortrait(int pixels)
        {
            return _preview.TakePortrait(pixels);
        }

        /// <summary>Délie les gestes de l'atelier et cache tout ; accepte de n'avoir rien à cacher.</summary>
        public void Hide()
        {
            _confirmControls.Bind(null);
            _previewControls.Bind(null, null);
            _screen.Hide();
            _preview.Hide();
            _atelier.Hide();
        }
    }
}
