using System.Runtime.CompilerServices;

// Ce qui change l'état du terrain est internal, ouvert aux seules couches de la simulation au-dessus de lui
// (ARCHITECTURE, section 5) : l'économie y finit un chantier, le monde y pose. Au-dessus de la simulation, on lit, on
// écoute et on envoie des ordres, rien d'autre.
[assembly: InternalsVisibleTo("Diciela.Simulation.Economy")]
[assembly: InternalsVisibleTo("Diciela.Simulation")]
