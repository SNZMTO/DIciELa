using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.View;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Les outils de la ville (ARCHITECTURE, section 11) : poser, retirer, choisir, et la main, qui n'en tient qu'un à la
    /// fois ; nés à l'ouverture de la ville, libérés à sa sortie. Les gestes et l'interface les reçoivent d'ici.
    /// </summary>
    internal sealed class TownTools : IDisposable
    {
        public TownTools(World world, Camera camera, PlacementPreview preview, PlacementSettings settings,
            NatureViews nature, StructureViews structures)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));

            Placement = new PlacementTool(world, camera, preview, settings);
            Clearing = new ClearingTool(world, camera, nature);
            Choose = new ChooseTool(camera, structures);
            Hand = new Hand(Placement, Clearing);
        }

        public PlacementTool Placement { get; }
        public ClearingTool Clearing { get; }
        public ChooseTool Choose { get; }
        public Hand Hand { get; }

        /// <summary>Lâche ce qui est en main et cesse de suivre le monde.</summary>
        public void Dispose()
        {
            Hand.Dispose();
            Placement.Cancel();
            Placement.Dispose();
        }
    }
}
