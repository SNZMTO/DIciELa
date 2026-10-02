using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// L'aperçu de la pose (GDD, section 9 ; décision 177) : le modèle en fantôme, celui du chantier (sa forme, feuilles
    /// et tiges comprises), vert si la pose est permise, rouge sinon, posé par la même routine que les constructions. Il
    /// ne décide de rien : l'outil de pose lui dit quoi montrer. Ses copies des fantômes servent d'un aperçu au suivant,
    /// et partent avec lui.
    /// </summary>
    public sealed class PlacementPreview : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Tooltip("La fiche des réglages de la pose : le fantôme du chantier et les couleurs de l'aperçu (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        // Fait au premier usage : Unity refuse d'en créer un pendant la construction d'un composant.
        private MaterialPropertyBlock _block;
        private SiteGhosts _ghosts;
        private StructureDefinition _shown;
        private GameObject _view;
        private TranslucentModel _model;
        private bool _allowed;

        /// <summary>Montre cette construction à cette place, de la couleur de la réponse des règles de pose.</summary>
        public void Show(StructureDefinition definition, Placement placement, Relief relief, bool allowed)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));
            if (_settings == null || _settings.SiteGhostMaterial == null)
                throw new InvalidOperationException("L'aperçu n'a pas sa fiche de réglages de la pose, ou son fantôme.");

            if (!ReferenceEquals(definition, _shown))
            {
                Clear();
                _shown = definition;
                _view = ModelPose.Instantiate(definition.Model, transform, definition.Id);
                _ghosts ??= new SiteGhosts(_settings.SiteGhostMaterial, _settings.PlannedColor, _settings.BlendedCutoff);
                _model = new TranslucentModel(_view, _ghosts);
                Paint(allowed);
            }
            else if (allowed != _allowed)
            {
                Paint(allowed);
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

        private void OnDestroy()
        {
            Clear();
            _ghosts?.Clear();
            _ghosts = null;
        }

        private void Paint(bool allowed)
        {
            _block ??= new MaterialPropertyBlock();
            _block.SetColor(BaseColorId, allowed ? _settings.AllowedColor : _settings.RefusedColor);
            _model.Apply(_block);
            _allowed = allowed;
        }
    }
}
