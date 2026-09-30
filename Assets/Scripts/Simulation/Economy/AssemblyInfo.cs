using System.Runtime.CompilerServices;

// Ce qui change l'état de l'économie est internal, ouvert aux seules couches de la simulation au-dessus d'elle
// (ARCHITECTURE, section 5) : la société y prend et y fait ses tâches, le monde y ouvre les stocks et les chantiers. Au-
// dessus de la simulation, on lit, on écoute et on envoie des ordres, rien d'autre.
[assembly: InternalsVisibleTo("Diciela.Simulation.Society")]
[assembly: InternalsVisibleTo("Diciela.Simulation")]
