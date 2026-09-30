namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Une place d'ouvrier prise sur un chantier (ARCHITECTURE, section 7) : la prendre l'a réservée, pour que le
    /// chantier n'ait jamais plus d'ouvriers que permis ; elle se rend quand l'ouvrier s'en va.
    /// </summary>
    public sealed class BuildShift
    {
        internal BuildShift(ConstructionSite site)
        {
            Site = site;
        }

        public ConstructionSite Site { get; }

        /// <summary>Vrai une fois l'ouvrier parti : la place est rendue.</summary>
        public bool IsOver { get; private set; }

        internal void MarkOver()
        {
            IsOver = true;
        }
    }
}
