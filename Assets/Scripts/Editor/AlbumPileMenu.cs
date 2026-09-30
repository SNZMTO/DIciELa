using Diciela.Controls;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mises en scène du tas de départ pour l'album (Docs/ALBUM.md), en Play, une fois la tente plantée. Toucher le
    /// tas : comme un clic sur lui, qui ouvre sa fiche. Y mettre la Chimère : l'ordre même qu'envoie le bouton de la
    /// fiche.
    /// </summary>
    public static class AlbumPileMenu
    {
        // Où l'on vise le tas pour le toucher : à cette hauteur au-dessus de son centre, en mètres.
        private const float TouchHeight = 1f;

        [MenuItem("DICIELA/Album/Toucher le tas")]
        public static void TouchThePile()
        {
            World world = AlbumWorld.Running();
            Structure pile = world != null ? PileOf(world) : null;
            var choose = Object.FindAnyObjectByType<ChooseControls>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (pile == null || choose == null || choose.Tool == null || rig == null)
            {
                Debug.LogError("DICIELA : on touche le tas en Play, une fois la tente plantée.");
                return;
            }

            Vector2 center = pile.Placement.Center;
            var top = new Vector3(center.x, world.Map.Relief.HeightAt(center.x, center.y) + TouchHeight, center.y);
            choose.Tool.TouchAt(rig.Camera.WorldToScreenPoint(top));
            Debug.Log("DICIELA : mise en scène de l'album : le tas est touché.");
        }

        [MenuItem("DICIELA/Album/Mettre la Chimère au tas")]
        public static void AssignTheChimera()
        {
            World world = AlbumWorld.Running();
            Structure pile = world != null ? PileOf(world) : null;
            Villager chimera = world != null ? world.Villagers.Chimera : null;
            if (pile == null || chimera == null)
            {
                Debug.LogError("DICIELA : on met la Chimère au tas en Play, une fois la tente plantée.");
                return;
            }
            world.Submit(new AssignVillagerCommand(chimera.Id, pile.Id));
            Debug.Log("DICIELA : mise en scène de l'album : la Chimère est mise au tas.");
        }

        private static Structure PileOf(World world)
        {
            foreach (Structure structure in world.Structures.All)
            {
                if (world.Warehouses.StockOf(structure) != null)
                    return structure;
            }
            return null;
        }
    }
}
