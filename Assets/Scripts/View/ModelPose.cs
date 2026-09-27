using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La seule routine de pose des modèles (SAVOIR, 2.9), pour l'aperçu comme pour les constructions posées. Un modèle
    /// du pack a son origine n'importe où : on le recentre d'après la boîte de ses seuls maillages, sous une racine ;
    /// la racine se pose au centre de l'empreinte, sur le sol, tournée de son angle (0 : la porte au nord).
    /// </summary>
    public static class ModelPose
    {
        /// <summary>Une racine qui porte une copie du modèle, sa base centrée sous la racine.</summary>
        public static GameObject Instantiate(GameObject model, Transform parent, string name)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            GameObject copy = Object.Instantiate(model, root.transform, false);
            Bounds bounds = MeshBounds(copy, root.transform);
            copy.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            return root;
        }

        /// <summary>Pose la racine à cette place : au centre de l'empreinte, sur le sol, tournée de son angle.</summary>
        public static void Place(Transform root, Placement placement, Relief relief)
        {
            Vector2 center = placement.Center;
            root.SetPositionAndRotation(new Vector3(center.x, relief.HeightAt(center.x, center.y), center.y),
                Quaternion.Euler(0f, placement.RotationDegrees, 0f));
        }

        /// <summary>La boîte des maillages de ce modèle (jamais ses particules), dans le repère de la racine.</summary>
        private static Bounds MeshBounds(GameObject model, Transform root)
        {
            var bounds = new Bounds();
            bool any = false;
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
                Encapsulate(ref bounds, ref any, filter.sharedMesh, filter.transform, root);
            foreach (SkinnedMeshRenderer skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                Encapsulate(ref bounds, ref any, skinned.sharedMesh, skinned.transform, root);
            return bounds;
        }

        private static void Encapsulate(ref Bounds bounds, ref bool any, Mesh mesh, Transform part, Transform root)
        {
            if (mesh == null)
                return;
            Matrix4x4 toRoot = root.worldToLocalMatrix * part.localToWorldMatrix;
            Bounds local = mesh.bounds;
            for (int corner = 0; corner < 8; corner++)
            {
                var offset = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f,
                    (corner & 4) == 0 ? -1f : 1f);
                Vector3 point = toRoot.MultiplyPoint3x4(local.center + Vector3.Scale(local.extents, offset));
                if (any)
                    bounds.Encapsulate(point);
                else
                    bounds = new Bounds(point, Vector3.zero);
                any = true;
            }
        }
    }
}
