using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// La table des postes (ARCHITECTURE, section 4) : les postes ouverts par les lieux de travail bâtis, et ceux ouverts
    /// pour un premier habitant (GDD, section 7), et qui tient chacun. La seule à changer qui tient quel poste. Le joueur
    /// y met un habitant ou l'en retire par ses ordres ; la Chimère ne prend jamais de poste d'elle-même (GDD, section 6).
    /// Les événements d'un pas attendent la fin de leur couche (ARCHITECTURE, section 5).
    /// </summary>
    public sealed class WorkSlots
    {
        private readonly List<WorkSlot> _all = new List<WorkSlot>();
        private readonly List<SlotHappening> _happened = new List<SlotHappening>();

        /// <summary>Levé quand un habitant prend un poste.</summary>
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

        /// <summary>Vrai si ce lieu a un poste que personne ne tient.</summary>
        public bool HasFreeSlotAt(Structure workplace)
        {
            return FreeSlotAt(workplace) != null;
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
        /// Une construction est mise en service : bâtie, elle ouvre les postes que sa fiche liste. Seule la mise en service
        /// du monde l'appelle (Commissioning).
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
        /// poste libre, ou s'il y est déjà. L'ordre « affecter » l'appelle, et le déroulé du départ quand le lieu de
        /// travail d'un premier habitant est bâti.
        /// </summary>
        internal void Assign(Villager villager, Structure workplace)
        {
            WorkSlot held = villager == null ? null : SlotOf(villager);
            WorkSlot free = workplace == null ? null : FreeSlotAt(workplace);
            // Les raisons d'un refus, dans l'ordre où elles se vérifient.
            AssignmentRefusal? refusal = villager == null ? AssignmentRefusal.NoVillager
                : workplace == null || !IsWorkplace(workplace) ? AssignmentRefusal.NotAWorkplace
                : held != null && ReferenceEquals(held.Workplace, workplace) ? AssignmentRefusal.AlreadyThere
                : free == null ? AssignmentRefusal.NoFreeSlot
                : (AssignmentRefusal?)null;
            if (refusal.HasValue)
            {
                Refuse(refusal.Value);
                return;
            }
            if (held != null)
                Release(villager, held);
            Hold(villager, free);
        }

        /// <summary>
        /// Ouvre à ce lieu un poste de cette sorte pour cet habitant, qui le prend aussitôt (GDD, section 7 : le poste « à
        /// tout faire » d'un premier habitant, tant que son lieu de travail n'est pas bâti). Il n'en tenait aucun. Seul le
        /// monde l'appelle (le déroulé du départ).
        /// </summary>
        internal void OpenFor(Villager villager, Structure workplace, WorkSlotDefinition definition)
        {
            if (villager == null)
                throw new ArgumentNullException(nameof(villager));
            if (SlotOf(villager) != null)
                throw new InvalidOperationException("L'habitant n° " + villager.Id + " tient déjà un poste.");
            var slot = new WorkSlot(workplace, definition);
            _all.Add(slot);
            Hold(villager, slot);
        }

        /// <summary>
        /// Ferme ce poste, que plus personne ne tient (GDD, section 7 : le poste « à tout faire » d'un premier habitant,
        /// quand son lieu de travail est bâti). Seul le monde l'appelle (le déroulé du départ).
        /// </summary>
        internal void Close(WorkSlot slot)
        {
            if (slot.Holder != null)
                throw new InvalidOperationException("Un poste tenu ne se ferme pas : son habitant le quitte d'abord.");
            _all.Remove(slot);
        }

        /// <summary>
        /// Retire cet habitant de son poste : il redevient libre. Refusé sans habitant, ou s'il ne tient aucun poste.
        /// L'ordre « retirer » l'appelle, et le déroulé du départ quand le lieu d'un premier habitant n'a pas de poste
        /// libre.
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
                    SlotHappening happening = _happened[i];
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

        private void Hold(Villager villager, WorkSlot slot)
        {
            slot.SetHolder(villager);
            villager.TakeSlot(slot);
            _happened.Add(new SlotHappening(villager, slot, true, default));
        }

        private void Release(Villager villager, WorkSlot slot)
        {
            slot.SetHolder(null);
            _happened.Add(new SlotHappening(villager, slot, false, default));
        }

        private void Refuse(AssignmentRefusal reason)
        {
            _happened.Add(new SlotHappening(null, null, false, reason));
        }
    }
}
