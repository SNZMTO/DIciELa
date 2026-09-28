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
    /// d'être bâtie, une construction se voit en fantôme translucide (GDD, section 9) ; bâtie, avec ses matériaux. On
    /// touche une construction en visant son modèle, pas le sol : vu de biais, le toit d'une maison cache le sol
    /// derrière elle.
    /// </summary>
    public sealed class StructureViews : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : le matériau et la couleur du fantôme (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        private sealed class Shown
        {
            public Structure Structure;
            public Transform Transform;
            // La boîte de ses maillages, dans le repère du modèle : celle qu'on vise pour le toucher.
            public Bounds LocalBox;
        }

        private readonly List<Shown> _shown = new List<Shown>();
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
            foreach (Shown shown in _shown)
                Destroy(shown.Transform.gameObject);
            _shown.Clear();
        }

        /// <summary>
        /// La construction dont le modèle est touché le premier par ce rayon (en général, celui du pointeur) ; faux si le
        /// rayon n'en touche aucune.
        /// </summary>
        public bool TryPick(Ray ray, out Structure picked)
        {
            picked = null;
            float nearest = float.PositiveInfinity;
            foreach (Shown shown in _shown)
            {
                // Les modèles ne sont ni agrandis ni réduits : les distances restent en mètres, comparables entre elles.
                var local = new Ray(shown.Transform.InverseTransformPoint(ray.origin),
                    shown.Transform.InverseTransformDirection(ray.direction));
                if (shown.LocalBox.IntersectRay(local, out float distance) && distance < nearest)
                {
                    nearest = distance;
                    picked = shown.Structure;
                }
            }
            return picked != null;
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
            if (structure.State == StructureState.Planned)
                new TranslucentModel(view, _settings.TranslucentMaterial).SetColor(_settings.PlannedColor);
            _shown.Add(new Shown { Structure = structure, Transform = view.transform, LocalBox = LocalBox(view) });
        }

        /// <summary>La boîte des maillages de ce modèle, dans son propre repère.</summary>
        private static Bounds LocalBox(GameObject model)
        {
            var box = new Bounds();
            bool any = false;
            Matrix4x4 toModel = model.transform.worldToLocalMatrix;
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null)
                    continue;
                Matrix4x4 toLocal = toModel * filter.transform.localToWorldMatrix;
                Bounds mesh = filter.sharedMesh.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    var offset = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f,
                        (corner & 4) == 0 ? -1f : 1f);
                    Vector3 point = toLocal.MultiplyPoint3x4(mesh.center + Vector3.Scale(mesh.extents, offset));
                    if (any)
                        box.Encapsulate(point);
                    else
                        box = new Bounds(point, Vector3.zero);
                    any = true;
                }
            }
            return box;
        }
    }
}
