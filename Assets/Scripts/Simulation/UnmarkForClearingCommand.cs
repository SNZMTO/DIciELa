using System;
using System.Collections.Generic;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « ne plus retirer » : des arbres et des rochers marqués ne le sont plus ; celui qu'on abattait est laissé,
    /// avec le travail déjà fait. Un numéro inconnu, ou un arbre qui n'était pas marqué, ne changent rien.
    /// </summary>
    public sealed class UnmarkForClearingCommand : Command
    {
        private readonly int[] _natureIds;

        public UnmarkForClearingCommand(IReadOnlyList<int> natureIds)
        {
            if (natureIds == null)
                throw new ArgumentNullException(nameof(natureIds));

            _natureIds = new int[natureIds.Count];
            for (int i = 0; i < _natureIds.Length; i++)
                _natureIds[i] = natureIds[i];
        }

        /// <summary>Les numéros des arbres et des rochers, dans l'ordre où le joueur les a touchés.</summary>
        public IReadOnlyList<int> NatureIds => _natureIds;

        internal override void Execute(World world)
        {
            foreach (int id in _natureIds)
            {
                NatureItem item = world.Nature.Find(id);
                if (item != null)
                    world.Nature.Unmark(item);
            }
        }
    }
}
