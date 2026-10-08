using System.Collections.Generic;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les arbres et les rochers marqués, cachés en prévision (décision 228) : comme un arbre retiré, leur instance sur
    /// le Terrain prend la taille 0, et la sienne est gardée pour la lui rendre quand la prévision s'éteint ou qu'on le
    /// démarque. Un arbre retiré pour de bon pendant ce temps reste caché.
    /// </summary>
    internal sealed class ForeseenNature
    {
        private readonly Dictionary<int, TreeInstance> _hidden = new Dictionary<int, TreeInstance>();
        private readonly HashSet<int> _keep = new HashSet<int>();
        private readonly List<int> _back = new List<int>();

        /// <summary>
        /// Cache ces arbres et rochers sur ce Terrain, chacun à l'instance que lui donne la lecture de la carte, et
        /// rend leur taille à ceux qu'il cachait et qui n'en sont plus.
        /// </summary>
        public void Hide(TerrainData data, IReadOnlyList<int> instances, IReadOnlyList<NatureItem> items)
        {
            _keep.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                NatureItem item = items[i];
                if (item.IsRemoved)
                    continue;
                _keep.Add(item.Id);
                if (_hidden.ContainsKey(item.Id))
                    continue;
                int index = instances[item.Id - 1];
                TreeInstance tree = data.GetTreeInstance(index);
                _hidden.Add(item.Id, tree);
                tree.widthScale = 0f;
                tree.heightScale = 0f;
                data.SetTreeInstance(index, tree);
            }
            _back.Clear();
            foreach (KeyValuePair<int, TreeInstance> pair in _hidden)
            {
                if (!_keep.Contains(pair.Key))
                    _back.Add(pair.Key);
            }
            foreach (int id in _back)
            {
                data.SetTreeInstance(instances[id - 1], _hidden[id]);
                _hidden.Remove(id);
            }
        }

        /// <summary>
        /// Cet arbre ou ce rocher est retiré pour de bon : il reste caché, on ne lui rendra pas sa taille.
        /// </summary>
        public void Forget(NatureItem item)
        {
            _hidden.Remove(item.Id);
        }
    }
}
