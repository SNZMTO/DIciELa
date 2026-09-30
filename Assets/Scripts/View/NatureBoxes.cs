using System.Collections.Generic;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les boîtes des arbres et des rochers peints, pour les viser (SAVOIR, 2.7) : la boîte du maillage de son modèle, à sa
    /// taille, élargie pour couvrir tous ses angles, posée sur le sol à son pied. On vise le modèle, pas le sol : le haut
    /// d'un arbre cache le sol derrière lui. Faites une fois à l'ouverture ; un arbre retiré ne se vise plus.
    /// </summary>
    internal sealed class NatureBoxes
    {
        private readonly List<Bounds> _boxes = new List<Bounds>();
        private readonly Nature _nature;

        public NatureBoxes(Nature nature, TerrainData data, IReadOnlyList<int> instances, Relief relief)
        {
            _nature = nature;
            TreePrototype[] prototypes = data.treePrototypes;
            var models = new Bounds[prototypes.Length];
            for (int p = 0; p < prototypes.Length; p++)
                models[p] = ModelBox(prototypes[p].prefab);

            foreach (NatureItem item in nature.All)
            {
                TreeInstance tree = data.GetTreeInstance(instances[item.Id - 1]);
                Bounds model = models[tree.prototypeIndex];
                // Tourné, le modèle reste dans le carré de sa plus grande demi-largeur, fois √2 pour ses coins.
                float half = Mathf.Max(model.extents.x, model.extents.z) * 1.4143f * tree.widthScale;
                Vector2 foot = item.Placement.Center;
                float ground = relief.HeightAt(foot.x, foot.y);
                var min = new Vector3(foot.x - half, ground + model.min.y * tree.heightScale, foot.y - half);
                var max = new Vector3(foot.x + half, ground + model.max.y * tree.heightScale, foot.y + half);
                var box = new Bounds();
                box.SetMinMax(min, max);
                _boxes.Add(box);
            }
        }

        /// <summary>L'arbre ou le rocher debout dont la boîte est touchée la première par ce rayon ; faux s'il n'y en a pas.</summary>
        public bool TryPick(Ray ray, out NatureItem picked)
        {
            picked = null;
            float nearest = float.MaxValue;
            for (int i = 0; i < _boxes.Count; i++)
            {
                NatureItem item = _nature.All[i];
                if (item.IsRemoved || !_boxes[i].IntersectRay(ray, out float distance) || distance >= nearest)
                    continue;
                nearest = distance;
                picked = item;
            }
            return picked != null;
        }

        /// <summary>La boîte des maillages du modèle, dans son repère ; une boîte d'un mètre s'il n'en a pas.</summary>
        private static Bounds ModelBox(GameObject model)
        {
            var box = new Bounds(new Vector3(0f, 0.5f, 0f), Vector3.one);
            if (model == null)
                return box;
            bool first = true;
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null)
                    continue;
                Bounds local = filter.sharedMesh.bounds;
                // Le repère du morceau, ramené à celui du modèle : ses huit coins suffisent à sa boîte.
                Matrix4x4 toModel = model.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    var offset = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f,
                        (corner & 4) == 0 ? -1f : 1f);
                    Vector3 point = toModel.MultiplyPoint3x4(local.center + Vector3.Scale(local.extents, offset));
                    if (first)
                        box = new Bounds(point, Vector3.zero);
                    else
                        box.Encapsulate(point);
                    first = false;
                }
            }
            return box;
        }
    }
}
