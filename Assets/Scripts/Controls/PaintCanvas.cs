using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Là où travaille un outil de sol (décisions 206, 210, 216 et 220) : ce que vise son pointeur (PaintPointer), le
    /// geste en cours (PaintDrawing), ce qu'en disent la Place ou les sols, montré par l'aperçu (PaintAim), et les ordres
    /// qui en partent (PaintOrders). Il lit chez l'outil ce que celui-ci tient (son mode, son encre, ses options, s'il est
    /// en main) et n'en garde aucune copie.
    /// </summary>
    internal sealed class PaintCanvas : IDisposable
    {
        private readonly PaintTool _tool;
        private readonly World _world;
        private readonly Camera _camera;
        private readonly PlacementSettings _settings;
        private readonly PaintDrawing _drawing = new PaintDrawing();
        private readonly PaintPointer _pointer;
        private readonly PaintAim _target;
        private readonly PaintOrders _orders;
        private readonly PaintGrips _grips;

        public PaintCanvas(PaintTool tool, World world, Camera camera, PaintToolPreview preview,
            PlacementSettings settings)
        {
            _tool = tool;
            _world = world;
            _camera = camera;
            _settings = settings;
            _pointer = new PaintPointer(camera, world.Map.Relief);
            _target = new PaintAim(world, preview, settings);
            _grips = new PaintGrips(_drawing, world, camera, settings);
            // Un ordre joué ou refusé : la Place a peut-être changé sous le pointeur.
            _orders = new PaintOrders(world, () => Refresh(true));
        }

        /// <summary>Levé quand la réponse à ce qui est visé change, ou la place de charrette qu'un clic retirerait.</summary>
        public event Action ResultChanged;

        /// <summary>Levé quand un ordre part.</summary>
        public event Action Acted { add => _orders.Sent += value; remove => _orders.Sent -= value; }

        public PaintAnswer Answer { get; private set; } = PaintAnswer.Ok;
        public PlazaMark Hovered { get; private set; }
        public bool IsDrawing => _drawing.IsDrawing;

        public void Dispose()
        {
            _orders.Dispose();
        }

        /// <summary>Vise le sol sous ce point de l'écran, l'aimant des places de charrette en marche ou coupé.</summary>
        public void AimAt(Vector2 screenPoint, bool magnet)
        {
            if (!_pointer.Aim(screenPoint, magnet))
            {
                LoseAim();
                return;
            }
            // Un point tenu suit le pointeur ; sinon, le geste en cours se prolonge jusque-là.
            Vector2 ground = _pointer.Ground;
            Refresh(_grips.Drag(ground, _tool.Options) ||
                    IsDrawing && _drawing.ExtendTo(ground, _settings.StrokeSpacingMeters, _world.Grid.Metrics.CellSize));
        }

        public void LoseAim()
        {
            if (IsDrawing)
                return;
            _pointer.Lose();
            Refresh(false);
        }

        public void Begin(Vector2 screenStart)
        {
            // Un point posé ou l'épingle pris ici se glissent ; un geste pointé ne se glisse pas.
            if (_grips.Press(screenStart, _tool.Options) || _drawing.IsPointed)
                return;
            PaintToolMode mode = _tool.Mode;
            if (mode == PaintToolMode.CartSpot || PaintPoints.Wanted(_tool) ||
                !_pointer.TryGround(screenStart, out Vector2 start))
                return;
            _drawing.Begin(mode, _tool.Options.Shape, start, _tool.Options.BrushRadius,
                GroundAim.SideDegrees(_camera, _settings.RotationStepDegrees));
            Refresh(true);
        }

        /// <summary>
        /// Le relâchement : la forme glissée part en ordre, avec ses copies, si la Place l'accepte ; un geste pointé
        /// continue ; ce qui était tenu est lâché.
        /// </summary>
        public void End()
        {
            if (_grips.Release())
                Refresh(true);
            if (!IsDrawing || _drawing.IsPointed)
                return;
            GroundShape shape = _target.Checked;
            bool accepted = _drawing.Shape != null && !_target.IsStale && Answer.IsOk;
            _drawing.End();
            if (accepted)
                _orders.SendShape(shape, _tool.Mode == PaintToolMode.Eraser, _tool.Inks, _tool.Entry.Scope);
            Refresh(true);
        }

        /// <summary>
        /// Un clic : un rond du pinceau ; un point d'un tracé ou d'un polygone, ou son départ sur le point qui le finit ;
        /// poser une place de charrette (aimantée, avec ses copies), ou retirer celle qu'on vise.
        /// </summary>
        public void Click()
        {
            if (!_pointer.IsAiming || _orders.IsWaiting)
                return;
            PaintToolMode mode = _tool.Mode;
            if (PaintPoints.Wanted(_tool))
            {
                ClickPointed();
                return;
            }
            if (IsDrawing || _target.IsStale)
                return;
            if (mode == PaintToolMode.CartSpot && Hovered != null)
                _orders.SendRemoval(Hovered);
            else if (mode == PaintToolMode.CartSpot && Answer.IsOk)
                _orders.SendMarks(_target.Copies, _target.Spot.Center, _target.Spot.HeadingDegrees);
            else if ((mode == PaintToolMode.Brush || mode == PaintToolMode.Eraser) && Answer.IsOk)
                _orders.SendShape(_target.Checked, mode == PaintToolMode.Eraser, _tool.Inks, _tool.Entry.Scope);
        }

        /// <summary>Abandonne le geste en cours ; s'il n'y en a pas, oublie aussi ce qui est visé.</summary>
        public void Abandon(bool forgetAim)
        {
            _grips.Release();
            _drawing.End();
            if (forgetAim)
                _pointer.Lose();
            Refresh(true);
        }

        /// <summary>La symétrie a changé : l'épingle se pose où il faut si elle n'est pas encore posée.</summary>
        public void PlacePin()
        {
            _grips.Pin.PlaceByDefault(_tool);
            Refresh(true);
        }

        /// <summary>Le style a changé : le geste pointé en cours se relie droit, ou en courbe.</summary>
        public void Restyle()
        {
            _drawing.Restyle(_tool.Options.Style == PaintTraceStyle.Curved);
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
            if (_tool.IsActive && _pointer.IsAiming)
                _target.Show(_tool, _pointer, _drawing);
            else
                _target.Hide();
            if (_target.Answer == Answer && ReferenceEquals(_target.Hovered, Hovered))
                return;
            Answer = _target.Answer;
            Hovered = _target.Hovered;
            ResultChanged?.Invoke();
        }

        // Un clic d'un geste pointé : un point de plus, ou le départ, sur le point qui le finit. Refusé, il reste, et la
        // raison s'écrit.
        private void ClickPointed()
        {
            bool finish = _grips.Points.Click(_pointer.Ground, _pointer.Screen, _tool, _settings.CurveStepMeters);
            if (finish)
                _drawing.Settle();
            Refresh(true);
            if (!finish || _target.IsStale || !Answer.IsOk)
                return;
            _orders.SendShape(_target.Checked, false, _tool.Inks, _tool.Entry.Scope);
            _drawing.End();
            Refresh(true);
        }
    }
}
