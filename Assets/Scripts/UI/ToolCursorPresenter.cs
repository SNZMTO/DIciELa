using System;
using Diciela.Controls;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur du curseur des outils (GDD, section 16 ; décision 221) : la flèche les mains vides ; sinon la
    /// silhouette de l'outil en main (ToolCursorShapes), à la couleur de sa case dans la roue (WheelTool), et pour un
    /// outil de sol ses pastilles : ce qu'il pose (sauf au mode place de charrette), la forme du mode Forme ou le style
    /// d'un tracé, la symétrie quand il y en a une ; la croix rouge quand le geste serait refusé, comme le disent
    /// l'aperçu et la carte ; une éclaboussure quand l'outil agit, qu'un ordre part (décision 225). Il écoute les
    /// outils : le curseur ne photographie jamais.
    /// </summary>
    public sealed class ToolCursorPresenter : IDisposable
    {
        private readonly IToolCursorView _view;
        private readonly PlacementTool _placement;
        private readonly ClearingTool _clearing;
        private readonly MoveTool _move;
        private readonly PaintTool _paths;
        private readonly PaintTool _plaza;

        public ToolCursorPresenter(IToolCursorView view, PlacementTool placement, ClearingTool clearing, MoveTool move,
            PaintTool paths, PaintTool plaza)
        {
            if (view == null || ReferenceEquals(placement, null) || ReferenceEquals(clearing, null) ||
                ReferenceEquals(move, null) || ReferenceEquals(paths, null) || ReferenceEquals(plaza, null))
                throw new ArgumentException("Le curseur des outils a besoin de sa vue et des outils de pose, « Retirer », " +
                                            "Déplacer, Chemins et de la Place.");
            _view = view;
            _placement = placement;
            _clearing = clearing;
            _move = move;
            _paths = paths;
            _plaza = plaza;
            _placement.HeldChanged += OnHeldChanged;
            _placement.ResultChanged += ShowRefused;
            _clearing.HeldChanged += OnHeldChanged;
            _move.HeldChanged += OnHeldChanged;
            _move.ResultChanged += ShowRefused;
            _paths.HeldChanged += OnHeldChanged;
            _paths.ResultChanged += ShowRefused;
            _plaza.HeldChanged += OnHeldChanged;
            _plaza.ResultChanged += ShowRefused;
            _placement.Acted += _view.Splash;
            _clearing.Acted += _view.Splash;
            _move.Acted += _view.Splash;
            _paths.Acted += _view.Splash;
            _plaza.Acted += _view.Splash;
            OnHeldChanged();
        }

        public void Dispose()
        {
            _placement.HeldChanged -= OnHeldChanged;
            _placement.ResultChanged -= ShowRefused;
            _clearing.HeldChanged -= OnHeldChanged;
            _move.HeldChanged -= OnHeldChanged;
            _move.ResultChanged -= ShowRefused;
            _paths.HeldChanged -= OnHeldChanged;
            _paths.ResultChanged -= ShowRefused;
            _plaza.HeldChanged -= OnHeldChanged;
            _plaza.ResultChanged -= ShowRefused;
            _placement.Acted -= _view.Splash;
            _clearing.Acted -= _view.Splash;
            _move.Acted -= _view.Splash;
            _paths.Acted -= _view.Splash;
            _plaza.Acted -= _view.Splash;
            _view.ShowArrow();
        }

        // L'outil de sol en main ; null si aucun ne l'est.
        private PaintTool HeldPaint()
        {
            return _plaza.IsActive ? _plaza : _paths.IsActive ? _paths : null;
        }

        private void OnHeldChanged()
        {
            PaintTool paint = HeldPaint();
            if (paint != null)
                _view.ShowLook(PaintLook(paint, paint == _plaza ? ToolChoice.Plaza : ToolChoice.Paths));
            else if (_placement.IsActive)
                _view.ShowLook(Look(ToolCursorShapes.Hammer, ToolChoice.Build));
            else if (_clearing.IsActive)
                _view.ShowLook(Look(ToolCursorShapes.Axe, ToolChoice.Clear));
            else if (_move.IsActive)
                _view.ShowLook(Look(ToolCursorShapes.Hand, ToolChoice.Move));
            else
                _view.ShowArrow();
            ShowRefused();
        }

        // La croix : la réponse de l'outil en main à ce qu'il vise ; « rien ne change » n'est pas un refus.
        private void ShowRefused()
        {
            PaintTool paint = HeldPaint();
            bool refused = paint != null
                ? !paint.Answer.IsOk && !paint.Answer.ChangesNothing
                : _placement.IsActive
                    ? _placement.Result != PlacementResult.Ok
                    : _move.IsActive && _move.Moving != null && _move.Result != PlacementResult.Ok;
            _view.ShowRefused(refused);
        }

        private static ToolCursorLook Look(ToolCursorShape shape, ToolChoice choice)
        {
            return new ToolCursorLook(shape.Silhouette, shape.Tip, WheelTool.Of(choice).Pastel, "", null, "", "");
        }

        private static ToolCursorLook PaintLook(PaintTool tool, ToolChoice choice)
        {
            PaintToolMode mode = tool.Mode;
            ToolCursorShape shape = ToolCursorShapes.Of(mode);
            PaintInk ink = mode == PaintToolMode.CartSpot ? PaintInk.None : tool.Inks.Chosen;
            PaintOptions options = tool.Options;
            string form = mode == PaintToolMode.Shape ? PaintIconClasses.Shape(options.Shape)
                : mode == PaintToolMode.Trace ? PaintIconClasses.Style(options.Style) : "";
            string symmetry = options.Symmetry == PaintSymmetry.None ? "" : PaintIconClasses.Symmetry(options.Symmetry);
            return new ToolCursorLook(shape.Silhouette, shape.Tip, WheelTool.Of(choice).Pastel,
                ink.IsZone ? PaintIconClasses.Zone : "", ink.Floor != null ? PaintIconClasses.Picture(ink.Floor) : null,
                form, symmetry);
        }
    }
}
