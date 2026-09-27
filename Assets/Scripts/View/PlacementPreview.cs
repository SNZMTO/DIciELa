using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// L'aperçu de la pose (GDD, section 9) : une copie translucide du modèle, verte si la pose est permise, rouge sinon,
    /// posée par la même routine que les constructions. Il ne décide de rien : l'outil de pose lui dit quoi montrer.
    /// </summary>
    public sealed class PlacementPreview : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : le matériau et les couleurs de l'aperçu (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        private StructureDefinition _shown;
        private GameObject _view;
        private TranslucentModel _model;
        private bool _allowed;

        /// <summary>Montre cette construction à cette place, de la couleur de la réponse des règles de pose.</summary>
        public void Show(StructureDefinition definition, Placement placement, Relief relief, bool allowed)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));
            if (_settings == null)
                throw new InvalidOperationException("L'aperçu n'a pas sa fiche de réglages de la pose.");

            if (!ReferenceEquals(definition, _shown))
            {
                Clear();
                _shown = definition;
                _view = ModelPose.Instantiate(definition.Model, transform, definition.Id);
                _model = new TranslucentModel(_view, _settings.TranslucentMaterial);
                _model.SetColor(ColorFor(allowed));
                _allowed = allowed;
            }
            else if (allowed != _allowed)
            {
                _model.SetColor(ColorFor(allowed));
                _allowed = allowed;
            }

            ModelPose.Place(_view.transform, placement, relief);
            _view.SetActive(true);
        }

        /// <summary>Cache l'aperçu sans l'oublier : le pointeur a quitté le sol.</summary>
        public void Hide()
        {
            if (_view != null)
                _view.SetActive(false);
        }

        /// <summary>Retire l'aperçu : la pose est finie ou annulée.</summary>
        public void Clear()
        {
            if (_view != null)
                Destroy(_view);
            _view = null;
            _model = null;
            _shown = null;
        }

        private Color ColorFor(bool allowed)
        {
            return allowed ? _settings.AllowedColor : _settings.RefusedColor;
        }
    }
}
