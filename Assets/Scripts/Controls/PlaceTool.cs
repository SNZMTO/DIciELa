using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Un outil de sol (GDD, sections 9 et 11 ; décisions 206 et 210) : un seul outil, deux entrées (PaintEntry), l'outil
    /// de la Place, pris à sa carte, et l'outil Chemins, pris à la roue. Peindre et gommer au pinceau, tracer un disque ou
    /// un rectangle, poser ou retirer une place de charrette (PlaceToolMode) ; ce qu'il pose, la zone ou un sol, est son
    /// encre (PaintInks). Il tient son mode, son encre, la taille de son pinceau et le cap de la place de charrette ; il
    /// travaille sur sa surface (PlaceCanvas) : un geste glissé est une forme, et une forme est un ordre, envoyé au
    /// relâchement ; pendant le geste, l'aperçu montre ce qu'elle changerait, par la même fonction que l'ordre, et une
    /// forme refusée ne part pas. Les gestes et les tests l'appellent par les mêmes méthodes. Celui qui le crée le libère.
    /// </summary>
    public sealed class PlaceTool : IDisposable, IEscapable, IHeldTool
    {
        private readonly PlacementSettings _settings;
        private readonly PlaceCanvas _canvas;

        /// <summary>Un outil de sol de cette entrée, qui pose la zone (si l'entrée la pose) ou l'un de ces sols.</summary>
        public PlaceTool(World world, Camera camera, PlaceToolPreview preview, PlacementSettings settings,
            PaintEntry entry, IReadOnlyList<FloorDefinition> floors)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (camera == null || preview == null || settings == null)
                throw new ArgumentException("L'outil de la Place a besoin d'une caméra, d'un aperçu et de ses réglages.");

            _settings = settings;
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Mode = entry.Modes[0];
            Inks = new PaintInks(entry, world.Knowledge, floors);
            _canvas = new PlaceCanvas(this, world, camera, preview, settings);
            Vector2 range = settings.BrushRadiusRangeMeters;
            BrushRadius = Mathf.Clamp(entry.StartBrushRadius(settings), range.x, range.y);
        }

        public void Dispose()
        {
            _canvas.Dispose();
        }

        /// <summary>Levé quand on le prend en main, qu'on le lâche, qu'on change son mode ou la taille du pinceau.</summary>
        public event Action HeldChanged;

        /// <summary>Levé quand la réponse à ce qu'il vise change, ou la place de charrette qu'un clic retirerait.</summary>
        public event Action ResultChanged
        {
            add => _canvas.ResultChanged += value;
            remove => _canvas.ResultChanged -= value;
        }

        public bool IsActive { get; private set; }
        public PlaceToolMode Mode { get; private set; }

        /// <summary>Le rayon du pinceau et de la gomme, en mètres.</summary>
        public float BrushRadius { get; private set; }

        /// <summary>Le cap de la place de charrette en main, comme une construction à la pose.</summary>
        public float HeadingDegrees { get; private set; }

        /// <summary>Son entrée : l'outil de la Place, ou l'outil Chemins.</summary>
        public PaintEntry Entry { get; }

        /// <summary>Ce qu'il pose : la zone ou un sol, et les sols que la ville sait faire.</summary>
        public PaintInks Inks { get; }

        /// <summary>La réponse de la Place ou des sols à ce qu'il vise ; Ok quand rien n'est visé.</summary>
        public PaintAnswer Answer => _canvas.Answer;

        /// <summary>La place de charrette sous le pointeur, qu'un clic retirerait ; null s'il n'y en a pas.</summary>
        public PlaceMark HoveredMark => _canvas.Hovered;

        public bool IsDrawing => _canvas.IsDrawing;

        /// <summary>Prend l'outil en main, dans le mode où on l'a laissé.</summary>
        public void Take()
        {
            if (IsActive)
                return;
            IsActive = true;
            Inks.Refresh();
            _canvas.Refresh(true);
            HeldChanged?.Invoke();
        }

        /// <summary>Change de mode, s'il est le sien ; le geste en cours est abandonné.</summary>
        public void Choose(PlaceToolMode mode)
        {
            if (!IsActive || !Entry.Allows(mode) || (mode == Mode && !IsDrawing))
                return;
            Mode = mode;
            _canvas.Abandon(false);
            HeldChanged?.Invoke();
        }

        /// <summary>Change ce qu'il pose : ce sol, ou la zone (null) ; le geste en cours est abandonné.</summary>
        public void ChooseFloor(FloorDefinition floor)
        {
            if (!IsActive || !Inks.Choose(floor))
                return;
            _canvas.Abandon(false);
            HeldChanged?.Invoke();
        }

        /// <summary>Grandit (positif) ou rapetisse le pinceau d'autant de crans, dans ses bornes.</summary>
        public void ResizeBrush(int notches)
        {
            Vector2 range = _settings.BrushRadiusRangeMeters;
            float radius = Mathf.Clamp(BrushRadius + notches * _settings.BrushRadiusStepMeters, range.x, range.y);
            if (!IsActive || Mathf.Approximately(radius, BrushRadius))
                return;
            BrushRadius = radius;
            _canvas.ForgetDot();
            HeldChanged?.Invoke();
        }

        /// <summary>Vise le sol sous ce point de l'écran ; pendant un geste, le prolonge jusque-là.</summary>
        public void AimAt(Vector2 screenPoint)
        {
            if (IsActive)
                _canvas.AimAt(screenPoint);
        }

        /// <summary>Le pointeur ne vise plus le sol ; un geste en cours attend qu'il y revienne.</summary>
        public void LoseAim()
        {
            _canvas.LoseAim();
        }

        /// <summary>Commence un geste glissé depuis ce point de l'écran (pas pour une place de charrette).</summary>
        public void BeginDrawing(Vector2 screenStart)
        {
            if (IsActive)
                _canvas.Begin(screenStart);
        }

        /// <summary>Le relâchement : la forme tracée part en ordre, si la Place l'accepte.</summary>
        public void EndDrawing()
        {
            _canvas.End();
        }

        /// <summary>Le clic gauche : un rond du pinceau ; poser une place de charrette, ou retirer celle qu'on vise.</summary>
        public void Click()
        {
            if (IsActive)
                _canvas.Click();
        }

        /// <summary>Tourne la place de charrette d'un cran (GDD, section 11 : comme une construction, R).</summary>
        public void RotateStep()
        {
            RotateBy(_settings.RotationStepDegrees);
        }

        /// <summary>Tourne la place de charrette de cet angle, en degrés ; positif, vers l'est.</summary>
        public void RotateBy(float degrees)
        {
            HeadingDegrees = Mathf.Repeat(HeadingDegrees + degrees, 360f);
            if (IsActive && Mode == PlaceToolMode.CartSpot)
                _canvas.Refresh(true);
        }

        /// <summary>Le clic droit : abandonne le geste en cours ; sans geste, lâche l'outil.</summary>
        public void Cancel()
        {
            if (IsDrawing)
                _canvas.Abandon(false);
            else
                PutDown();
        }

        /// <summary>Échap : comme le clic droit ; faux si l'outil n'était pas en main.</summary>
        public bool Escape()
        {
            if (!IsActive)
                return false;
            Cancel();
            return true;
        }

        /// <summary>Le lâche tout à fait, avec le geste en cours.</summary>
        public void PutDown()
        {
            if (!IsActive)
                return;
            IsActive = false;
            _canvas.Abandon(true);
            HeldChanged?.Invoke();
        }
    }
}
