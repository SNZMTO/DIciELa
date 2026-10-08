using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// L'aperçu d'un outil de sol (décisions 206, 216 et 227) : les mailles que le geste ajouterait ou retirerait,
    /// lissées, le tour de la forme (le cercle du pinceau, le disque, le rectangle, la place de charrette), vert si le
    /// geste se fait, rouge sinon, comme l'aperçu de la pose, avec les copies d'une place de charrette par symétrie ;
    /// les points posés d'un tracé ou d'un polygone, un rond chacun, qu'on prend pour les déplacer ; et l'épingle d'une
    /// symétrie, avec ses axes. Il ne décide de rien : l'outil lui dit quoi montrer. Les mailles ne se refont que quand
    /// l'outil les redemande, le tour que quand il change.
    /// </summary>
    public sealed class PaintToolPreview : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : le matériau translucide, les couleurs de l'aperçu, les lignes de la Place.")]
        [SerializeField] private PlacementSettings _settings;

        private readonly DrapedMesh _built = new DrapedMesh();
        private readonly CellField _field = new CellField();
        private readonly ContourMesh _fill = new ContourMesh();
        private GroundOverlay _cells;
        private GroundOverlay _outline;
        private GroundOverlay _handles;
        private GroundOverlay _pin;
        private readonly List<PaintOutline> _shownOutlines = new List<PaintOutline>();
        private bool _outlineAllowed;
        // Ce que montrent les points et l'épingle, en nombres à la suite, et ce qu'on leur demande : rien ne se refait
        // à l'identique.
        private readonly List<float> _shownHandles = new List<float>();
        private readonly List<float> _shownPin = new List<float>();
        private readonly List<float> _asked = new List<float>();

        /// <summary>Montre ces mailles, de la couleur de la réponse ; aucune maille les cache.</summary>
        public void ShowCells(NavGrid grid, Relief relief, IReadOnlyList<Vector2Int> cells, bool allowed)
        {
            EnsureOverlays();
            _cells.SetColor(allowed ? _settings.AllowedColor : _settings.RefusedColor);
            float size = grid.Metrics.CellSize;
            _field.Build(cells, size, CellField.RadiusCells(_settings.GroundRoundingMeters, size));
            _built.Begin(relief, _settings.OverlayLiftMeters);
            _fill.AddFill(_built, _field);
            _cells.Apply(_built);
            _cells.SetVisible(cells.Count > 0);
        }

        /// <summary>Montre ces tours, de la couleur de la réponse ; les mêmes tours ne sont pas refaits.</summary>
        public void ShowOutlines(Relief relief, IReadOnlyList<PaintOutline> outlines, bool allowed)
        {
            EnsureOverlays();
            Color color = allowed ? _settings.AllowedColor : _settings.RefusedColor;
            color.a = _settings.OutlineColor.a;
            _outline.SetColor(color);
            _outline.SetVisible(outlines.Count > 0 && outlines[0].Shape != PaintOutline.Kind.None);
            if (SameAsShown(outlines) && allowed == _outlineAllowed)
                return;
            _shownOutlines.Clear();
            _shownOutlines.AddRange(outlines);
            _outlineAllowed = allowed;
            float width = _settings.OutlineWidthMeters;
            // Les lignes un peu au-dessus des mailles, qu'elles ne s'y mêlent pas.
            _built.Begin(relief, _settings.OverlayLiftMeters * 2f);
            foreach (PaintOutline outline in outlines)
            {
                if (outline.Shape == PaintOutline.Kind.Circle)
                    _built.AddCircle(outline.Center, outline.Radius, width);
                else if (outline.Shape == PaintOutline.Kind.Box)
                    _built.AddOutline(outline.Rect, width);
                else if (outline.Shape == PaintOutline.Kind.Mark)
                    _built.AddMark(outline.Rect, width);
            }
            _outline.Apply(_built);
        }

        /// <summary>
        /// Montre les points posés d'un tracé ou d'un polygone, chacun un rond ; aucun point les cache ; les mêmes points ne
        /// sont pas refaits.
        /// </summary>
        public void ShowHandles(Relief relief, IReadOnlyList<Vector2> points)
        {
            EnsureOverlays();
            _handles.SetVisible(points.Count > 0);
            _asked.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                _asked.Add(points[i].x);
                _asked.Add(points[i].y);
            }
            if (points.Count == 0 || !Changed(_shownHandles))
                return;
            _handles.SetColor(_settings.OutlineColor);
            // Les ronds au-dessus des lignes, qu'on les voie sur le tour.
            _built.Begin(relief, _settings.OverlayLiftMeters * 3f);
            for (int i = 0; i < points.Count; i++)
                _built.AddCircle(points[i], _settings.HandleRadiusMeters, _settings.OutlineWidthMeters);
            _handles.Apply(_built);
        }

        /// <summary>
        /// Montre l'épingle d'une symétrie : un rond à son centre, et ses axes de part et d'autre, ou ses rayons d'un seul
        /// côté (l'étoile) ; cachée sinon ; la même épingle n'est pas refaite.
        /// </summary>
        public void ShowPin(Relief relief, bool shown, Vector2 center, IReadOnlyList<float> axisDegrees, bool rays)
        {
            EnsureOverlays();
            _pin.SetVisible(shown);
            _asked.Clear();
            _asked.Add(center.x);
            _asked.Add(center.y);
            _asked.Add(rays ? 1f : 0f);
            for (int i = 0; i < axisDegrees.Count; i++)
                _asked.Add(axisDegrees[i]);
            if (!shown || !Changed(_shownPin))
                return;
            _pin.SetColor(_settings.OutlineColor);
            float width = _settings.OutlineWidthMeters;
            _built.Begin(relief, _settings.OverlayLiftMeters * 3f);
            _built.AddCircle(center, _settings.HandleRadiusMeters, width);
            foreach (float degrees in axisDegrees)
            {
                Vector2 along = Heading.Direction(degrees) * _settings.PinAxisMeters;
                _built.AddSegment(rays ? center : center - along, center + along, width);
            }
            _pin.Apply(_built);
        }

        /// <summary>Cache tout l'aperçu : l'outil n'est plus en main, ou ne vise plus le sol.</summary>
        public void Hide()
        {
            _cells?.SetVisible(false);
            _outline?.SetVisible(false);
            _handles?.SetVisible(false);
            _pin?.SetVisible(false);
            _shownOutlines.Clear();
        }

        private void OnDestroy()
        {
            _cells?.Destroy();
            _outline?.Destroy();
            _handles?.Destroy();
            _pin?.Destroy();
            _cells = null;
            _outline = null;
            _handles = null;
            _pin = null;
        }

        private void EnsureOverlays()
        {
            if (_cells != null)
                return;
            if (_settings == null || _settings.TranslucentMaterial == null)
                throw new System.InvalidOperationException("L'aperçu de la Place n'a pas sa fiche de réglages de la pose.");
            _cells = new GroundOverlay("Aperçu des mailles", transform, _settings.TranslucentMaterial, 2);
            _outline = new GroundOverlay("Aperçu du tour", transform, _settings.TranslucentMaterial, 3);
            _handles = new GroundOverlay("Aperçu des points", transform, _settings.TranslucentMaterial, 4);
            _pin = new GroundOverlay("Épingle de la symétrie", transform, _settings.TranslucentMaterial, 4);
        }

        // Vrai si ce qu'on demande n'est pas ce qui est montré, qui le devient.
        private bool Changed(List<float> shown)
        {
            bool same = shown.Count == _asked.Count;
            for (int i = 0; same && i < shown.Count; i++)
                same = Mathf.Approximately(shown[i], _asked[i]);
            if (same)
                return false;
            shown.Clear();
            shown.AddRange(_asked);
            return true;
        }

        private bool SameAsShown(IReadOnlyList<PaintOutline> outlines)
        {
            if (outlines.Count != _shownOutlines.Count)
                return false;
            for (int i = 0; i < outlines.Count; i++)
            {
                if (!Same(outlines[i], _shownOutlines[i]))
                    return false;
            }
            return true;
        }

        private static bool Same(PaintOutline a, PaintOutline b)
        {
            return a.Shape == b.Shape && a.Center == b.Center && Mathf.Approximately(a.Radius, b.Radius) &&
                   a.Rect.Size == b.Rect.Size && Mathf.Approximately(a.Rect.RotationDegrees, b.Rect.RotationDegrees);
        }
    }
}
