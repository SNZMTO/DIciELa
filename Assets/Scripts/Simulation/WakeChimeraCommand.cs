namespace Diciela.Simulation
{
    /// <summary>
    /// L'ordre « réveiller » (GDD, section 6) : le joueur touche de nouveau le logis de la Chimère, et elle se lève, ou
    /// renonce à se coucher si elle y allait ; elle se remet à flâner là où elle est. Refusé, il ne change rien et les
    /// habitants disent pourquoi : pas de Chimère, ou elle est debout.
    /// </summary>
    public sealed class WakeChimeraCommand : Command
    {
        internal override void Execute(World world)
        {
            world.Villagers.WakeChimera();
        }
    }
}
