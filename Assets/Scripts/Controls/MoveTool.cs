using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// L'outil Déplacer (GDD, section 9 ; décisions 199 et 201) : en main, on touche une construction en visant son
    /// modèle ; son fantôme suit alors le pointeur comme à la pose (GhostAim : le même aperçu, les mêmes règles, la
    /// construction elle-même ne comptant pas), et on le tourne de même ; un clic l'envoie à sa nouvelle place par
    /// l'ordre « déplacer ». Clic droit ou Échap reposent le fantôme, l'outil restant en main ; sans fantôme, ils lâchent
    /// l'outil. Une construction qu'on déplace déjà ne se reprend pas. Un ordre envoyé attend d'être joué : joué, le
    /// fantôme se repose et l'outil attend la suivante ; refusé, il suit encore le pointeur. Les gestes et les tests
    /// l'appellent par les mêmes méthodes. Celui qui le crée le libère (Dispose).
    /// </summary>
    public sealed class MoveTool : IDisposable, IHeldTool, IAimingTool
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly StructureViews _views;
        private readonly GhostAim _ghost;
        private Vector2 _pointer;
        private bool _sent;

        public MoveTool(World world, Camera camera, StructureViews views, PlacementPreview preview,
            PlacementSettings settings)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (camera == null || views == null || preview == null || settings == null)
                throw new ArgumentException("L'outil Déplacer a besoin d'une caméra, des constructions vues, d'un aperçu " +
                                            "et des réglages de la pose.");

            _world = world;
            _camera = camera;
            _views = views;
            _ghost = new GhostAim(camera, world.Map.Relief, preview, settings);
            _world.Structures.MoveStarted += OnMovePlayed;
            _world.Structures.StructureMoved += OnMovePlayed;
            _world.Structures.MoveRefused += OnMoveRefused;
        }

        public void Dispose()
        {
            _world.Structures.MoveStarted -= OnMovePlayed;
            _world.Structures.StructureMoved -= OnMovePlayed;
            _world.Structures.MoveRefused -= OnMoveRefused;
        }

        /// <summary>
        /// Levé quand on le prend en main, qu'on le lâche, ou qu'un fantôme se met à suivre le pointeur ou se repose.
        /// </summary>
        public event Action HeldChanged;

        /// <summary>Levé quand la réponse des règles de pose à la place visée change.</summary>
        public event Action ResultChanged
        {
            add => _ghost.ResultChanged += value;
            remove => _ghost.ResultChanged -= value;
        }

        public bool IsActive { get; private set; }

        /// <summary>La construction dont le fantôme suit le pointeur ; null tant qu'on n'en a touché aucune.</summary>
        public Structure Moving { get; private set; }

        /// <summary>L'angle du fantôme, en degrés depuis le nord vers l'est : d'abord celui de la construction.</summary>
        public float RotationDegrees => _ghost.RotationDegrees;

        /// <summary>La réponse des règles de pose à la place visée ; Ok quand rien n'est visé.</summary>
        public PlacementResult Result => _ghost.Result;

        /// <summary>Le prend en main : on touchera la construction à déplacer.</summary>
        public void Take()
        {
            if (IsActive)
                return;
            IsActive = true;
            HeldChanged?.Invoke();
        }

        /// <summary>Vise ce point de l'écran : la construction à toucher, ou le sol où mène le fantôme.</summary>
        public void AimAt(Vector2 screenPoint)
        {
            _pointer = screenPoint;
            if (Moving != null && _ghost.AimAt(screenPoint))
                Refresh();
        }

        public void LoseAim()
        {
            _ghost.LoseAim();
        }

        public void RotateStep()
        {
            RotateBy(_ghost.StepDegrees);
        }

        public void RotateBy(float degrees)
        {
            if (_ghost.RotateBy(degrees) && Moving != null)
                Refresh();
        }

        /// <summary>
        /// Sans fantôme, touche la construction visée ; avec, l'envoie à la place visée si les règles le permettent. Vrai
        /// si une construction est touchée ou un ordre envoyé.
        /// </summary>
        public bool Confirm()
        {
            if (!IsActive)
                return false;
            if (Moving == null)
                return Pick();
            if (!_ghost.IsAimed || _sent || Result != PlacementResult.Ok)
                return false;
            _world.Submit(new MoveStructureCommand(Moving.Id, _ghost.Aim));
            _sent = true;
            return true;
        }

        /// <summary>Repose le fantôme, l'outil restant en main ; sans fantôme, lâche l'outil.</summary>
        public void Cancel()
        {
            if (Moving != null)
                Drop();
            else
                PutDown();
        }

        public void PutDown()
        {
            if (!IsActive)
                return;
            Moving = null;
            _ghost.Clear();
            IsActive = false;
            HeldChanged?.Invoke();
        }

        // Touche la construction dont on vise le modèle : son fantôme suivra le pointeur, à son angle.
        private bool Pick()
        {
            if (!_views.TryPick(_camera.ScreenPointToRay(_pointer), out Structure picked) || picked.MovingTo.HasValue)
                return false;
            Moving = picked;
            _ghost.Restart(picked.Placement.RotationDegrees);
            HeldChanged?.Invoke();
            return true;
        }

        private void Drop()
        {
            Moving = null;
            _ghost.Clear();
            HeldChanged?.Invoke();
        }

        // Un déplacement est joué : on peut en envoyer un autre ; le nôtre, le fantôme se repose et l'outil attend.
        private void OnMovePlayed(Structure structure)
        {
            _sent = false;
            if (ReferenceEquals(structure, Moving))
                Drop();
            else if (Moving != null && _ghost.IsAimed)
                Refresh();
        }

        private void OnMoveRefused(Structure structure, Placement placement, PlacementResult reason)
        {
            _sent = false;
            if (Moving != null && _ghost.IsAimed)
                Refresh();
        }

        private void Refresh()
        {
            _ghost.Show(Moving.Definition, _world.PlacementRules.CheckMove(Moving, _ghost.Aim));
        }
    }
}
