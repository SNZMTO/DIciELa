using System;
using Diciela.Data;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Le fantôme qui suit le pointeur sur le sol (GDD, section 9) : la place visée et l'angle, qui reste d'une visée à
    /// l'autre, la réponse des règles de pose, et l'aperçu qui la montre. La pose et l'outil Déplacer s'en servent ;
    /// chacun dit quelle construction suit le pointeur et quelle règle répond.
    /// </summary>
    internal sealed class GhostAim
    {
        private readonly Camera _camera;
        private readonly Relief _relief;
        private readonly PlacementPreview _preview;
        private readonly PlacementSettings _settings;

        public GhostAim(Camera camera, Relief relief, PlacementPreview preview, PlacementSettings settings)
        {
            _camera = camera;
            _relief = relief;
            _preview = preview;
            _settings = settings;
        }

        /// <summary>Levé quand la réponse des règles de pose à la place visée change.</summary>
        public event Action ResultChanged;

        /// <summary>L'angle du fantôme, en degrés depuis le nord vers l'est.</summary>
        public float RotationDegrees { get; private set; }

        /// <summary>Vrai tant que le pointeur vise le sol.</summary>
        public bool IsAimed { get; private set; }

        /// <summary>La place visée, à l'angle du fantôme ; elle ne vaut que visée.</summary>
        public Placement Aim { get; private set; }

        /// <summary>La réponse des règles de pose à la place visée ; Ok quand rien n'est visé.</summary>
        public PlacementResult Result { get; private set; }

        /// <summary>Vise le sol sous ce point de l'écran ; faux s'il le manque, et l'aperçu se cache.</summary>
        public bool AimAt(Vector2 screenPoint)
        {
            if (!GroundAim.TryGround(_camera, _relief, screenPoint, out Vector2 ground))
            {
                LoseAim();
                return false;
            }
            IsAimed = true;
            Aim = new Placement(ground, RotationDegrees);
            return true;
        }

        /// <summary>Le pointeur ne vise plus le sol (il est sur l'interface, ou dans le ciel).</summary>
        public void LoseAim()
        {
            IsAimed = false;
            _preview.Hide();
            SetResult(PlacementResult.Ok);
        }

        /// <summary>Tourne de cet angle ; vrai si une place est visée, qu'il faut alors revérifier.</summary>
        public bool RotateBy(float degrees)
        {
            RotationDegrees = Mathf.Repeat(RotationDegrees + degrees, 360f);
            if (!IsAimed)
                return false;
            Aim = new Placement(Aim.Center, RotationDegrees);
            return true;
        }

        /// <summary>Le cran d'une touche brève (GDD, section 9).</summary>
        public float StepDegrees => _settings.RotationStepDegrees;

        /// <summary>Repart de cet angle, sans rien viser encore (une construction qu'on touche pour la déplacer).</summary>
        public void Restart(float degrees)
        {
            RotationDegrees = degrees;
            IsAimed = false;
        }

        /// <summary>Montre cette construction à la place visée, de la couleur de cette réponse.</summary>
        public void Show(StructureDefinition definition, PlacementResult result)
        {
            SetResult(result);
            _preview.Show(definition, Aim, _relief, result == PlacementResult.Ok);
        }

        /// <summary>Retire le fantôme : plus rien ne suit le pointeur.</summary>
        public void Clear()
        {
            IsAimed = false;
            _preview.Clear();
            SetResult(PlacementResult.Ok);
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
