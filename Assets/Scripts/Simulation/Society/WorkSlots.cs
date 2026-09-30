using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// La table des postes (ARCHITECTURE, section 4) : les postes ouverts par les lieux de travail bâtis, et qui tient
    /// chacun. La seule à changer qui tient quel poste. Le joueur y met un habitant ou l'en retire par ses ordres ; la
    /// Chimère ne prend jamais de poste d'elle-même (GDD, section 6). Les événements d'un pas attendent la fin de leur
    /// couche (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class WorkSlots
    {
        private readonly List<WorkSlot> _all = new List<WorkSlot>();
        private readonly List<Happening> _happened = new List<Happening>();

        /// <summary>Levé quand un habitant prend un poste, par l'ordre « affecter ».</summary>
        public event Action<Villager, WorkSlot> VillagerAssigned;

        /// <summary>Levé quand un habitant quitte son poste : retiré, ou affecté ailleurs.</summary>
        public event Action<Villager, WorkSlot> VillagerUnassigned;

        /// <summary>Levé quand un ordre « affecter » ou « retirer » est refusé, avec la raison.</summary>
        public event Action<AssignmentRefusal> AssignmentRefused;

        /// <summary>Les postes ouverts, dans l'ordre où ils se sont ouverts.</summary>
        public IReadOnlyList<WorkSlot> All => _all;

        /// <summary>Le poste que tient cet habitant ; null s'il n'en tient aucun.</summary>
        public WorkSlot SlotOf(Villager villager)
        {
            foreach (WorkSlot slot in _all)
            {
                if (ReferenceEquals(slot.Holder, villager))
                    return slot;
            }
            return null;
        }

        /// <summary>Vrai si ce lieu ouvre au moins un poste.</summary>
        public bool IsWorkplace(Structure structure)
        {
            foreach (WorkSlot slot in _all)
            {
                if (ReferenceEquals(slot.Workplace, structure))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Une construction vient d'être posée ou bâtie : bâtie, elle ouvre les postes que sa fiche liste. Seuls le monde
        /// et les ouvriers qui finissent un chantier l'appellent.
        /// </summary>
        internal void Track(Structure structure)
        {
            if (structure.State != StructureState.Built || IsWorkplace(structure))
                return;
            foreach (WorkSlotDefinition definition in structure.Definition.WorkSlots)
                _all.Add(new WorkSlot(structure, definition));
        }

        /// <summary>
        /// Met cet habitant au premier poste libre de ce lieu ; s'il en tenait un ailleurs, il le quitte d'abord.
        /// Refusé (rien ne change, et <see cref="AssignmentRefused"/> le dira) sans habitant, sans poste à ce lieu, sans
        /// poste libre, ou s'il y est déjà. Seul l'ordre « affecter » l'appelle.
        /// </summary>
        internal void Assign(Villager villager, Structure workplace)
        {
            if (villager == null)
            {
                Refuse(AssignmentRefusal.NoVillager);
                return;
            }
            if (workplace == null || !IsWorkplace(workplace))
            {
                Refuse(AssignmentRefusal.NotAWorkplace);
                return;
            }
            WorkSlot held = SlotOf(villager);
            if (held != null && ReferenceEquals(held.Workplace, workplace))
            {
                Refuse(AssignmentRefusal.AlreadyThere);
                return;
            }
            WorkSlot free = FreeSlotAt(workplace);
            if (free == null)
            {
                Refuse(AssignmentRefusal.NoFreeSlot);
                return;
            }

            if (held != null)
                Release(villager, held);
            free.SetHolder(villager);
            villager.TakeSlot(free);
            _happened.Add(new Happening(villager, free, true, default));
        }

        /// <summary>
        /// Retire cet habitant de son poste : il redevient libre. Refusé sans habitant, ou s'il ne tient aucun poste.
        /// Seul l'ordre « retirer » l'appelle.
        /// </summary>
        internal void Unassign(Villager villager)
        {
            if (villager == null)
            {
                Refuse(AssignmentRefusal.NoVillager);
                return;
            }
            WorkSlot held = SlotOf(villager);
            if (held == null)
            {
                Refuse(AssignmentRefusal.NotAssigned);
                return;
            }
            Release(villager, held);
            villager.LeaveSlot();
        }

        /// <summary>Livre les événements du pas, dans leur ordre, puis vide la file. Seul le monde l'appelle.</summary>
        internal void DeliverEvents()
        {
            try
            {
                for (int i = 0; i < _happened.Count; i++)
                {
                    Happening happening = _happened[i];
                    if (happening.Villager == null)
                        AssignmentRefused?.Invoke(happening.Refusal);
                    else if (happening.Assigned)
                        VillagerAssigned?.Invoke(happening.Villager, happening.Slot);
                    else
                        VillagerUnassigned?.Invoke(happening.Villager, happening.Slot);
                }
            }
            finally
            {
                _happened.Clear();
            }
        }

        private WorkSlot FreeSlotAt(Structure workplace)
        {
            foreach (WorkSlot slot in _all)
            {
                if (ReferenceEquals(slot.Workplace, workplace) && slot.Holder == null)
                    return slot;
            }
            return null;
        }

        private void Release(Villager villager, WorkSlot slot)
        {
            slot.SetHolder(null);
            _happened.Add(new Happening(villager, slot, false, default));
        }

        private void Refuse(AssignmentRefusal reason)
        {
            _happened.Add(new Happening(null, null, false, reason));
        }

        private readonly struct Happening
        {
            public Happening(Villager villager, WorkSlot slot, bool assigned, AssignmentRefusal refusal)
            {
                Villager = villager;
                Slot = slot;
                Assigned = assigned;
                Refusal = refusal;
            }

            public Villager Villager { get; }
            public WorkSlot Slot { get; }
            public bool Assigned { get; }
            public AssignmentRefusal Refusal { get; }
        }
    }
}
