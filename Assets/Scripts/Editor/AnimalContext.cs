using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Ce que le monteur des animaux partage entre eux (AnimalBuilder) : les couleurs, les emplacements par nom, les poses
    /// du pack et ses pièces reconnues à leur maillage.
    /// </summary>
    internal sealed class AnimalContext
    {
        public AnimalContext(List<PaletteDefinition> palettes, Dictionary<string, AttachmentSlotDefinition> slots,
            PackPoses poses, Dictionary<Mesh, AttachmentDefinition> byMesh)
        {
            Palettes = palettes;
            Slots = slots;
            Poses = poses;
            ByMesh = byMesh;
        }

        public List<PaletteDefinition> Palettes { get; }
        public Dictionary<string, AttachmentSlotDefinition> Slots { get; }
        public PackPoses Poses { get; }

        /// <summary>Les pièces du pack, par le maillage de leur modèle : le pack en pose des copies sur ses personnages.</summary>
        public Dictionary<Mesh, AttachmentDefinition> ByMesh { get; }
    }
}
