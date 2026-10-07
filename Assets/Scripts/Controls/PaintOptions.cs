using System;
using Diciela.Data;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Les réglages d'un outil de sol qui ne tiennent à aucun geste (décision 216), ceux de la rangée d'options de sa
    /// barre : la forme du mode Forme, le style d'un tracé ou d'un polygone (droit ou en courbe), la symétrie (son
    /// épingle, ses axes, le nombre de l'étoile), la taille du pinceau (dans ses bornes, par crans), et le cap de la place
    /// de charrette, qu'on tourne comme une construction à la pose. Ce que la barre choisit se choisit ici, l'outil en
    /// main ; l'outil s'y fait (il abandonne le geste, le redessine, pose l'épingle) et le dit. Le pinceau, le cap et
    /// l'épingle, eux, ne changent que par les gestes de l'outil.
    /// </summary>
    public sealed class PaintOptions
    {
        private readonly PlacementSettings _settings;
        private readonly Func<bool> _active;
        private readonly Action<PaintOptionEffect> _changed;

        internal PaintOptions(PlacementSettings settings, float startRadius, Func<bool> active,
            Action<PaintOptionEffect> changed)
        {
            _settings = settings;
            _active = active;
            _changed = changed;
            Vector2 range = settings.BrushRadiusRangeMeters;
            BrushRadius = Mathf.Clamp(startRadius, range.x, range.y);
            StarBranches = settings.StartStarBranches;
        }

        /// <summary>La forme du mode Forme.</summary>
        public PaintShape Shape { get; private set; }

        /// <summary>Comment un tracé et un polygone relient leurs points.</summary>
        public PaintTraceStyle Style { get; private set; }

        /// <summary>La symétrie de ce qu'il trace, gomme ou pose.</summary>
        public PaintSymmetry Symmetry { get; private set; }

        /// <summary>Les branches de l'étoile : la forme et ses copies.</summary>
        public int StarBranches { get; private set; }

        /// <summary>Le moins et le plus de branches de l'étoile.</summary>
        public Vector2Int StarBranchesRange => _settings.StarBranchesRange;

        /// <summary>Vrai une fois l'épingle posée ; avant, la première symétrie la pose où il faut.</summary>
        public bool HasPin { get; private set; }

        /// <summary>L'épingle : le centre des symétries, en mètres au sol.</summary>
        public Vector2 PinCenter { get; private set; }

        /// <summary>Le cap du premier axe des miroirs, en degrés.</summary>
        public float AxisDegrees { get; private set; }

        /// <summary>Le rayon du pinceau, de la gomme et du tracé, en mètres.</summary>
        public float BrushRadius { get; private set; }

        /// <summary>Le cap de la place de charrette en main, en degrés, vers l'est depuis le nord.</summary>
        public float HeadingDegrees { get; private set; }

        /// <summary>Prend cette forme ; le geste en cours est abandonné.</summary>
        public void Choose(PaintShape shape)
        {
            if (!_active() || shape == Shape)
                return;
            Shape = shape;
            _changed(PaintOptionEffect.Abandon);
        }

        /// <summary>Relie les points d'un tracé ou d'un polygone droit, ou en courbe ; celui qu'on trace se redessine.</summary>
        public void Choose(PaintTraceStyle style)
        {
            if (!_active() || style == Style)
                return;
            Style = style;
            _changed(PaintOptionEffect.Restyle);
        }

        /// <summary>Prend cette symétrie ; la première pose l'épingle.</summary>
        public void Choose(PaintSymmetry symmetry)
        {
            if (!_active() || symmetry == Symmetry)
                return;
            Symmetry = symmetry;
            _changed(PaintOptionEffect.PlacePin);
        }

        /// <summary>Donne à l'étoile tant de branches, la forme comprise, dans leurs bornes.</summary>
        public void ChooseStar(int branches)
        {
            Vector2Int range = _settings.StarBranchesRange;
            branches = Mathf.Clamp(branches, range.x, range.y);
            if (!_active() || branches == StarBranches)
                return;
            StarBranches = branches;
            _changed(PaintOptionEffect.Refresh);
        }

        /// <summary>Pose l'épingle ici, son premier axe à ce cap.</summary>
        internal void PlacePin(Vector2 center, float axisDegrees)
        {
            HasPin = true;
            PinCenter = center;
            AxisDegrees = Mathf.Repeat(axisDegrees, 360f);
        }

        /// <summary>Déplace l'épingle ici.</summary>
        internal void MovePin(Vector2 center)
        {
            PinCenter = center;
        }

        /// <summary>
        /// Tourne de cet angle, en degrés : la place de charrette, ou, hors de ce mode, les axes d'une symétrie.
        /// </summary>
        internal void Turn(float degrees, bool cartSpot)
        {
            if (!cartSpot && Symmetry != PaintSymmetry.None)
                AxisDegrees = Mathf.Repeat(AxisDegrees + degrees, 360f);
            else
                HeadingDegrees = Mathf.Repeat(HeadingDegrees + degrees, 360f);
        }

        /// <summary>Grandit (positif) ou rapetisse le pinceau d'autant de crans, dans ses bornes ; vrai s'il change.</summary>
        internal bool Resize(int notches)
        {
            Vector2 range = _settings.BrushRadiusRangeMeters;
            float radius = Mathf.Clamp(BrushRadius + notches * _settings.BrushRadiusStepMeters, range.x, range.y);
            if (Mathf.Approximately(radius, BrushRadius))
                return false;
            BrushRadius = radius;
            return true;
        }
    }
}
