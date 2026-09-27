using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Les réglages de la pose (GDD, section 9) : tourner, et les couleurs de l'aperçu et du fantôme. Les valeurs
    /// écrites ici ne servent qu'à une fiche neuve : la fiche fait foi.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages de la pose", fileName = "PlacementSettings")]
    public sealed class PlacementSettings : ScriptableObject
    {
        [Header("Tourner")]
        [Tooltip("Le cran d'une touche brève, en degrés (GDD, section 9).")]
        [SerializeField, Range(1f, 90f)] private float _rotationStepDegrees = 15f;

        [Tooltip("La touche tenue, en bougeant la souris de gauche à droite : degrés par pixel.")]
        [SerializeField, Range(0.01f, 2f)] private float _rotationDegreesPerPixel = 0.5f;

        [Header("Couleurs")]
        [Tooltip("Le matériau translucide de l'aperçu et du fantôme ; sa couleur vient des réglages ci-dessous.")]
        [SerializeField] private Material _translucentMaterial;

        [Tooltip("L'aperçu, quand la pose est permise.")]
        [SerializeField] private Color _allowedColor = new Color(0.35f, 0.85f, 0.4f, 0.5f);

        [Tooltip("L'aperçu, quand la pose est refusée.")]
        [SerializeField] private Color _refusedColor = new Color(0.95f, 0.3f, 0.25f, 0.5f);

        [Tooltip("Le fantôme : une construction posée qui attend d'être bâtie.")]
        [SerializeField] private Color _plannedColor = new Color(0.7f, 0.85f, 1f, 0.45f);

        public float RotationStepDegrees => _rotationStepDegrees;
        public float RotationDegreesPerPixel => _rotationDegreesPerPixel;
        public Material TranslucentMaterial => _translucentMaterial;
        public Color AllowedColor => _allowedColor;
        public Color RefusedColor => _refusedColor;
        public Color PlannedColor => _plannedColor;
    }
}
