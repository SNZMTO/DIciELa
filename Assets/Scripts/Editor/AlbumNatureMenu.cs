using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La mise en scène du défrichage pour l'album et les essais (Docs/ALBUM.md), en Play : marquer les arbres et les
    /// rochers les plus proches de la Chimère, du plus proche au plus loin, par l'ordre même qu'envoie l'outil
    /// « Retirer » ; si la carte n'en a aucun de peint près d'elle, il le dit. L'outil se prend par sa touche (X), que le
    /// CLI sait presser.
    /// </summary>
    public static class AlbumNatureMenu
    {
        private const int MarkedCount = 6;
        private const float SearchMeters = 30f;

        [MenuItem("DICIELA/Album/Marquer les arbres près de la Chimère")]
        public static void MarkNearTheChimera()
        {
            World world = AlbumWorld.Running();
            Villager chimera = world != null ? world.Villagers.Chimera : null;
            if (chimera == null)
            {
                Debug.LogError("DICIELA : on marque des arbres en Play, une fois la Chimère créée.");
                return;
            }

            var near = new List<NatureItem>();
            foreach (NatureItem item in world.Nature.All)
            {
                if (!item.IsRemoved && !item.IsMarked &&
                    Vector2.Distance(item.Placement.Center, chimera.Position) <= SearchMeters)
                    near.Add(item);
            }
            near.Sort((a, b) => Vector2.Distance(a.Placement.Center, chimera.Position)
                .CompareTo(Vector2.Distance(b.Placement.Center, chimera.Position)));
            if (near.Count == 0)
            {
                Debug.LogWarning("DICIELA : aucun arbre ni rocher debout à moins de " + SearchMeters + " m de la " +
                                 "Chimère : la carte n'en a pas de peint près d'elle.");
                return;
            }

            var ids = new List<int>();
            for (int i = 0; i < near.Count && i < MarkedCount; i++)
                ids.Add(near[i].Id);
            world.Submit(new MarkForClearingCommand(ids));
            Debug.Log("DICIELA : mise en scène de l'album : " + ids.Count + " arbres ou rochers marqués.");
        }
    }
}
