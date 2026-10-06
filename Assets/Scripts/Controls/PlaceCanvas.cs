using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Là où travaille l'outil de la Place (décision 206) : ce qu'il vise au sol, le geste en cours (PlaceDrawing), ce
    /// que la Place en dit, montré par l'aperçu (PlaceAim), et les ordres qui en partent (PlaceOrders). Il lit chez
    /// l'outil ce que celui-ci tient (son mode, son pinceau, son cap, s'il est en main) et n'en garde aucune copie.
    /// </summary>
    internal sealed class PlaceCanvas : IDisposable
    {
        private readonly PlaceTool _tool;
        private readonly World _world;
        private readonly Camera _camera;
        private readonly PlacementSettings _settings;
        private readonly PlaceDrawing _drawing = new PlaceDrawing();
        private readonly PlaceAim _target;
        private readonly PlaceOrders _orders;
        private Vector2 _aim;
        private bool _aimed;

        public PlaceCanvas(PlaceTool tool, World world, Camera camera, PlaceToolPreview preview,
            PlacementSettings settings)
        {
            _tool = tool;
            _world = world;
            _camera = camera;
            _settings = settings;
            _target = new PlaceAim(world, preview);
            // Un ordre joué ou refusé : la Place a peut-être changé sous le pointeur.
            _orders = new PlaceOrders(world, () => Refresh(true));
        }

        /// <summary>Levé quand la réponse à ce qui est visé change, ou la place de charrette qu'un clic retirerait.</summary>
        public event Action ResultChanged;

        public PlaceResult Result { get; private set; }
        public PlaceMark Hovered { get; private set; }
        public bool IsDrawing => _drawing.IsDrawing;

        public void Dispose()
        {
            _orders.Dispose();
        }

        public void AimAt(Vector2 screenPoint)
        {
            if (!TryGround(screenPoint, out Vector2 ground))
            {
                LoseAim();
                return;
            }
            _aimed = true;
            _aim = ground;
            Refresh(IsDrawing && _drawing.ExtendTo(ground, _settings.StrokeSpacingMeters,
                _world.Grid.Metrics.CellSize));
        }

        public void LoseAim()
        {
            if (IsDrawing)
                return;
            _aimed = false;
            Refresh(false);
        }

        public void Begin(Vector2 screenStart)
        {
            if (_tool.Mode == PlaceToolMode.CartSpot || !TryGround(screenStart, out Vector2 start))
                return;
            _drawing.Begin(_tool.Mode, start, _tool.BrushRadius, BoxDegrees());
            Refresh(true);
        }

        /// <summary>Le relâchement : la forme tracée part en ordre, si la Place l'accepte.</summary>
        public void End()
        {
            if (!IsDrawing)
                return;
            GroundShape shape = _drawing.Shape;
            bool accepted = shape != null && !_target.IsStale && Result == PlaceResult.Ok;
            _drawing.End();
            if (accepted)
                _orders.SendShape(shape, _tool.Mode == PlaceToolMode.Eraser);
            Refresh(true);
        }

        /// <summary>Un clic : un rond du pinceau ; poser une place de charrette, ou retirer celle qu'on vise.</summary>
        public void Click()
        {
            if (!_aimed || IsDrawing || _orders.IsWaiting || _target.IsStale)
                return;
            PlaceToolMode mode = _tool.Mode;
            if (mode == PlaceToolMode.CartSpot && Hovered != null)
                _orders.SendRemoval(Hovered);
            else if (mode == PlaceToolMode.CartSpot && Result == PlaceResult.Ok)
                _orders.SendMark(_aim, _tool.HeadingDegrees);
            else if ((mode == PlaceToolMode.Brush || mode == PlaceToolMode.Eraser) && Result == PlaceResult.Ok)
                _orders.SendShape(_target.Dot, mode == PlaceToolMode.Eraser);
        }

        /// <summary>Abandonne le geste en cours ; s'il n'y en a pas, oublie aussi ce qui est visé.</summary>
        public void Abandon(bool forgetAim)
        {
            _drawing.End();
            if (forgetAim)
                _aimed = false;
            Refresh(true);
        }

        /// <summary>Le pinceau a changé de taille : son rond se refait.</summary>
        public void ForgetDot()
        {
            _target.ForgetDot();
            Refresh(true);
        }

        /// <summary>Redemande à la Place ce que donnerait ce qui est visé (si quelque chose a changé), et le montre.</summary>
        public void Refresh(bool changed)
        {
            if (changed)
                _target.Invalidate();
            if (_tool.IsActive && _aimed)
                _target.Show(_tool.Mode, _aim, _tool.BrushRadius, _tool.HeadingDegrees, _drawing);
            else
                _target.Hide();
            if (_target.Result == Result && ReferenceEquals(_target.Hovered, Hovered))
                return;
            Result = _target.Result;
            Hovered = _target.Hovered;
            ResultChanged?.Invoke();
        }

        // Le cap des côtés d'un rectangle : celui de la caméra, au cran de rotation, pour que des rectangles tracés de la
        // même vue s'alignent.
        private float BoxDegrees()
        {
            float step = _settings.RotationStepDegrees;
            return Mathf.Repeat(Mathf.Round(_camera.transform.eulerAngles.y / step) * step, 360f);
        }

        private bool TryGround(Vector2 screenPoint, out Vector2 ground)
        {
            return GroundAim.TryGround(_camera, _world.Map.Relief, screenPoint, out ground);
        }
    }
}
