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
    /// Un outil de sol (GDD, sections 9 et 11 ; décisions 206, 210 et 216) : un seul outil, deux entrées (PaintEntry),
    /// l'outil de la Place, pris à sa carte, et l'outil Chemins, pris à la roue. Ses modes, en familles (PaintToolMode) :
    /// peindre et gommer au pinceau, tracer point par point, tracer une forme, poser ou retirer une place de charrette ;
    /// ce qu'il pose, la zone ou un sol, est son encre (PaintInks) ; ses réglages sans geste, ses options (PaintOptions).
    /// Il travaille sur sa surface (PaintCanvas) : une forme est un ordre, envoyé au relâchement d'un geste glissé, ou au
    /// clic sur le dernier point d'un tracé ; pendant le geste, l'aperçu montre ce qu'elle changerait, par la même
    /// fonction que l'ordre, et une forme refusée ne part pas. Les gestes et les tests l'appellent par les mêmes méthodes.
    /// Celui qui le crée le libère.
    /// </summary>
    public sealed class PaintTool : IDisposable, IHeldTool
    {
        private readonly PlacementSettings _settings;
        private readonly PaintCanvas _canvas;

        /// <summary>Un outil de sol de cette entrée, qui pose la zone (si l'entrée la pose) ou l'un de ces sols.</summary>
        public PaintTool(World world, Camera camera, PaintToolPreview preview, PlacementSettings settings,
            PaintEntry entry, IReadOnlyList<FloorDefinition> floors)
        {
            if (ReferenceEquals(world, null) || camera == null || preview == null || settings == null)
                throw new ArgumentException("L'outil de sol a besoin du monde, d'une caméra, d'un aperçu et de ses réglages.");
            _settings = settings;
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Mode = entry.Modes[0];
            Inks = new PaintInks(entry, world.Knowledge, floors);
            Options = new PaintOptions(settings, entry.StartBrushRadius(settings), () => IsActive, OnOptionChanged);
            _canvas = new PaintCanvas(this, world, camera, preview, settings);
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
        public PaintToolMode Mode { get; private set; }

        /// <summary>Son entrée : l'outil de la Place, ou l'outil Chemins.</summary>
        public PaintEntry Entry { get; }

        /// <summary>Ce qu'il pose : la zone ou un sol, et les sols que la ville sait faire.</summary>
        public PaintInks Inks { get; }

        /// <summary>Ses options : la forme, le style, la symétrie, la taille du pinceau, le cap de la place de charrette.</summary>
        public PaintOptions Options { get; }

        /// <summary>La réponse de la Place ou des sols à ce qu'il vise ; Ok quand rien n'est visé.</summary>
        public PaintAnswer Answer => _canvas.Answer;

        /// <summary>La place de charrette sous le pointeur, qu'un clic retirerait ; null s'il n'y en a pas.</summary>
        public PlazaMark HoveredMark => _canvas.Hovered;

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
        public void Choose(PaintToolMode mode)
        {
            if (!IsActive || !Entry.Allows(mode) || (mode == Mode && !IsDrawing))
                return;
            Mode = mode;
            _canvas.Abandon(false);
            HeldChanged?.Invoke();
        }

        /// <summary>Change ce qu'il pose : la zone, ou un sol ; le geste en cours est abandonné.</summary>
        public void ChooseInk(PaintInk ink)
        {
            Changed(IsActive && Inks.Choose(ink), () => _canvas.Abandon(false));
        }

        /// <summary>Grandit (positif) ou rapetisse le pinceau d'autant de crans, dans ses bornes.</summary>
        public void ResizeBrush(int notches)
        {
            Changed(IsActive && Options.Resize(notches), _canvas.ForgetDot);
        }

        /// <summary>Vise le sol sous ce point, aimanté ou non ; pendant un geste, le prolonge jusque-là.</summary>
        public void AimAt(Vector2 screenPoint, bool magnet = true)
        {
            if (IsActive)
                _canvas.AimAt(screenPoint, magnet);
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

        /// <summary>
        /// Tourne d'un cran (R) : la place de charrette, comme une construction (GDD, section 11), ou, hors de ce mode, les
        /// axes d'une symétrie.
        /// </summary>
        public void RotateStep()
        {
            RotateBy(_settings.RotationStepDegrees);
        }

        /// <summary>Tourne de cet angle, en degrés, vers l'est : la place de charrette, ou les axes d'une symétrie.</summary>
        public void RotateBy(float degrees)
        {
            Options.Turn(degrees, Mode == PaintToolMode.CartSpot);
            if (IsActive)
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

        // Une option choisie à la roue : la surface s'y fait, et la carte le dit.
        private void OnOptionChanged(PaintOptionEffect effect)
        {
            if (effect == PaintOptionEffect.Abandon)
                _canvas.Abandon(false);
            else if (effect == PaintOptionEffect.Restyle)
                _canvas.Restyle();
            else if (effect == PaintOptionEffect.PlacePin)
                _canvas.PlacePin();
            else
                _canvas.Refresh(true);
            HeldChanged?.Invoke();
        }

        // Si l'encre ou le pinceau a changé, la surface s'y fait (abandon du geste, nouveau rond) et la carte le dit.
        private void Changed(bool changed, Action follow)
        {
            if (!changed)
                return;
            follow();
            HeldChanged?.Invoke();
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
