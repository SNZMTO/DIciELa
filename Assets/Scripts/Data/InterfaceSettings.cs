using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de l'interface : la table des textes et la langue dans laquelle on la lit (ARCHITECTURE, section
    /// 11). Toute chaîne visible passe par une clé de cette table.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages de l'interface", fileName = "InterfaceSettings")]
    public sealed class InterfaceSettings : ScriptableObject
    {
        [Tooltip("La table des textes : Assets/Localization/strings.csv.")]
        [SerializeField] private TextAsset _texts;

        [Tooltip("La langue affichée : le nom d'une colonne de la table des textes (fr, en).")]
        [SerializeField] private string _language = "fr";

        public TextAsset Texts => _texts;
        public string Language => _language;
    }
}
