using System.Runtime.CompilerServices;

// Ce qui change l'état de la société est internal, ouvert au seul monde (ARCHITECTURE, section 5) : au-dessus de la
// simulation, on lit, on écoute et on envoie des ordres, rien d'autre.
[assembly: InternalsVisibleTo("Diciela.Simulation")]
