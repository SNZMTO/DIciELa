using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Un poste ouvert (GDD, section 7) : son lieu de travail, sa fiche (les tâches qu'on y fait) et celui qui le tient.
    /// La table des postes est la seule à changer qui le tient.
    /// </summary>
    public sealed class WorkSlot
    {
        internal WorkSlot(Structure workplace, WorkSlotDefinition definition)
        {
            Workplace = workplace;
            Definition = definition;
        }

        public Structure Workplace { get; }
        public WorkSlotDefinition Definition { get; }

        /// <summary>Celui qui le tient ; null s'il est libre.</summary>
        public Villager Holder { get; private set; }

        internal void SetHolder(Villager holder)
        {
            Holder = holder;
        }
    }
}
