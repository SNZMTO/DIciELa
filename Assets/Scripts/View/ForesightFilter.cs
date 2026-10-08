using System;
using Diciela.Data;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Diciela.View
{
    /// <summary>
    /// Le filtre de rêverie de la prévision (GDD, section 16 ; décision 228) : en prévision, l'image s'adoucit
    /// (couleurs moins vives, un peu plus claires, les bords de l'écran voilés de lavande), pour qu'on sache toujours
    /// qu'on regarde la ville à venir. C'est un volume de filtres global, au-dessus de celui de la carte, dont le
    /// profil est une fiche (Assets/Art/Reverie.asset) ; il entre et sort en fondu, le temps que disent les réglages de
    /// la caméra, sans que la pause du jeu l'arrête.
    /// </summary>
    [RequireComponent(typeof(CameraRig))]
    public sealed class ForesightFilter : MonoBehaviour
    {
        // Au-dessus du volume de la carte, de priorité 0 : la rêverie s'ajoute à ses filtres.
        private const float Priority = 10f;
        private const string VolumeName = "Rêverie";

        [Tooltip("Le profil de filtres de la rêverie (Assets/Art/Reverie.asset).")]
        [SerializeField] private VolumeProfile _profile;

        [Tooltip("La fiche des réglages de la caméra (Assets/Data/Settings) : le temps du fondu.")]
        [SerializeField] private CameraSettings _settings;

        private Volume _volume;
        private Foresight _foresight;

        /// <summary>Suit cette prévision ; le filtre est éteint tant qu'elle l'est.</summary>
        public void Show(Foresight foresight)
        {
            if (_profile == null || _settings == null)
                throw new InvalidOperationException("Le filtre de la prévision n'a pas son profil ou ses réglages.");
            _foresight = foresight ?? throw new ArgumentNullException(nameof(foresight));
            if (_volume == null)
            {
                var holder = new GameObject(VolumeName);
                holder.transform.SetParent(transform, false);
                _volume = holder.AddComponent<Volume>();
                _volume.isGlobal = true;
                _volume.priority = Priority;
                _volume.sharedProfile = _profile;
            }
            _volume.weight = 0f;
        }

        /// <summary>Cesse de suivre la prévision ; le filtre s'éteint tout de suite.</summary>
        public void Hide()
        {
            _foresight = null;
            if (_volume != null)
                _volume.weight = 0f;
        }

        private void Update()
        {
            if (_volume == null || _foresight == null)
                return;
            float target = _foresight.IsOn ? 1f : 0f;
            float seconds = Mathf.Max(_settings.ForesightBlendSeconds, 0.01f);
            _volume.weight = Mathf.MoveTowards(_volume.weight, target, Time.unscaledDeltaTime / seconds);
        }

        private void OnDestroy()
        {
            if (_volume != null)
                Object.Destroy(_volume.gameObject);
            _volume = null;
        }
    }
}
