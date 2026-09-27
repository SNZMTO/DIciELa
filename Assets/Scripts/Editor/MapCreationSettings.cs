using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les réglages d'une carte neuve : ils ne servent qu'à la créer. Une fois la carte créée, c'est son Terrain qui fait
    /// foi ; changer cette fiche ne touche jamais une carte existante.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Réglages d'une carte neuve", fileName = "CarteNeuve")]
    public sealed class MapCreationSettings : ScriptableObject
    {
        [Tooltip("Côté de la carte, en mètres (GDD, section 2 : environ 500 m, à fixer avant de peindre).")]
        [SerializeField, Min(16f)] private float _sizeMeters = 500f;

        [Tooltip("Hauteur que le relief peut atteindre, en mètres.")]
        [SerializeField, Min(1f)] private float _maxHeightMeters = 100f;

        [Tooltip("Hauteur du sol d'une carte neuve, en mètres : de quoi creuser le lac, la rivière et les canaux.")]
        [SerializeField, Min(0f)] private float _groundHeightMeters = 20f;

        [Tooltip("Points du relief par côté : 33, 65, 129, 257, 513, 1025, 2049 ou 4097.")]
        [SerializeField] private int _heightmapResolution = 1025;

        [Tooltip("Pixels de peinture du sol par côté.")]
        [SerializeField] private int _alphamapResolution = 1024;

        public float SizeMeters => _sizeMeters;
        public float MaxHeightMeters => _maxHeightMeters;
        public float GroundHeightMeters => _groundHeightMeters;
        public int HeightmapResolution => _heightmapResolution;
        public int AlphamapResolution => _alphamapResolution;
    }
}
