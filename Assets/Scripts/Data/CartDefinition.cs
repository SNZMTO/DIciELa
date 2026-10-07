using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La charrette de l'étal (GDD, section 11 ; décisions 194, 197, 198 et 204) : chaque entrepôt, le campement compris,
    /// en garde une ; un charretier la tire jusqu'à une place de charrette de la Place, une marque que le joueur y pose,
    /// où elle sert d'étal. Ses places, sa vitesse quand on la tire dans l'herbe, la distance entre elle et qui la tire,
    /// la taille de la marque où elle se gare ; ses modèles du pack, dont chaque charrette prend un au hasard, et comment
    /// la vue la fait glisser à sa place.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Charrette", fileName = "Charrette")]
    public sealed class CartDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("Les places de son stock : ce qu'elle tient pour l'étal, selon l'encombrement (GDD, sections 8 et 11).")]
        [SerializeField, Min(1)] private int _capacity = 20;

        [Tooltip("Sa vitesse dans l'herbe quand on la tire, en part du pas de qui la tire : moitié moins vite (GDD, " +
                 "section 9) ; sur un sol, la fiche du sol la dit.")]
        [SerializeField, Range(0.1f, 2f)] private float _speedFactor = 0.5f;

        [Tooltip("La distance entre qui la tire et son milieu, en mètres ; la vue met l'avant du modèle à qui la tire.")]
        [SerializeField, Min(0f)] private float _pullMeters = 2.2f;

        [Tooltip("La longueur d'une place de charrette, la marque que le joueur pose sur la Place, vers l'avant de la " +
                 "charrette, en mètres : celle de la plus grande charrette de ses modèles (décision 204).")]
        [SerializeField, Min(0.5f)] private float _spotLengthMeters = 5.8f;

        [Tooltip("La largeur d'une place de charrette, en mètres : celle de la plus grande charrette de ses modèles.")]
        [SerializeField, Min(0.5f)] private float _spotWidthMeters = 2.8f;

        [Tooltip("Ses modèles du pack, tels quels, l'origine au sol, l'avant vers +z : chaque charrette en prend un au " +
                 "hasard, toujours le même. La simulation ne compte que combien il y en a.")]
        [SerializeField] private List<GameObject> _models = new List<GameObject>();

        [Tooltip("À l'écran, la vitesse à laquelle elle glisse jusqu'à sa place quand on la gare, en mètres par seconde.")]
        [SerializeField, Min(0.1f)] private float _settleMetersPerSecond = 1.5f;

        [Tooltip("À l'écran, la vitesse à laquelle elle se tourne pour se garer, en degrés par seconde.")]
        [SerializeField, Min(1f)] private float _settleDegreesPerSecond = 120f;

        public string Id => _id;
        public int Capacity => _capacity;
        public float SpeedFactor => _speedFactor;
        public float PullMeters => _pullMeters;
        public float SpotLengthMeters => _spotLengthMeters;
        public float SpotWidthMeters => _spotWidthMeters;
        public IReadOnlyList<GameObject> Models => _models;
        public float SettleMetersPerSecond => _settleMetersPerSecond;
        public float SettleDegreesPerSecond => _settleDegreesPerSecond;
    }
}
