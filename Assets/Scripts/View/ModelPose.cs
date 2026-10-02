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
            return relief.GroundAt(center) + Rotation(placement) * local;
        }

        /// <summary>Comment le modèle est tourné à cette place : de son angle, autour de la verticale.</summary>
        public static Quaternion Rotation(Placement placement)
        {
            return Quaternion.Euler(0f, placement.RotationDegrees, 0f);
        }

        /// <summary>
        /// La boîte des maillages de ce modèle (ses morceaux cachés compris), dans son propre repère ; faux s'il n'a
        /// aucun maillage. La seule mesure d'un modèle pour le viser : une construction, un arbre, un rocher.
        /// </summary>
        public static bool TryLocalBox(GameObject model, out Bounds box)
        {
            box = new Bounds();
            bool any = false;
            Matrix4x4 toModel = model.transform.worldToLocalMatrix;
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null)
                    continue;
                // Le repère du morceau, ramené à celui du modèle : ses huit coins suffisent à sa boîte.
                Matrix4x4 toLocal = toModel * filter.transform.localToWorldMatrix;
                Bounds mesh = filter.sharedMesh.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    var offset = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f,
                        (corner & 4) == 0 ? -1f : 1f);
                    Vector3 point = toLocal.MultiplyPoint3x4(mesh.center + Vector3.Scale(mesh.extents, offset));
                    if (any)
                        box.Encapsulate(point);
                    else
                        box = new Bounds(point, Vector3.zero);
                    any = true;
                }
            }
            return any;
        }
    }
}
