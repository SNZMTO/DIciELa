namespace Diciela.Simulation
{
    /// <summary>
    /// Un ordre du joueur (le patron Commande) : chaque geste qui change la ville devient un petit objet envoyé au monde,
    /// qui l'applique au début du pas suivant (ARCHITECTURE, section 5). Les tests et la ville témoin envoient les mêmes.
    /// </summary>
    public interface IOrder
    {
        /// <summary>Applique l'ordre ; il se vérifie lui-même par la même fonction que l'aperçu.</summary>
        void Execute(World world);
    }
}
