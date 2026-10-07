using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.View;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Les outils de la ville (ARCHITECTURE, section 11) : poser, retirer, déplacer, choisir, tracer la Place et ses sols,
    /// tracer les chemins, et la main,
    /// qui n'en tient qu'un à la fois ; nés à l'ouverture de la ville, libérés à sa sortie. Les gestes et l'interface les
    /// reçoivent d'ici.
    /// </summary>
    internal sealed class TownTools : IDisposable
    {
        public TownTools(World world, Catalog catalog, Camera camera, PlacementPreview preview,
            PaintToolPreview paintPreview, PlacementSettings settings, NatureViews nature, StructureViews structures)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));

            Placement = new PlacementTool(world, camera, preview, settings);
            Clearing = new ClearingTool(world, camera, nature);
            Choose = new ChooseTool(camera, structures);
            Move = new MoveTool(world, camera, structures, preview, settings);
            Plaza = new PaintTool(world, camera, paintPreview, settings, PaintEntry.Plaza, catalog.Floors);
            Paths = new PaintTool(world, camera, paintPreview, settings, PaintEntry.Paths, catalog.Floors);
            Hand = new Hand(Placement, Clearing, Move, Plaza, Paths);
        }

        public PlacementTool Placement { get; }
        public ClearingTool Clearing { get; }
        public ChooseTool Choose { get; }
        public MoveTool Move { get; }
        public PaintTool Plaza { get; }

        /// <summary>L'outil Chemins : le même que celui de la Place, à son autre entrée (décision 210).</summary>
        public PaintTool Paths { get; }
        public Hand Hand { get; }

        /// <summary>Lâche ce qui est en main et cesse de suivre le monde.</summary>
        public void Dispose()
        {
            Hand.Dispose();
            Placement.Cancel();
            Placement.Dispose();
            Move.PutDown();
            Move.Dispose();
            Plaza.PutDown();
            Plaza.Dispose();
            Paths.PutDown();
            Paths.Dispose();
        }
    }
}
