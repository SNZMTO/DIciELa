using System;
using System.Collections.Generic;
using Diciela.Simulation.Land;

namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « marquer à retirer » (GDD, sections 2 et 9) : des arbres et des rochers, dans l'ordre où le joueur les a
    /// touchés ; on les retirera dans cet ordre. Un numéro inconnu, un arbre déjà marqué ou déjà retiré ne changent rien.
    /// </summary>
    public sealed class MarkForClearingCommand : Command
    {
        private readonly int[] _natureIds;

        public MarkForClearingCommand(IReadOnlyList<int> natureIds)
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
                    world.Nature.Mark(item);
            }
        }
    }
}
