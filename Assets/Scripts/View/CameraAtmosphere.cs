using Diciela.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Diciela.View
{
    /// <summary>
    /// Les ombres et le brouillard qui suivent le zoom (GDD, section 16) : de loin, les ombres couvrent tout le sol visible
    /// et le brouillard de la carte recule ; de près, les ombres restent fines et le brouillard est celui de la carte.
    /// Elle règle une copie en mémoire du réglage de rendu (URP), jamais l'asset, et remet tout comme avant quand la
    /// caméra cesse de montrer la carte ou que le Play s'arrête (SAVOIR, 2.10).
    /// </summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(CameraRig))]
    public sealed class CameraAtmosphere : MonoBehaviour
    {
        // La distance des ombres ne se règle de nouveau qu'au-delà d'un demi-mètre d'écart.
        private const float ShadowStepMeters = 0.5f;

        [Tooltip("La fiche des réglages de la caméra (Assets/Data/Settings).")]
        [SerializeField] private CameraSettings _settings;

        private CameraRig _rig;
        private bool _active;
        private RenderPipelineAsset _previousPipeline;
        private UniversalRenderPipelineAsset _copy;
        private Scene _fogScene;
        private float _fogStart;
        private float _fogEnd;

        private void Awake()
        {
            _rig = GetComponent<CameraRig>();
        }

        // Après la caméra (ordre 100), qui se pose dans son propre LateUpdate.
        private void LateUpdate()
        {
            if (_settings == null || !_rig.IsShowing)
            {
                Restore();
                return;
            }
            if (!_active)
                Begin();
            ApplyShadows();
            ApplyFog();
        }

        private void OnDisable()
        {
            Restore();
        }

        private void Begin()
        {
            _active = true;
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset current)
            {
                _previousPipeline = QualitySettings.renderPipeline;
                _copy = Instantiate(current);
                _copy.name = current.name + " (copie de jeu)";
                QualitySettings.renderPipeline = _copy;
            }
            _fogScene = SceneManager.GetActiveScene();
            _fogStart = RenderSettings.fogStartDistance;
            _fogEnd = RenderSettings.fogEndDistance;
        }

        private void Restore()
        {
            if (!_active)
                return;
            _active = false;
            if (_copy != null)
            {
                QualitySettings.renderPipeline = _previousPipeline;
                Destroy(_copy);
                _copy = null;
            }
            _previousPipeline = null;
            if (_fogScene.IsValid() && SceneManager.GetActiveScene() == _fogScene)
            {
                RenderSettings.fogStartDistance = _fogStart;
                RenderSettings.fogEndDistance = _fogEnd;
            }
        }

        /// <summary>Les ombres vont jusqu'au sol visible en haut de l'écran, entre les deux bornes de la fiche.</summary>
        private void ApplyShadows()
        {
            if (_copy == null)
                return;
            float distance = Mathf.Clamp(VisibleGroundReach(), _settings.ShadowMinMeters, _settings.ShadowMaxMeters);
            if (Mathf.Abs(_copy.shadowDistance - distance) > ShadowStepMeters)
                _copy.shadowDistance = distance;
        }

        /// <summary>La distance, depuis la caméra, du sol vu au bord haut de l'écran (sol plat à la hauteur du point visé).</summary>
        private float VisibleGroundReach()
        {
            float pitch = _rig.PitchDegrees * Mathf.Deg2Rad;
            float topBelowHorizon = pitch - _settings.FieldOfView * 0.5f * Mathf.Deg2Rad;
            if (topBelowHorizon <= 0f)
                return float.MaxValue;
            return _rig.Distance * Mathf.Sin(pitch) / Mathf.Sin(topBelowHorizon);
        }

        /// <summary>Le brouillard de la carte recule quand on s'éloigne ; de près, il reste le sien.</summary>
        private void ApplyFog()
        {
            float start = Mathf.Max(_fogStart, _settings.FogStartPerDistance * _rig.Distance);
            RenderSettings.fogStartDistance = start;
            RenderSettings.fogEndDistance = start + (_fogEnd - _fogStart);
        }
    }
}
