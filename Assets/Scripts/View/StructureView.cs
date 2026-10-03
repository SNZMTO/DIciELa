using Diciela.Data;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La vue d'une construction (ARCHITECTURE, section 11 ; décisions 166 et 201) : son modèle à sa place, tel quel quand
    /// elle sert ; un chantier dont la ligne monte avec le travail fait, quand elle attend d'être bâtie ou qu'on la remonte
    /// à sa nouvelle place ; un chantier dont la ligne descend, pendant qu'on la défait. Pendant un déplacement, jusqu'à
    /// ce qu'elle saute, le fantôme de sa nouvelle place, qu'on ne touche pas. Un sol qu'on traverse épouse le relief, là
    /// où il est. La vue se refait à chaque moment d'un déplacement, d'après l'état de la construction.
    /// </summary>
    internal sealed class StructureView
    {
        private readonly Transform _parent;
        private readonly Relief _relief;
        private readonly SiteMaterials _materials;
        private readonly SiteGhosts _ghosts;
        private readonly ConstructionSites _sites;
        private GameObject _model;
        private Mesh[] _modelDraped;
        private SiteFill _fill;
        private GameObject _target;
        private Mesh[] _targetDraped;
        private SiteFill _targetFill;

        public StructureView(Structure structure, Transform parent, Relief relief, SiteMaterials materials,
            SiteGhosts ghosts, ConstructionSites sites)
        {
            Structure = structure;
            _parent = parent;
            _relief = relief;
            _materials = materials;
            _ghosts = ghosts;
            _sites = sites;
            Rebuild();
        }

        public Structure Structure { get; }

        /// <summary>Le modèle à sa place : celui qu'on vise pour la toucher.</summary>
        public Transform Transform => _model.transform;

        /// <summary>La boîte de ses maillages, dans le repère du modèle ; vide sans maillage : on ne la touche pas.</summary>
        public Bounds LocalBox { get; private set; }

        /// <summary>Refait la vue d'après l'état de la construction : son modèle, et le fantôme de sa nouvelle place.</summary>
        public void Rebuild()
        {
            Forget(true);
            bool site = Structure.State != StructureState.Built;
            _model = Make(Structure.Placement, out _modelDraped);
            _fill = site ? new SiteFill(_model, _materials, _ghosts) : null;
            ModelPose.TryLocalBox(_model, out Bounds box);
            LocalBox = box;
            bool aiming = Structure.MovingTo.HasValue && !IsAt(Structure.MovingTo.Value);
            _target = aiming ? Make(Structure.MovingTo.Value, out _targetDraped) : null;
            _targetFill = aiming ? new SiteFill(_target, _materials, _ghosts) : null;
        }

        /// <summary>La ligne du chantier suit le travail fait ; le fantôme de la nouvelle place reste un fantôme.</summary>
        public void Follow()
        {
            _fill?.Show(Progress());
            _targetFill?.Show(0d);
        }

        /// <summary>Retire ses modèles, et les maillages copiés pour eux.</summary>
        public void Forget()
        {
            Forget(false);
        }

        // Refaite, l'ancienne vue sort tout de suite des constructions vues, cachée, avant d'être détruite à la fin de
        // l'image ; retirée avec elles, elle part avec elles.
        private void Forget(bool detach)
        {
            Destroy(_model, _modelDraped, detach);
            Destroy(_target, _targetDraped, detach);
            _model = null;
            _modelDraped = null;
            _fill = null;
            _target = null;
            _targetDraped = null;
            _targetFill = null;
        }

        // Ce qui est fait de son chantier, de 0 à 1 : bâti ou remonté, la ligne monte ; défait, elle descend.
        private double Progress()
        {
            ConstructionSite site = _sites.SiteOf(Structure);
            if (site == null)
                return 0d;
            if (!site.IsMove)
                return site.Progress;
            if (!site.HasLanded)
                return 1d - Mathf.Clamp01((float)(site.WorkDone / site.TakeDownHours));
            return Mathf.Clamp01((float)((site.WorkDone - site.TakeDownHours) / (site.WorkNeeded - site.TakeDownHours)));
        }

        private GameObject Make(Placement placement, out Mesh[] draped)
        {
            StructureDefinition definition = Structure.Definition;
            GameObject view = ModelPose.Instantiate(definition.Model, _parent, definition.Id + " n° " + Structure.Id);
            ModelPose.Place(view.transform, placement, _relief);
            draped = definition.IsWalkable ? FloorDrape.Apply(view, _relief) : null;
            return view;
        }

        private bool IsAt(Placement placement)
        {
            return Structure.Placement.Center == placement.Center &&
                   Structure.Placement.RotationDegrees == placement.RotationDegrees;
        }

        private static void Destroy(GameObject view, Mesh[] draped, bool detach)
        {
            if (view != null)
            {
                if (detach)
                {
                    view.SetActive(false);
                    view.transform.SetParent(null, false);
                }
                Object.Destroy(view);
            }
            if (draped == null)
                return;
            foreach (Mesh mesh in draped)
            {
                if (mesh != null)
                    Object.Destroy(mesh);
            }
        }
    }
}
