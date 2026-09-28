using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La seule routine de pose des modèles (SAVOIR, 2.9), pour l'aperçu comme pour les constructions posées. Le modèle
    /// est déjà préparé (DICIELA/Contenu/Préparer les modèles) : centré sur son empreinte, porte au nord, son origine au
    /// sol. On pose donc son origine au centre de l'empreinte, sur le sol, tournée de son angle (0 : la porte au nord) ;
    /// ses fondations s'enfoncent, et cachent le vide sous lui sur une pente.
    /// </summary>
    public static class ModelPose
    {
        /// <summary>Une copie du modèle préparé, rangée sous parent.</summary>
        public static GameObject Instantiate(GameObject model, Transform parent, string name)
        {
            GameObject copy = Object.Instantiate(model, parent, false);
            copy.name = name;
            return copy;
        }

        /// <summary>Pose la copie à cette place : son origine au centre de l'empreinte, sur le sol, tournée de son angle.</summary>
        public static void Place(Transform root, Placement placement, Relief relief)
        {
            Vector2 center = placement.Center;
            root.SetPositionAndRotation(new Vector3(center.x, relief.HeightAt(center.x, center.y), center.y),
                Quaternion.Euler(0f, placement.RotationDegrees, 0f));
        }
    }
}
