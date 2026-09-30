using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// L'outil de pose (GDD, section 9) : la construction choisie suit le pointeur sur le sol ; l'aperçu dit, par les
    /// règles de pose, la même fonction que l'ordre, si elle se pose là ; on tourne d'un cran ou au degré ; on pose par
    /// l'ordre « poser », et l'outil reste prêt pour la suivante jusqu'à ce qu'on l'annule, ou qu'elle soit unique. Un
    /// ordre envoyé attend d'être joué avant le suivant ; posé ou refusé, l'aperçu se refait. Les gestes et les tests
    /// l'appellent par les mêmes méthodes. Celui qui le crée le libère (Dispose).
    /// </summary>
    public sealed class PlacementTool : IDisposable, IEscapable
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly PlacementPreview _preview;
        private readonly PlacementSettings _settings;
        private float _rotationDegrees;
        private bool _aimed;
        private bool _sent;
        private Placement _aim;

        public PlacementTool(World world, Camera camera, PlacementPreview preview, PlacementSettings settings)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (camera == null || preview == null || settings == null)
                throw new ArgumentException("L'outil de pose a besoin d'une caméra, d'un aperçu et de ses réglages.");

            _world = world;
            _camera = camera;
            _preview = preview;
            _settings = settings;
            _world.Structures.StructurePlaced += OnStructurePlaced;
            _world.Structures.PlacementRefused += OnPlacementRefused;
        }

        public void Dispose()
        {
            _world.Structures.StructurePlaced -= OnStructurePlaced;
            _world.Structures.PlacementRefused -= OnPlacementRefused;
        }

        /// <summary>Levé quand la construction en main change : choisie, ou posée de côté.</summary>
        public event Action CurrentChanged;

        /// <summary>Levé quand la réponse des règles de pose à la place visée change.</summary>
        public event Action ResultChanged;

        /// <summary>La construction en cours de pose ; null quand l'outil est au repos.</summary>
        public StructureDefinition Current { get; private set; }

        public bool IsActive => !ReferenceEquals(Current, null);

        /// <summary>La réponse des règles de pose à la place visée ; Ok quand rien n'est visé.</summary>
        public PlacementResult Result { get; private set; }

        /// <summary>Prend cette construction en main : elle suivra le pointeur.</summary>
        public void Begin(StructureDefinition definition)
        {
            if (ReferenceEquals(definition, null))
                throw new ArgumentNullException(nameof(definition));
            if (ReferenceEquals(definition, Current))
                return;

            _preview.Clear();
            Current = definition;
            _aimed = false;
            SetResult(PlacementResult.Ok);
            CurrentChanged?.Invoke();
        }

        /// <summary>Vise le sol sous ce point de l'écran ; hors du sol, l'aperçu se cache.</summary>
        public void AimAt(Vector2 screenPoint)
        {
            if (!IsActive)
                return;

            Relief relief = _world.Map.Relief;
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            if (!relief.TryRaycast(ray.origin, ray.direction, _camera.farClipPlane, out Vector3 ground))
            {
                LoseAim();
                return;
            }

            _aimed = true;
            _aim = new Placement(new Vector2(ground.x, ground.z), _rotationDegrees);
            Refresh();
        }

        /// <summary>Le pointeur ne vise plus le sol (il est sur l'interface, ou dans le ciel).</summary>
        public void LoseAim()
        {
            _aimed = false;
            _preview.Hide();
            SetResult(PlacementResult.Ok);
        }

        /// <summary>Tourne d'un cran (GDD, section 9) ; l'angle vaut pour la visée suivante.</summary>
        public void RotateStep()
        {
            RotateBy(_settings.RotationStepDegrees);
        }

        /// <summary>Tourne de cet angle, en degrés ; un angle positif tourne vers l'est, comme la caméra.</summary>
        public void RotateBy(float degrees)
        {
            _rotationDegrees = Mathf.Repeat(_rotationDegrees + degrees, 360f);
            if (!IsActive || !_aimed)
                return;
            _aim = new Placement(_aim.Center, _rotationDegrees);
            Refresh();
        }

        /// <summary>Pose la construction visée, si les règles le permettent ; vrai si l'ordre est envoyé.</summary>
        public bool Confirm()
        {
            if (!IsActive || !_aimed || _sent || Result != PlacementResult.Ok)
                return false;
            _world.Submit(new PlaceStructureCommand(Current, _aim));
            _sent = true;
            return true;
        }

        /// <summary>Échap lâche l'outil s'il est en main ; faux s'il ne l'était pas.</summary>
        public bool Escape()
        {
            if (!IsActive)
                return false;
            Cancel();
            return true;
        }

        /// <summary>Pose l'outil de côté : plus d'aperçu, plus de construction en main.</summary>
        public void Cancel()
        {
            if (!IsActive)
                return;
            _preview.Clear();
            Current = null;
            _aimed = false;
            SetResult(PlacementResult.Ok);
            CurrentChanged?.Invoke();
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
            else if (IsActive && _aimed)
                Refresh();
        }

        /// <summary>Demande aux règles de pose si la place visée convient, et le montre.</summary>
        private void Refresh()
        {
            SetResult(_world.PlacementRules.Check(Current, _aim));
            _preview.Show(Current, _aim, _world.Map.Relief, Result == PlacementResult.Ok);
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
