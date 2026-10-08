using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using Diciela.View;
using UnityEngine.UIElements;

namespace Diciela.Game
{
    /// <summary>
    /// La prévision de la ville (GDD, section 16 ; décisions 228 à 230) : V ou son bouton, au-dessus de l'icône des
    /// outils, la basculent, et Échap l'éteint, en dernier dans sa pile ; en prévision, le monde se voit tel qu'il sera
    /// une fois tous ses chantiers finis (WorldViews), sous le voile de rêverie (ForesightVeil). Le temps continue, et
    /// l'on y construit comme ailleurs. Branchée à l'ouverture de la ville ; à sa sortie, elle s'éteint et se
    /// débranche.
    /// </summary>
    internal sealed class TownForesight : IDisposable
    {
        private readonly Foresight _foresight = new Foresight();
        private readonly WorldViews _views;
        private readonly ForesightControls _controls;
        private readonly ForesightButton _button;
        private readonly ForesightVeil _veil;

        public TownForesight(WorldViews views, ForesightControls controls, UIDocument document, TextTable texts,
            Tooltips tooltips)
        {
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _controls = controls != null ? controls : throw new ArgumentNullException(nameof(controls));
            _button = new ForesightButton(document);
            _veil = new ForesightVeil(document);
            _foresight.Changed += OnChanged;
            _controls.Bind(_foresight);
            _button.Show(_foresight, texts, tooltips, _controls.Key);
            _veil.Show(_foresight);
        }

        /// <summary>
        /// Éteint la prévision (le monde se revoit tel qu'il est), puis débranche son geste, son bouton et son voile.
        /// </summary>
        public void Dispose()
        {
            _foresight.Set(false);
            _foresight.Changed -= OnChanged;
            _button.Hide();
            _veil.Hide();
            _controls.Bind(null);
        }

        private void OnChanged()
        {
            _views.ShowForesight(_foresight.IsOn);
        }
    }
}
