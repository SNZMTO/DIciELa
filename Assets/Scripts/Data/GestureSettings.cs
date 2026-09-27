using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages communs des gestes (SAVOIR, 2.8) : le seuil qui sépare le clic du glisser, pour les trois boutons
    /// de la souris comme pour une touche tenue en bougeant la souris. La fiche fait foi.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages des gestes", fileName = "GestureSettings")]
    public sealed class GestureSettings : ScriptableObject
    {
        [Tooltip("Distance en pixels au-delà de laquelle un clic devient un glisser.")]
        [SerializeField, Min(0f)] private float _dragThresholdPixels = 6f;

        public float DragThresholdPixels => _dragThresholdPixels;
    }
}
