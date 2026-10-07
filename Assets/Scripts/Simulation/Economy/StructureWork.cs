using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Bâtir une construction posée en fantôme (GDD, section 9) : les matériaux et les heures de sa fiche ; finie, elle
    /// devient bâtie, et le monde la met en service à la fin du pas (décision 179).
    /// </summary>
    public sealed class StructureWork : SiteWork
    {
        private readonly Structure _structure;
        private readonly Structures _structures;

        internal StructureWork(Structure structure, Structures structures)
        {
            _structure = structure ?? throw new ArgumentNullException(nameof(structure));
            _structures = structures ?? throw new ArgumentNullException(nameof(structures));
        }

        public override Structure Structure => _structure;
        public override double WorkNeeded => _structure.Definition.WorkHours;
        internal override IReadOnlyList<GoodAmount> Cost => _structure.Definition.Cost;
        internal override bool Commissions => true;

        internal override void Finish()
        {
            _structures.Finish(_structure);
        }
    }
}
