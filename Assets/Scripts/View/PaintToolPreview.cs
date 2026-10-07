using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// L'aperçu de l'outil de la Place (décision 206) : les mailles que le geste ajouterait ou retirerait, et le tour de
    /// la forme (le cercle du pinceau, le disque, le rectangle, la place de charrette), vert si le geste se fait, rouge
    /// sinon, comme l'aperçu de la pose. Il ne décide de rien : l'outil lui dit quoi montrer. Les mailles ne se refont
    /// que quand l'outil les redemande, le tour que quand il change.
    /// </summary>
    public sealed class PaintToolPreview : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la pose : le matériau translucide, les couleurs de l'aperçu, les lignes de la Place.")]
        [SerializeField] private PlacementSettings _settings;

        private readonly DrapedMesh _built = new DrapedMesh();
        private GroundOverlay _cells;
        private GroundOverlay _outline;
        private PaintOutline _shownOutline;
        private bool _outlineAllowed;

        /// <summary>Montre ces mailles, de la couleur de la réponse ; aucune maille les cache.</summary>
        public void ShowCells(NavGrid grid, Relief relief, IReadOnlyList<Vector2Int> cells, bool allowed)
        {
            EnsureOverlays();
            _cells.SetColor(allowed ? _settings.AllowedColor : _settings.RefusedColor);
            _built.Begin(relief, _settings.OverlayLiftMeters);
            _built.AddCells(cells, grid);
            _cells.Apply(_built);
            _cells.SetVisible(cells.Count > 0);
        }

        /// <summary>Montre ce tour, de la couleur de la réponse ; le même tour n'est pas refait.</summary>
        public void ShowOutline(Relief relief, PaintOutline outline, bool allowed)
        {
            EnsureOverlays();
            Color color = allowed ? _settings.AllowedColor : _settings.RefusedColor;
            color.a = _settings.OutlineColor.a;
            _outline.SetColor(color);
            _outline.SetVisible(outline.Shape != PaintOutline.Kind.None);
            if (Same(outline, _shownOutline) && allowed == _outlineAllowed)
                return;
            _shownOutline = outline;
            _outlineAllowed = allowed;
            float width = _settings.OutlineWidthMeters;
            // Les lignes un peu au-dessus des mailles, qu'elles ne s'y mêlent pas.
            _built.Begin(relief, _settings.OverlayLiftMeters * 2f);
            switch (outline.Shape)
            {
                case PaintOutline.Kind.Circle:
                    _built.AddCircle(outline.Center, outline.Radius, width);
                    break;
                case PaintOutline.Kind.Box:
                    _built.AddOutline(outline.Rect, width);
                    break;
                case PaintOutline.Kind.Mark:
                    _built.AddMark(outline.Rect, width);
                    break;
            }
            _outline.Apply(_built);
        }

        /// <summary>Cache tout l'aperçu : l'outil n'est plus en main, ou ne vise plus le sol.</summary>
        public void Hide()
        {
            _cells?.SetVisible(false);
            _outline?.SetVisible(false);
            _shownOutline = PaintOutline.None;
        }

        private void OnDestroy()
        {
            _cells?.Destroy();
            _outline?.Destroy();
            _cells = null;
            _outline = null;
        }

        private void EnsureOverlays()
        {
            if (_cells != null)
                return;
            if (_settings == null || _settings.TranslucentMaterial == null)
                throw new System.InvalidOperationException("L'aperçu de la Place n'a pas sa fiche de réglages de la pose.");
            _cells = new GroundOverlay("Aperçu des mailles", transform, _settings.TranslucentMaterial, 2);
            _outline = new GroundOverlay("Aperçu du tour", transform, _settings.TranslucentMaterial, 3);
        }

        private static bool Same(PaintOutline a, PaintOutline b)
        {
            return a.Shape == b.Shape && a.Center == b.Center && Mathf.Approximately(a.Radius, b.Radius) &&
                   a.Rect.Size == b.Rect.Size && Mathf.Approximately(a.Rect.RotationDegrees, b.Rect.RotationDegrees);
        }
    }
}
