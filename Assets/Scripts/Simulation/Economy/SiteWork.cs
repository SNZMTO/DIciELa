using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Ce que bâtit un chantier (GDD, section 9) : une construction posée en fantôme (StructureWork), un bâtiment qu'on
    /// déplace (MoveWork), un sol qu'on trace ou qu'on efface (FloorWork). Chacun dit ce qu'on vient servir, son travail
    /// et ses matériaux, et ce qui arrive quand on y travaille et quand le dernier travail est fait ; le chantier garde
    /// les livraisons, l'avancement et les ouvriers.
    /// </summary>
    public abstract class SiteWork
    {
        /// <summary>La construction qu'on bâtit ou qu'on déplace ; null pour un sol.</summary>
        public virtual Structure Structure => null;

        /// <summary>Le sol qu'on trace ou qu'on efface ; null pour une construction.</summary>
        public virtual FloorPatch Floor => null;

        /// <summary>Le travail demandé, en heures d'un débutant.</summary>
        public abstract double WorkNeeded { get; }

        /// <summary>Les matériaux demandés : ceux de la fiche d'une construction posée ; aucun pour les autres.</summary>
        internal virtual IReadOnlyList<GoodAmount> Cost => Array.Empty<GoodAmount>();

        /// <summary>Vrai si ce qu'il bâtit, fini, se met en service : une construction posée.</summary>
        internal virtual bool Commissions => false;

        /// <summary>Le chantier en est à tant d'heures de travail fait : ce qui en découle avant la fin.</summary>
        internal virtual void Progress(double workDone)
        {
        }

        /// <summary>Le dernier travail est fait : ce qu'il bâtit est fini.</summary>
        internal abstract void Finish();
    }
}
