namespace Diciela.Simulation.Society
{
    /// <summary>Pourquoi un ordre « affecter » ou « retirer » est refusé. Les valeurs ne changent jamais de numéro.</summary>
    public enum AssignmentRefusal
    {
        /// <summary>Cet habitant n'existe pas.</summary>
        NoVillager = 0,

        /// <summary>Ce lieu n'existe pas, ou n'ouvre aucun poste (pas encore bâti, ou pas un lieu de travail).</summary>
        NotAWorkplace = 1,

        /// <summary>Tous les postes de ce lieu sont tenus.</summary>
        NoFreeSlot = 2,

        /// <summary>L'habitant tient déjà un poste de ce lieu.</summary>
        AlreadyThere = 3,

        /// <summary>L'habitant ne tient aucun poste : il n'y a rien à retirer.</summary>
        NotAssigned = 4
    }
}
