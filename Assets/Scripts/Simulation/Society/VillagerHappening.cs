using Diciela.Data;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Ce qui est arrivé aux habitants pendant un pas, en attendant d'être annoncé à la fin de la couche (ARCHITECTURE,
    /// section 5) : une arrivée, un ordre refusé, un habitant qui va dormir, s'endort ou se réveille.
    /// </summary>
    internal readonly struct VillagerHappening
    {
        public enum Kinds
        {
            Arrived,
            ChimeraRefused,
            WentToBed,
            FellAsleep,
            WokeUp,
            SleepRefused
        }

        private VillagerHappening(Kinds kind, Villager villager, VillagerKindDefinition refusedKind,
            SleepRefusal sleepRefusal)
        {
            Kind = kind;
            Villager = villager;
            RefusedKind = refusedKind;
            SleepRefusal = sleepRefusal;
        }

        public Kinds Kind { get; }

        /// <summary>L'habitant concerné ; null pour un ordre refusé.</summary>
        public Villager Villager { get; }

        /// <summary>La sorte d'une Chimère refusée.</summary>
        public VillagerKindDefinition RefusedKind { get; }

        /// <summary>Pourquoi un ordre « dormir » ou « réveiller » est refusé.</summary>
        public SleepRefusal SleepRefusal { get; }

        public static VillagerHappening Of(Kinds kind, Villager villager)
        {
            return new VillagerHappening(kind, villager, null, default);
        }

        public static VillagerHappening ChimeraRefusedWith(VillagerKindDefinition kind)
        {
            return new VillagerHappening(Kinds.ChimeraRefused, null, kind, default);
        }

        public static VillagerHappening SleepRefusedFor(SleepRefusal reason)
        {
            return new VillagerHappening(Kinds.SleepRefused, null, null, reason);
        }
    }
}
