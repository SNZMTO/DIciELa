using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La charrette de l'étal (GDD, section 11 ; décision 194) : chaque entrepôt, le campement compris, en garde une ;
    /// un charretier la tire jusqu'à une place de charrette de la Place, où elle sert d'étal. Ses places, sa vitesse quand
    /// on la tire, la distance entre elle et qui la tire, et son modèle du pack, que la vue montre.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Charrette", fileName = "Charrette")]
    public sealed class CartDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("Les places de son stock : ce qu'elle tient pour l'étal, selon l'encombrement (GDD, sections 8 et 11).")]
        [SerializeField, Min(1)] private int _capacity = 20;

        [Tooltip("Sa vitesse quand on la tire, en part du pas de qui la tire : moitié moins vite dans l'herbe (GDD, " +
                 "section 9).")]
        [SerializeField, Range(0.1f, 2f)] private float _speedFactor = 0.5f;

        [Tooltip("La distance entre qui la tire et son milieu, en mètres.")]
        [SerializeField, Min(0f)] private float _pullMeters = 2.2f;

        [Tooltip("Où elle se gare sur la Place : sur un cercle autour de son milieu, à cette part de son demi-côté (0 : au " +
                 "milieu ; 1 : au bord).")]
        [SerializeField, Range(0f, 1f)] private float _parkRadius = 0.55f;

        [Tooltip("De combien elle se met de travers, au plus, en se garant : tournée vers le milieu ou vers le bord, puis " +
                 "de travers d'un écart tiré au hasard, en degrés.")]
        [SerializeField, Range(0f, 90f)] private float _parkTurnDegrees = 30f;

        [Tooltip("Son modèle du pack, tel quel, l'origine au sol, l'avant vers +z : la vue le montre ; la simulation ne " +
                 "s'en sert pas.")]
        [SerializeField] private GameObject _model;

        public string Id => _id;
        public int Capacity => _capacity;
        public float SpeedFactor => _speedFactor;
        public float PullMeters => _pullMeters;
        public float ParkRadius => _parkRadius;
        public float ParkTurnDegrees => _parkTurnDegrees;
        public GameObject Model => _model;
    }
}
