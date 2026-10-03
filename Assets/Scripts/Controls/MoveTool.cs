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
    /// modèle ; son fantôme suit alors le pointeur comme à la pose (le même aperçu, les mêmes règles, la construction
    /// elle-même ne comptant pas), et on le tourne de même ; un clic l'envoie à sa nouvelle place par l'ordre
    /// « déplacer ». Clic droit ou Échap reposent le fantôme, l'outil restant en main ; sans fantôme, ils lâchent l'outil.
    /// Une construction qu'on déplace déjà ne se reprend pas. Un ordre envoyé attend d'être joué : joué, le fantôme se
    /// repose et l'outil attend la suivante ; refusé, il suit encore le pointeur. Les gestes et les tests l'appellent par
    /// les mêmes méthodes. Celui qui le crée le libère (Dispose).
    /// </summary>
    public sealed class MoveTool : IDisposable, IHeldTool, IAimingTool
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly StructureViews _views;
        private readonly PlacementPreview _preview;
        private readonly PlacementSettings _settings;
        private Vector2 _pointer;
        private float _rotationDegrees;
        private bool _aimed;
        private bool _sent;
        private Placement _aim;

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
            _preview = preview;
            _settings = settings;
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
        public event Action ResultChanged;

        public bool IsActive { get; private set; }

        /// <summary>La construction dont le fantôme suit le pointeur ; null tant qu'on n'en a touché aucune.</summary>
        public Structure Moving { get; private set; }

        /// <summary>L'angle du fantôme, en degrés depuis le nord vers l'est : d'abord celui de la construction.</summary>
        public float RotationDegrees => _rotationDegrees;

        /// <summary>La réponse des règles de pose à la place visée ; Ok quand rien n'est visé.</summary>
        public PlacementResult Result { get; private set; }

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
            if (Moving == null)
                return;
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            if (!_world.Map.Relief.TryRaycast(ray.origin, ray.direction, _camera.farClipPlane, out Vector3 ground))
            {
                LoseAim();
                return;
            }
            _aimed = true;
            _aim = new Placement(new Vector2(ground.x, ground.z), _rotationDegrees);
            Refresh();
        }

        public void LoseAim()
        {
            _aimed = false;
            _preview.Hide();
            SetResult(PlacementResult.Ok);
        }

        public void RotateStep()
        {
            RotateBy(_settings.RotationStepDegrees);
        }

        public void RotateBy(float degrees)
        {
            _rotationDegrees = Mathf.Repeat(_rotationDegrees + degrees, 360f);
            if (Moving == null || !_aimed)
                return;
            _aim = new Placement(_aim.Center, _rotationDegrees);
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
            if (!_aimed || _sent || Result != PlacementResult.Ok)
                return false;
            _world.Submit(new MoveStructureCommand(Moving.Id, _aim));
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

        public bool Escape()
        {
            if (!IsActive)
                return false;
            Cancel();
            return true;
        }

        public void PutDown()
        {
            if (!IsActive)
                return;
            Moving = null;
            _aimed = false;
            _preview.Clear();
            SetResult(PlacementResult.Ok);
            IsActive = false;
            HeldChanged?.Invoke();
        }

        // Touche la construction dont on vise le modèle : son fantôme suivra le pointeur, à son angle.
        private bool Pick()
        {
            if (!_views.TryPick(_camera.ScreenPointToRay(_pointer), out Structure picked) || picked.MovingTo.HasValue)
                return false;
            Moving = picked;
            _rotationDegrees = picked.Placement.RotationDegrees;
            _aimed = false;
            HeldChanged?.Invoke();
            return true;
        }

        private void Drop()
        {
            Moving = null;
            _aimed = false;
            _preview.Clear();
            SetResult(PlacementResult.Ok);
            HeldChanged?.Invoke();
        }

        // Un déplacement est joué : on peut en envoyer un autre ; le nôtre, le fantôme se repose et l'outil attend.
        private void OnMovePlayed(Structure structure)
        {
            _sent = false;
            if (ReferenceEquals(structure, Moving))
                Drop();
            else if (Moving != null && _aimed)
                Refresh();
        }

        private void OnMoveRefused(Structure structure, Placement placement, PlacementResult reason)
        {
            _sent = false;
            if (Moving != null && _aimed)
                Refresh();
        }

        private void Refresh()
        {
            SetResult(_world.PlacementRules.CheckMove(Moving, _aim));
            _preview.Show(Moving.Definition, _aim, _world.Map.Relief, Result == PlacementResult.Ok);
        }

        private void SetResult(PlacementResult result)
        {
            if (result == Result)
                return;
            Result = result;
            ResultChanged?.Invoke();
        }
    }
}
