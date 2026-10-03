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
    /// une ligne qui suit le travail fait, son fantôme au-dessus ; bâtie, le modèle tel quel. Déplacée, elle montre sa
    /// nouvelle place en fantôme, se défait, saute, puis se remonte (décision 201) : chaque moment refait sa vue
    /// (StructureView). Un sol qu'on traverse, comme la Place, épouse le relief (FloorDrape). On touche une construction
    /// en visant son modèle, pas le sol : vu de biais, le toit d'une maison cache le sol derrière elle.
    /// </summary>
    public sealed class StructureViews : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : les matériaux du chantier et la couleur du fantôme (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _settings;

        private readonly List<StructureView> _shown = new List<StructureView>();
        private Structures _structures;
        private ConstructionSites _sites;
        private Relief _relief;
        private SiteMaterials _siteMaterials;
        private SiteGhosts _siteGhosts;

        /// <summary>Montre les constructions de ce monde, puis suit celles qu'on pose, qu'on bâtit ou qu'on déplace.</summary>
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
            structures.StructureBuilt += Rebuild;
            structures.MoveStarted += Rebuild;
            structures.StructureTakenDown += Rebuild;
            structures.StructureMoved += Rebuild;
        }

        /// <summary>Cesse de suivre le monde et retire ce qu'on montrait.</summary>
        public void Hide()
        {
            if (_structures != null)
            {
                _structures.StructurePlaced -= OnStructurePlaced;
                _structures.StructureBuilt -= Rebuild;
                _structures.MoveStarted -= Rebuild;
                _structures.StructureTakenDown -= Rebuild;
                _structures.StructureMoved -= Rebuild;
            }
            _structures = null;
            _sites = null;
            _relief = null;
            foreach (StructureView shown in _shown)
                shown.Forget();
            _shown.Clear();
            _siteMaterials?.Clear();
            _siteMaterials = null;
            _siteGhosts?.Clear();
            _siteGhosts = null;
        }

        /// <summary>
        /// La construction dont le modèle est touché le premier par ce rayon (en général, celui du pointeur) ; faux si le
        /// rayon n'en touche aucune. Le fantôme d'une nouvelle place ne se touche pas.
        /// </summary>
        public bool TryPick(Ray ray, out Structure picked)
        {
            picked = null;
            float nearest = float.PositiveInfinity;
            foreach (StructureView shown in _shown)
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
            foreach (StructureView shown in _shown)
                shown.Follow();
        }

        private void OnDestroy()
        {
            Hide();
        }

        private void OnStructurePlaced(Structure structure)
        {
            _shown.Add(new StructureView(structure, transform, _relief, _siteMaterials, _siteGhosts, _sites));
        }

        /// <summary>Un moment de sa vie : bâtie, déplacée, défaite, sautée à sa nouvelle place ; sa vue se refait.</summary>
        private void Rebuild(Structure structure)
        {
            foreach (StructureView shown in _shown)
            {
                if (ReferenceEquals(shown.Structure, structure))
                    shown.Rebuild();
            }
        }
    }
}
