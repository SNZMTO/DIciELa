using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// L'atelier de l'écran de création (GDD, section 6) : un décor low poly loin de la carte (un fond, un sol, un socle),
    /// sa lumière et sa caméra, qui montre de face la Chimère à venir debout sur le socle. Il se montre à l'ouverture
    /// d'une partie et se cache quand la ville s'ouvre : sa caméra ne rend qu'avec lui. Généré par un menu d'éditeur
    /// (AtelierBuilder) ; ses couleurs sont dans ses matériaux.
    /// </summary>
    public sealed class Atelier : MonoBehaviour
    {
        [Tooltip("Le point du socle où se tient la Chimère à venir.")]
        [SerializeField] private Transform _spot;

        /// <summary>Le point du socle où se tient la Chimère à venir.</summary>
        public Vector3 Spot => _spot != null ? _spot.position : transform.position;

        /// <summary>Montre l'atelier et allume sa caméra.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>Cache l'atelier, sa caméra avec lui ; détruit avec sa scène, il n'a plus rien à cacher.</summary>
        public void Hide()
        {
            if (this != null)
                gameObject.SetActive(false);
        }
    }
}
