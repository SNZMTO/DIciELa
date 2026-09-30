using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de l'interface : la table des textes et la langue dans laquelle on la lit (ARCHITECTURE, section
    /// 11), et sa taille à l'écran (GDD, section 16 ; décision 144). Toute chaîne visible passe par une clé de cette
    /// table.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages de l'interface", fileName = "InterfaceSettings")]
    public sealed class InterfaceSettings : ScriptableObject
    {
        [Tooltip("La table des textes : Assets/Localization/strings.csv.")]
        [SerializeField] private TextAsset _texts;

        [Tooltip("La langue affichée : le nom d'une colonne de la table des textes (fr, en).")]
        [SerializeField] private string _language = "fr";

        [Tooltip("La hauteur d'écran de la maquette, en pixels : l'interface y est à sa taille de référence.")]
        [SerializeField, Min(1f)] private float _referenceHeight = 900f;

        [Tooltip("La place que l'interface se garde au moins, en pixels à sa taille : elle ne grossit pas au-delà.")]
        [SerializeField] private Vector2 _minimumRoom = new Vector2(1280f, 720f);

        [Tooltip("Le réglage « Taille de l'interface » : 1 = 100 %.")]
        [SerializeField, Range(0.8f, 1.5f)] private float _size = 1f;

        public TextAsset Texts => _texts;
        public string Language => _language;
        public float ReferenceHeight => _referenceHeight;
        public Vector2 MinimumRoom => _minimumRoom;
        public float Size => _size;
    }
}
