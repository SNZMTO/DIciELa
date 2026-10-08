using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// L'outil de pose (GDD, section 9) : la construction choisie suit le pointeur sur le sol, en fantôme (GhostAim) ;
    /// l'aperçu dit, par les règles de pose, la même fonction que l'ordre, si elle se pose là ; on tourne d'un cran ou au
    /// degré ; on pose par l'ordre « poser », et l'outil reste prêt pour la suivante jusqu'à ce qu'on l'annule, ou
    /// qu'elle soit unique. Un ordre envoyé attend d'être joué avant le suivant ; posé ou refusé, l'aperçu se refait. Les
    /// gestes et les tests l'appellent par les mêmes méthodes. Celui qui le crée le libère (Dispose).
    /// </summary>
    public sealed class PlacementTool : IDisposable, IHeldTool, IAimingTool
    {
        private readonly World _world;
        private readonly GhostAim _ghost;
        private bool _sent;

        public PlacementTool(World world, Camera camera, PlacementPreview preview, PlacementSettings settings)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (camera == null || preview == null || settings == null)
                throw new ArgumentException("L'outil de pose a besoin d'une caméra, d'un aperçu et de ses réglages.");

            _world = world;
            _ghost = new GhostAim(camera, world.Map.Relief, preview, settings);
            _world.Structures.StructurePlaced += OnStructurePlaced;
            _world.Structures.PlacementRefused += OnPlacementRefused;
        }

        public void Dispose()
        {
            _world.Structures.StructurePlaced -= OnStructurePlaced;
            _world.Structures.PlacementRefused -= OnPlacementRefused;
        }

        /// <summary>Levé quand la construction en main change : choisie, ou posée de côté.</summary>
        public event Action HeldChanged;

        /// <summary>Levé quand l'ordre de poser part.</summary>
        public event Action Acted;

        /// <summary>Levé quand la réponse des règles de pose à la place visée change.</summary>
        public event Action ResultChanged
        {
            add => _ghost.ResultChanged += value;
            remove => _ghost.ResultChanged -= value;
        }

        /// <summary>La construction en cours de pose ; null quand l'outil est au repos.</summary>
        public StructureDefinition Current { get; private set; }

        public bool IsActive => !ReferenceEquals(Current, null);

        /// <summary>L'angle de la construction en main, en degrés depuis le nord vers l'est ; il reste d'une pose à l'autre.</summary>
        public float RotationDegrees => _ghost.RotationDegrees;

        /// <summary>La réponse des règles de pose à la place visée ; Ok quand rien n'est visé.</summary>
        public PlacementResult Result => _ghost.Result;

        /// <summary>Prend cette construction en main : elle suivra le pointeur.</summary>
        public void Begin(StructureDefinition definition)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));
            if (ReferenceEquals(definition, Current))
                return;

            _ghost.Clear();
            Current = definition;
            HeldChanged?.Invoke();
        }

        /// <summary>Vise le sol sous ce point de l'écran ; hors du sol, l'aperçu se cache.</summary>
        public void AimAt(Vector2 screenPoint)
        {
            if (IsActive && _ghost.AimAt(screenPoint))
                Refresh();
        }

        /// <summary>Le pointeur ne vise plus le sol (il est sur l'interface, ou dans le ciel).</summary>
        public void LoseAim()
        {
            _ghost.LoseAim();
        }

        /// <summary>Tourne d'un cran (GDD, section 9) ; l'angle vaut pour la visée suivante.</summary>
        public void RotateStep()
        {
            RotateBy(_ghost.StepDegrees);
        }

        /// <summary>Tourne de cet angle, en degrés ; un angle positif tourne vers l'est, comme la caméra.</summary>
        public void RotateBy(float degrees)
        {
            if (_ghost.RotateBy(degrees) && IsActive)
                Refresh();
        }

        /// <summary>Pose la construction visée, si les règles le permettent ; vrai si l'ordre est envoyé.</summary>
        public bool Confirm()
        {
            if (!IsActive || !_ghost.IsAimed || _sent || Result != PlacementResult.Ok)
                return false;
            _world.Submit(new PlaceStructureCommand(Current, _ghost.Aim));
            _sent = true;
            Acted?.Invoke();
            return true;
        }

        /// <summary>Le lâche tout à fait : comme le clic droit.</summary>
        public void PutDown()
        {
            Cancel();
        }

        /// <summary>Pose l'outil de côté : plus d'aperçu, plus de construction en main.</summary>
        public void Cancel()
        {
            if (!IsActive)
                return;
            _ghost.Clear();
            Current = null;
            HeldChanged?.Invoke();
        }

        private void OnStructurePlaced(Structure structure)
        {
            OnOrderPlayed();
        }

        private void OnPlacementRefused(StructureDefinition definition, Placement placement, PlacementResult reason)
        {
            OnOrderPlayed();
        }

        /// <summary>
        /// Un ordre a été joué : on peut en envoyer un autre, et la place visée n'est peut-être plus libre. Une
        /// construction unique qu'on vient de poser ne se pose plus nulle part : l'outil la pose de côté.
        /// </summary>
        private void OnOrderPlayed()
        {
            _sent = false;
            if (IsActive && !_world.PlacementRules.IsAvailable(Current))
                Cancel();
            else if (IsActive && _ghost.IsAimed)
                Refresh();
        }

        /// <summary>Demande aux règles de pose si la place visée convient, et le montre.</summary>
        private void Refresh()
        {
            _ghost.Show(Current, _world.PlacementRules.Check(Current, _ghost.Aim));
        }
    }
}
