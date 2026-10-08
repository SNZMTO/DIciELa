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
    /// ce qu'elle saute, le fantôme de sa nouvelle place, qu'on ne touche pas. La vue se refait à chaque moment d'un
    /// déplacement, d'après l'état de la construction. En prévision, elle se montre comme si son chantier était fini.
    /// </summary>
    internal sealed class StructureView
    {
        private readonly Transform _parent;
        private readonly Relief _relief;
        private readonly SiteMaterials _materials;
        private readonly SiteGhosts _ghosts;
        private readonly ConstructionSites _sites;
        private GameObject _model;
        private SiteFill _fill;
        private GameObject _target;
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

        /// <summary>
        /// Le modèle qu'on voit, celui qu'on vise pour la toucher : à sa place ; en prévision, celui d'une construction
        /// qu'on déplace, déjà à sa nouvelle place (décision 228).
        /// </summary>
        public Transform Seen(bool foresight)
        {
            return foresight && _target != null ? _target.transform : _model.transform;
        }

        /// <summary>La boîte de ses maillages, dans le repère du modèle ; vide sans maillage : on ne la touche pas.</summary>
        public Bounds LocalBox { get; private set; }

        /// <summary>Refait la vue d'après l'état de la construction : son modèle, et le fantôme de sa nouvelle place.</summary>
        public void Rebuild()
        {
            Forget(true);
            bool site = Structure.State != StructureState.Built;
            _model = Make(Structure.Placement);
            _fill = site ? new SiteFill(_model, _materials, _ghosts) : null;
            ModelPose.TryLocalBox(_model, out Bounds box);
            LocalBox = box;
            bool aiming = Structure.MovingTo.HasValue && !IsAt(Structure.MovingTo.Value);
            _target = aiming ? Make(Structure.MovingTo.Value) : null;
            _targetFill = aiming ? new SiteFill(_target, _materials, _ghosts) : null;
        }

        /// <summary>
        /// La ligne du chantier suit le travail fait ; le fantôme de la nouvelle place reste un fantôme. En prévision,
        /// tout est fini (décision 228) : le chantier bâti, la construction qu'on déplace déjà à sa nouvelle place,
        /// bâtie.
        /// </summary>
        public void Follow(bool foresight)
        {
            if (_fill != null)
            {
                _fill.SetGhostShown(!foresight);
                _fill.Show(foresight ? 1d : Progress());
            }
            if (_targetFill == null)
                return;
            if (_model.activeSelf == foresight)
                _model.SetActive(!foresight);
            _targetFill.SetGhostShown(!foresight);
            _targetFill.Show(foresight ? 1d : 0d);
        }

        /// <summary>Retire ses modèles.</summary>
        public void Forget()
        {
            Forget(false);
        }

        // Refaite, l'ancienne vue sort tout de suite des constructions vues, cachée, avant d'être détruite à la fin de
        // l'image ; retirée avec elles, elle part avec elles.
        private void Forget(bool detach)
        {
            Destroy(_model, detach);
            Destroy(_target, detach);
            _model = null;
            _fill = null;
            _target = null;
            _targetFill = null;
        }

        // Ce qui est fait de son chantier, de 0 à 1 : bâti ou remonté, la ligne monte ; défait, elle descend.
        private double Progress()
        {
            ConstructionSite site = _sites.SiteOf(Structure);
            if (site == null)
                return 0d;
            if (!(site.Work is MoveWork move))
                return site.Progress;
            if (!move.HasLanded)
                return 1d - Mathf.Clamp01((float)(site.WorkDone / move.TakeDownHours));
            return Mathf.Clamp01((float)((site.WorkDone - move.TakeDownHours) / (site.WorkNeeded - move.TakeDownHours)));
        }

        private GameObject Make(Placement placement)
        {
            StructureDefinition definition = Structure.Definition;
            GameObject view = ModelPose.Instantiate(definition.Model, _parent, definition.Id + " n° " + Structure.Id);
            ModelPose.Place(view.transform, placement, _relief);
            return view;
        }

        private bool IsAt(Placement placement)
        {
            return Structure.Placement.Center == placement.Center &&
                   Structure.Placement.RotationDegrees == placement.RotationDegrees;
        }

        private static void Destroy(GameObject view, bool detach)
        {
            if (view == null)
                return;
            if (detach)
            {
                view.SetActive(false);
                view.transform.SetParent(null, false);
            }
            Object.Destroy(view);
        }
    }
}
