namespace Diciela.Simulation
{
    /// <summary>
    /// Un ordre du joueur (le patron Commande) : chaque geste qui change la ville devient un petit objet envoyé au
    /// monde par <see cref="World.Submit"/>, qui l'applique au début du pas suivant (ARCHITECTURE, section 5). Les
    /// tests et la ville témoin envoient les mêmes. Seul le monde l'exécute : <c>Execute</c> est <c>internal</c>,
    /// invisible hors de la simulation, si bien qu'aucun ordre ne peut sauter la file.
    /// </summary>
    public abstract class Command
    {
        /// <summary>Applique l'ordre ; il se vérifie lui-même par la même fonction que l'aperçu.</summary>
        internal abstract void Execute(World world);
    }
}
