using System;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Un sol qu'on traverse, comme la Place, épouse le relief (décision 193) : son modèle, posé à plat au centre de son
    /// empreinte, voit chacun de ses sommets monter ou descendre de l'écart du relief entre sa place et ce centre. Son
    /// dallage suit la pente, ses cailloux restent au sol, et qui y marche (à la hauteur du relief) garde les pieds
    /// dessus. Chaque maillage est copié pour ce modèle : celui du modèle préparé ne change pas. Un maillage illisible
    /// (un objet du pack sans « Read/Write ») reste à plat.
    /// </summary>
    internal static class FloorDrape
    {
        /// <summary>Fait épouser le relief à tous les maillages de ce modèle posé ; rend les copies faites, à détruire avec lui.</summary>
        public static Mesh[] Apply(GameObject view, Relief relief)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Vector3 center = view.transform.position;
            float ground = relief.HeightAt(center.x, center.z);
            MeshFilter[] filters = view.GetComponentsInChildren<MeshFilter>(true);
            var copies = new Mesh[filters.Length];
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh shared = filters[i].sharedMesh;
                if (shared == null || !shared.isReadable)
                    continue;
                Transform place = filters[i].transform;
                Vector3[] vertices = shared.vertices;
                for (int v = 0; v < vertices.Length; v++)
                {
                    Vector3 world = place.TransformPoint(vertices[v]);
                    world.y += relief.HeightAt(world.x, world.z) - ground;
                    vertices[v] = place.InverseTransformPoint(world);
                }
                Mesh copy = UnityEngine.Object.Instantiate(shared);
                copy.name = shared.name + " (au relief)";
                copy.vertices = vertices;
                copy.RecalculateBounds();
                filters[i].sharedMesh = copy;
                copies[i] = copy;
            }
            return copies;
        }
    }
}
