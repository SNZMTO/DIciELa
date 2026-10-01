using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « dormir » (GDD, section 6) : le joueur clique « Aller dormir » sur la fiche du logis de la Chimère, et
    /// elle va s'y coucher. Elle marche jusqu'au milieu du sol gardé libre devant sa porte, puis entre. Refusé, il ne
    /// change rien et les habitants disent pourquoi : pas de Chimère, pas de logis, déjà couchée ou en chemin, ou sa
    /// porte hors d'atteinte.
    /// </summary>
    public sealed class SendChimeraToBedCommand : Command
    {
        internal override void Execute(World world)
        {
            Structure home = world.Structures.ChimeraHome;
            Vector2? bed = null;
            if (home != null)
                bed = world.PlacementRules.Doorway(home.Definition, home.Placement).Center;
            world.Villagers.SendChimeraToBed(bed);
        }
    }
}
