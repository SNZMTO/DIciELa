using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using Diciela.View;
using UnityEngine.UIElements;

namespace Diciela.Game
{
    /// <summary>
    /// La prévision de la ville (GDD, section 16 ; décision 228) : V ou son bouton, au-dessus de l'icône des outils, la
    /// basculent ; en prévision, le monde se voit tel qu'il sera une fois tous ses chantiers finis (WorldViews), sous
    /// le filtre de rêverie (ForesightFilter). Le temps continue, et l'on y construit comme ailleurs. Branchée à
    /// l'ouverture de la ville ; à sa sortie, elle s'éteint et se débranche.
    /// </summary>
    internal sealed class TownForesight : IDisposable
    {
        private readonly Foresight _foresight = new Foresight();
        private readonly WorldViews _views;
        private readonly ForesightFilter _filter;
        private readonly ForesightControls _controls;
        private readonly ForesightButton _button;

        public TownForesight(WorldViews views, ForesightFilter filter, ForesightControls controls, UIDocument document,
            TextTable texts, Tooltips tooltips)
        {
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _filter = filter != null ? filter : throw new ArgumentNullException(nameof(filter));
            _controls = controls != null ? controls : throw new ArgumentNullException(nameof(controls));
            _button = new ForesightButton(document);
            _foresight.Changed += OnChanged;
            _filter.Show(_foresight);
            _controls.Bind(_foresight);
            _button.Show(_foresight, texts, tooltips, _controls.Key);
        }

        /// <summary>
        /// Éteint la prévision (le monde se revoit tel qu'il est), puis débranche son geste, son bouton et son filtre.
        /// </summary>
        public void Dispose()
        {
            _foresight.Set(false);
            _foresight.Changed -= OnChanged;
            _button.Hide();
            _controls.Bind(null);
            _filter.Hide();
        }

        private void OnChanged()
        {
            _views.ShowForesight(_foresight.IsOn);
        }
    }
}
