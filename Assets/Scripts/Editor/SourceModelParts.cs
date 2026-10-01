using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Ce qu'on lit dans un modèle du pack pour le préparer (SAVOIR, 2.9) : ses pièces vues de près, celles de sa
    /// porte, ses effets, la boîte exacte d'un groupe de pièces, et le côté où s'ouvre la porte.
    /// </summary>
    internal static class SourceModelParts
    {
        private const string DoorMark = "door";

        /// <summary>
        /// Les pièces dessinées du modèle, vues de près : chaque maillage d'un objet actif au rendu allumé, sauf ceux des
        /// niveaux de détail de loin (LOD1 et au-delà), que les pièces du pack portent en plus de leur version complète :
        /// les fusionner superposerait plusieurs versions d'une même pièce. Rend aussi le nombre de maillages laissés.
        /// </summary>
        public static List<MeshFilter> NearPieces(GameObject root, out int distantCount)
        {
            var distant = new HashSet<Renderer>();
            foreach (LODGroup group in root.GetComponentsInChildren<LODGroup>(true))
            {
                LOD[] levels = group.GetLODs();
                for (int level = 1; level < levels.Length; level++)
                {
                    foreach (Renderer renderer in levels[level].renderers)
                    {
                        if (renderer != null)
                            distant.Add(renderer);
                    }
                }
            }
            distantCount = distant.Count;

            var pieces = new List<MeshFilter>();
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(false))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (filter.sharedMesh != null && renderer != null && renderer.enabled && !distant.Contains(renderer))
                    pieces.Add(filter);
            }
            return pieces;
        }

        /// <summary>
        /// Les effets du modèle (la fumée d'une cheminée, la flamme d'un feu) : chaque objet allumé qui porte des
        /// particules sans qu'un de ses parents en porte ; la préparation les garde à leur place, à côté du maillage.
        /// </summary>
        public static List<GameObject> Effects(GameObject root)
        {
            var effects = new List<GameObject>();
            foreach (ParticleSystem system in root.GetComponentsInChildren<ParticleSystem>(false))
            {
                Transform parent = system.transform.parent;
                if (parent == null || parent.GetComponentInParent<ParticleSystem>() == null)
                    effects.Add(system.gameObject);
            }
            return effects;
        }

        /// <summary>Les pièces de la porte : celles dont le nom, ou celui d'un parent, contient « Door ».</summary>
        public static List<MeshFilter> DoorPieces(List<MeshFilter> pieces, Transform root)
        {
            var door = new List<MeshFilter>();
            foreach (MeshFilter piece in pieces)
            {
                for (Transform part = piece.transform; part != null && part != root; part = part.parent)
                {
                    if (part.name.ToLowerInvariant().Contains(DoorMark))
                    {
                        door.Add(piece);
                        break;
                    }
                }
            }
            return door;
        }

        /// <summary>
        /// L'angle qui tourne la façade de la porte vers le nord (+z) : la porte est du côté où elle s'écarte le plus du
        /// centre du modèle. Un angle positif tourne vers l'est, comme celui d'une construction.
        /// </summary>
        public static float YawTowardsNorth(Bounds model, Bounds door)
        {
            Vector3 side = door.center - model.center;
            if (Mathf.Abs(side.x) > Mathf.Abs(side.z))
                return side.x > 0f ? -90f : 90f;
            return side.z > 0f ? 0f : 180f;
        }

        /// <summary>
        /// La boîte exacte de ces pièces, dans la scène : celle de leurs sommets. Les coins de la boîte d'une pièce
        /// tournée débordent (un accessoire posé de biais gonflerait l'empreinte).
        /// </summary>
        public static Bounds Box(List<MeshFilter> pieces)
        {
            var box = new Bounds();
            bool any = false;
            var vertices = new List<Vector3>();
            foreach (MeshFilter piece in pieces)
            {
                piece.sharedMesh.GetVertices(vertices);
                Matrix4x4 toScene = piece.transform.localToWorldMatrix;
                foreach (Vector3 vertex in vertices)
                {
                    Vector3 point = toScene.MultiplyPoint3x4(vertex);
                    if (any)
                        box.Encapsulate(point);
                    else
                        box = new Bounds(point, Vector3.zero);
                    any = true;
                }
            }
            return box;
        }
    }
}
