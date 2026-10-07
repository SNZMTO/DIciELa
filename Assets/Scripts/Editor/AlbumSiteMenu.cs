using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La mise en scène du chantier qui monte pour l'album (Docs/ALBUM.md), en Play, une fois des cabanes posées : le
    /// premier chantier de la file, celui qu'on bâtit d'abord, est tiré au centre de l'écran comme par un glisser, puis
    /// la caméra s'en rapproche (un zoom vers le point visé : un cran de molette viserait le sol d'avant le glisser,
    /// la caméra ne se plaçant qu'à l'image suivante) ; l'outil de pose est mis de côté.
    /// </summary>
    public static class AlbumSiteMenu
    {
        // La distance de la caméra au chantier, en mètres : assez près pour voir la ligne, assez loin pour la cabane
        // entière, son toit compris.
        private const float FramingMeters = 16f;

        [MenuItem("DICIELA/Album/Regarder le premier chantier")]
        public static void LookAtTheFirstSite()
        {
            World world = AlbumWorld.Running();
            ConstructionSite site = world != null ? FirstBuildingSite(world) : null;
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (site == null || rig == null)
            {
                Debug.LogError("DICIELA : on regarde un chantier en Play, une fois une construction posée en fantôme.");
                return;
            }

            AlbumMenu.PutPlacementToolAside();
            Vector2 center = site.Structure.Placement.Center;
            var ground = world.Map.Relief.GroundAt(center);
            var middle = new Vector2(rig.Camera.pixelWidth / 2f, rig.Camera.pixelHeight / 2f);
            if (rig.BeginDrag(rig.Camera.WorldToScreenPoint(ground)))
                rig.DragTo(middle);
            rig.ZoomTo(FramingMeters);
            Debug.Log("DICIELA : la vue se rapproche du chantier de la construction n° " + site.Structure.Id + ".");
        }

        // Le premier chantier de la file qui bâtit ou déplace une construction ; un sol n'a pas de cabane à cadrer.
        private static ConstructionSite FirstBuildingSite(World world)
        {
            foreach (ConstructionSite site in world.ConstructionSites.Queue)
            {
                if (site.Structure != null)
                    return site;
            }
            return null;
        }
    }
}
