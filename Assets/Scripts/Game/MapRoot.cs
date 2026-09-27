using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.Game
{
    /// <summary>
    /// La racine d'une scène de carte : posée sur son Terrain, elle dit au démarrage où lire la carte. Une scène de carte
    /// en a une seule, sur un objet à la racine de la scène.
    /// </summary>
    [RequireComponent(typeof(Terrain))]
    public sealed class MapRoot : MonoBehaviour
    {
        public Terrain Terrain => GetComponent<Terrain>();

        /// <summary>La racine de cette scène, ou null si elle n'en a pas.</summary>
        public static MapRoot FindIn(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                MapRoot mapRoot = root.GetComponent<MapRoot>();
                if (mapRoot != null)
                    return mapRoot;
            }
            return null;
        }
    }
}
