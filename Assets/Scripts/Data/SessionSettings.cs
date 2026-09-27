using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// La fiche de session : ce que charge une scène racine du jeu (ARCHITECTURE, section 10). La scène Jeu a la sienne,
    /// la scène de test la sienne : un test ne peut pas atteindre la vraie carte.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Session", fileName = "Session")]
    public sealed class SessionSettings : ScriptableObject
    {
        [Tooltip("Le chemin de la scène de la carte que la session charge, par exemple Assets/Scenes/Carte.unity.")]
        [SerializeField] private string _mapScenePath = "";

        public string MapScenePath => _mapScenePath;
    }
}
