using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Une sorte de sol (GDD, section 9 ; décisions 209 et 210) : ce qu'on trace à l'outil Chemins, partout où l'on marche,
    /// ou à l'outil de la Place, dans sa zone. La terre, gratuite, dès le départ ; puis les pavés et les autres, chacun
    /// avec son idée. La fiche dit son nom, son aspect (le calque où il se peint dans le sol de la carte) et le pas d'une
    /// charrette tirée sur lui ; dans l'herbe, c'est la fiche de la charrette qui le dit. Son prix viendra avec les sols
    /// qui se paient.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Sol", fileName = "Sol")]
    public sealed class FloorDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes.")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son aspect : le calque du sol de la carte où il se peint quand il sert (GDD, section 16) ; celui " +
                 "du pack, ou une copie rangée hors du pack.")]
        [SerializeField] private TerrainLayer _layer;

        [Tooltip("La vitesse d'une charrette tirée sur ce sol, en part du pas de qui la tire : normale sur la terre, une " +
                 "fois et demie sur un sol dur (GDD, section 9).")]
        [SerializeField, Range(0.1f, 3f)] private float _cartSpeedFactor = 1f;

        public string Id => _id;
        public string NameKey => _nameKey;
        public TerrainLayer Layer => _layer;
        public float CartSpeedFactor => _cartSpeedFactor;
    }
}
