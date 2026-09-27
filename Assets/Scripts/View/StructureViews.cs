using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les constructions posées, vues (ARCHITECTURE, section 11) : à l'ouverture, celles qui existent déjà ; ensuite,
    /// chaque construction posée, par l'événement de la simulation, jamais par une photographie. Tant qu'elle attend
    /// d'être bâtie, une construction se voit en fantôme translucide (GDD, section 9).
    /// </summary>
    public sealed class StructureViews : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : le matériau et la couleur du fantôme (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        private readonly List<GameObject> _views = new List<GameObject>();
        private Structures _structures;
        private Relief _relief;

        /// <summary>Montre les constructions de ce monde, puis suit celles qu'on pose.</summary>
        public void Show(Structures structures, Relief relief)
        {
            if (ReferenceEquals(structures, null))
                throw new ArgumentNullException(nameof(structures));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));
            if (_settings == null)
                throw new InvalidOperationException("Les constructions n'ont pas leur fiche de réglages de la pose.");

            Hide();
            _structures = structures;
            _relief = relief;
            foreach (Structure structure in structures.All)
                OnStructurePlaced(structure);
            structures.StructurePlaced += OnStructurePlaced;
        }

        /// <summary>Cesse de suivre le monde et retire ce qu'on montrait.</summary>
        public void Hide()
        {
            if (_structures != null)
                _structures.StructurePlaced -= OnStructurePlaced;
            _structures = null;
            _relief = null;
            foreach (GameObject view in _views)
                Destroy(view);
            _views.Clear();
        }

        private void OnDestroy()
        {
            Hide();
        }

        private void OnStructurePlaced(Structure structure)
        {
            StructureDefinition definition = structure.Definition;
            GameObject view = ModelPose.Instantiate(definition.Model, transform, definition.Id + " n° " + structure.Id);
            ModelPose.Place(view.transform, structure.Placement, _relief);
            new TranslucentModel(view, _settings.TranslucentMaterial).SetColor(_settings.PlannedColor);
            _views.Add(view);
        }
    }
}
