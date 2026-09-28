using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.Game
{
    /// <summary>
    /// Le point d'arrivée de la carte (GDD, section 2) : là où la Chimère apparaît. Le développeur le pose dans la scène
    /// Carte, sur un objet vide ; une carte en a un au plus. Sans lui, elle apparaît au centre de la carte. Dans la vue
    /// Scène, un repère le montre : un cercle au sol et un mât.
    /// </summary>
    public sealed class ArrivalPoint : MonoBehaviour
    {
        // Le repère de la vue Scène : le rayon du cercle, la hauteur du mât, en mètres, et sa couleur.
        private const float MarkRadiusMeters = 1f;
        private const float MarkHeightMeters = 4f;
        private static readonly Color MarkColor = new Color(1f, 0.72f, 0.1f);

        /// <summary>
        /// Le point d'arrivée actif de cette scène, ou null si elle n'en a pas ; une erreur dans la console s'il y en a
        /// plusieurs, et le premier trouvé compte.
        /// </summary>
        public static ArrivalPoint FindIn(Scene scene)
        {
            ArrivalPoint found = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (ArrivalPoint point in root.GetComponentsInChildren<ArrivalPoint>())
                {
                    if (found == null)
                        found = point;
                    else
                        Debug.LogError("DICIELA : la carte a plusieurs points d'arrivée ; seul « " + found.name +
                                       " » compte.", point);
                }
            }
            return found;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = MarkColor;
            Vector3 foot = transform.position;
            Gizmos.DrawWireSphere(foot, MarkRadiusMeters);
            Gizmos.DrawLine(foot, foot + Vector3.up * MarkHeightMeters);
        }
    }
}
