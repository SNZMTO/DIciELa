using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La seule routine de pose des modèles (SAVOIR, 2.9), pour l'aperçu comme pour les constructions posées. Le modèle
    /// est déjà préparé (DICIELA/Contenu/Préparer les modèles) : centré sur son empreinte, porte au nord, son origine au
    /// sol. On pose donc son origine au centre de l'empreinte, sur le sol, tournée de son angle (0 : la porte au nord) ;
    /// ses fondations s'enfoncent, et cachent le vide sous lui sur une pente. Un point du modèle (sa couche) se pose de
    /// même.
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
            root.SetPositionAndRotation(PointOf(placement, relief, Vector3.zero), Rotation(placement));
        }

        /// <summary>Où se trouve, le modèle posé à cette place, ce point de son repère (à l'angle 0).</summary>
        public static Vector3 PointOf(Placement placement, Relief relief, Vector3 local)
        {
            Vector2 center = placement.Center;
            return new Vector3(center.x, relief.HeightAt(center.x, center.y), center.y) + Rotation(placement) * local;
        }

        /// <summary>Comment le modèle est tourné à cette place : de son angle, autour de la verticale.</summary>
        public static Quaternion Rotation(Placement placement)
        {
            return Quaternion.Euler(0f, placement.RotationDegrees, 0f);
        }
    }
}
