using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// L'aperçu de l'écran de création (GDD, section 6) : la Chimère à venir, habillée de l'apparence choisie, debout
    /// au point d'arrivée, tournée comme elle y naîtra. Ce n'est qu'une vue : la simulation la crée à la validation, et
    /// sa vue prend alors la place de l'aperçu.
    /// </summary>
    public sealed class AppearancePreview : MonoBehaviour
    {
        private GameObject _shown;
        private Vector3 _place;

        /// <summary>Se tient prêt à montrer une apparence à ce point de la carte, posé sur son relief.</summary>
        public void Stand(Vector2 point, Relief relief)
        {
            _place = new Vector3(point.x, relief.HeightAt(point.x, point.y), point.y);
        }

        /// <summary>Montre cette apparence à la place de la précédente.</summary>
        public void Show(Appearance appearance)
        {
            Hide();
            _shown = Dresser.Dress(appearance, transform);
            if (_shown != null)
                _shown.transform.SetPositionAndRotation(_place, Quaternion.identity);
        }

        /// <summary>Retire ce qu'on montrait.</summary>
        public void Hide()
        {
            if (_shown != null)
                Destroy(_shown);
            _shown = null;
        }

        private void OnDestroy()
        {
            Hide();
        }
    }
}
