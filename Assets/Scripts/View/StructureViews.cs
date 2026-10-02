using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les constructions posées, vues (ARCHITECTURE, section 11) : à l'ouverture, celles qui existent déjà ; ensuite,
    /// chaque construction posée, par l'événement de la simulation, jamais par une photographie. Tant qu'elle attend
    /// d'être bâtie, une construction est un chantier qui monte (GDD, section 9 ; décision 166) : sa vraie matière sous
    /// une ligne qui suit le travail fait, son fantôme au-dessus ; bâtie, le modèle tel quel. On touche une construction
    /// en visant son modèle, pas le sol : vu de biais, le toit d'une maison cache le sol derrière elle.
    /// </summary>
    public sealed class StructureViews : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : les matériaux du chantier et la couleur du fantôme (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        private sealed class Shown
        {
            public Structure Structure;
            public Transform Transform;
            // La boîte de ses maillages, dans le repère du modèle : celle qu'on vise pour le toucher.
            public Bounds LocalBox;
            // Tant qu'elle n'est pas bâtie : sa ligne qui monte, et son chantier, trouvé à son ouverture.
            public SiteFill Fill;
            public ConstructionSite Site;
        }

        private readonly List<Shown> _shown = new List<Shown>();
        private Structures _structures;
        private ConstructionSites _sites;
        private Relief _relief;
        private SiteMaterials _siteMaterials;
        private SiteGhosts _siteGhosts;

        /// <summary>Montre les constructions de ce monde, puis suit celles qu'on pose.</summary>
        public void Show(Structures structures, ConstructionSites sites, Relief relief)
        {
            if (ReferenceEquals(structures, null))
                throw new ArgumentNullException(nameof(structures));
            if (ReferenceEquals(sites, null))
                throw new ArgumentNullException(nameof(sites));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));
            if (_settings == null)
                throw new InvalidOperationException("Les constructions n'ont pas leur fiche de réglages de la pose.");
            if (_settings.SiteMaterial == null || _settings.SiteGhostMaterial == null)
                throw new InvalidOperationException("La fiche des réglages de la pose n'a pas les matériaux du chantier.");

            Hide();
            _structures = structures;
            _sites = sites;
            _relief = relief;
            _siteMaterials = new SiteMaterials(_settings.SiteMaterial, _settings.BlendedCutoff);
            _siteGhosts = new SiteGhosts(_settings.SiteGhostMaterial, _settings.PlannedColor, _settings.BlendedCutoff);
            SiteLine.Publish(_settings);
            foreach (Structure structure in structures.All)
                OnStructurePlaced(structure);
            structures.StructurePlaced += OnStructurePlaced;
            structures.StructureBuilt += OnStructureBuilt;
        }

        /// <summary>Cesse de suivre le monde et retire ce qu'on montrait.</summary>
        public void Hide()
        {
            if (_structures != null)
            {
                _structures.StructurePlaced -= OnStructurePlaced;
                _structures.StructureBuilt -= OnStructureBuilt;
            }
            _structures = null;
            _sites = null;
            _relief = null;
            foreach (Shown shown in _shown)
                Destroy(shown.Transform.gameObject);
            _shown.Clear();
            _siteMaterials?.Clear();
            _siteMaterials = null;
            _siteGhosts?.Clear();
            _siteGhosts = null;
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

        // La ligne de chaque chantier suit son travail fait ; la vague, elle, tourne seule dans le shader.
        private void LateUpdate()
        {
            if (_sites == null)
                return;
            foreach (Shown shown in _shown)
            {
                if (shown.Fill == null)
                    continue;
                if (shown.Site == null)
                    shown.Site = _sites.SiteOf(shown.Structure);
                shown.Fill.Show(shown.Site != null ? shown.Site.Progress : 0d);
            }
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
            SiteFill fill = structure.State == StructureState.Planned
                ? new SiteFill(view, definition.Model, _siteMaterials, _siteGhosts)
                : null;
            _shown.Add(new Shown
            {
                Structure = structure, Transform = view.transform, LocalBox = BoxOf(view), Fill = fill,
                Site = _sites.SiteOf(structure),
            });
        }

        // La boîte qu'on vise pour toucher ce modèle ; sans maillage, vide : on ne le touche pas.
        private static Bounds BoxOf(GameObject view)
        {
            ModelPose.TryLocalBox(view, out Bounds box);
            return box;
        }

        /// <summary>Son chantier est fini : le chantier part, le modèle bâti prend sa place.</summary>
        private void OnStructureBuilt(Structure structure)
        {
            for (int i = 0; i < _shown.Count; i++)
            {
                if (!ReferenceEquals(_shown[i].Structure, structure))
                    continue;
                Destroy(_shown[i].Transform.gameObject);
                _shown.RemoveAt(i);
                break;
            }
            OnStructurePlaced(structure);
        }
    }
}
