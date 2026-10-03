using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La mise en scène de l'étal pour l'album (Docs/ALBUM.md ; décision 202), en Play, une fois la Place bâtie, par les
    /// ordres du joueur, en attendant la fiche de la Place : la consigne de sa première place de charrette demande 10 Bois,
    /// et toute la Pierre au-delà de 15 au stock. Les charretiers, ou la Chimère, garnissent alors la charrette garée.
    /// </summary>
    public static class AlbumStallMenu
    {
        // La consigne de la mise en scène : une quantité, et un « garder au moins N ».
        private const int Wood = 10;
        private const int StoneKept = 15;

        [MenuItem("DICIELA/Album/Garnir la première charrette de la Place")]
        public static void StockTheFirstCart()
        {
            World world = AlbumWorld.Running();
            Catalog catalog = EditorAssets.Load<Catalog>(ContentBuilder.CatalogPath);
            if (world == null || world.Carts.Spots.Count == 0 || catalog == null || catalog.Goods.Count < 2)
            {
                Debug.LogError("DICIELA : on garnit la charrette en Play, une fois la Place bâtie.");
                return;
            }
            CartSpot spot = world.Carts.Spots[0];
            world.Submit(new SetConsigneCommand(spot.Place.Id, spot.Index,
                new ConsigneLine(catalog.Goods[0], ConsigneMode.Amount, Wood)));
            world.Submit(new SetConsigneCommand(spot.Place.Id, spot.Index,
                new ConsigneLine(catalog.Goods[1], ConsigneMode.KeepAtLeast, StoneKept)));
            Debug.Log("DICIELA : mise en scène de l'album : la première charrette demande " + Wood + " " +
                      catalog.Goods[0].Id + ", et ce qui dépasse " + StoneKept + " " + catalog.Goods[1].Id + ".");
        }
    }
}
