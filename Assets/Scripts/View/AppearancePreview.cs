using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// L'aperçu de l'écran de création (GDD, section 6) : la Chimère à venir, habillée de l'apparence choisie, debout sur
    /// le socle de l'atelier (Atelier), de face ; le joueur la fait tourner, et elle garde son angle quand il l'habille
    /// autrement. Ce n'est qu'une vue : la simulation la crée à la validation, au point d'arrivée de la carte.
    /// </summary>
    public sealed class AppearancePreview : MonoBehaviour
    {
        private GameObject _shown;
        private Vector3 _place;
        private float _yawDegrees;

        /// <summary>Se tient prêt à montrer une apparence à cette place, de face.</summary>
        public void Stand(Vector3 place)
        {
            _place = place;
            _yawDegrees = 0f;
            Place();
        }

        /// <summary>
        /// La fait tourner sur elle-même de cet angle, en degrés : positif, dans le sens des aiguilles d'une montre, vue
        /// d'en haut.
        /// </summary>
        public void Turn(float degrees)
        {
            _yawDegrees = Mathf.Repeat(_yawDegrees + degrees, 360f);
            Place();
        }

        /// <summary>Montre cette apparence à la place de la précédente, tournée comme elle.</summary>
        public void Show(Appearance appearance)
        {
            Hide();
            _shown = Dresser.Dress(appearance, transform);
            Place();
        }

        /// <summary>Retire ce qu'on montrait.</summary>
        public void Hide()
        {
            if (_shown != null)
                Destroy(_shown);
            _shown = null;
        }

        private void Place()
        {
            if (_shown != null)
                _shown.transform.SetPositionAndRotation(_place, Quaternion.Euler(0f, _yawDegrees, 0f));
        }

        private void OnDestroy()
        {
            Hide();
        }
    }
}
