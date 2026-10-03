namespace Diciela.Data
{
    /// <summary>
    /// Ce que fait une sorte de tâche : le geste que le code sait jouer (ARCHITECTURE, section 7). Les sortes de tâches
    /// sont des fiches ; chacune dit lequel de ces gestes elle est. Les valeurs ne changent jamais de numéro.
    /// </summary>
    public enum TaskWork
    {
        /// <summary>Porter : prendre des marchandises dans un stock et les livrer à un chantier.</summary>
        Carry = 0,

        /// <summary>Bâtir : travailler sur un chantier dont les matériaux sont livrés.</summary>
        Build = 1,

        /// <summary>Défricher : retirer un arbre ou un rocher marqué, et porter ce qu'il rend à un entrepôt.</summary>
        Clear = 2,

        /// <summary>Conduire : mener la charrette d'un entrepôt à sa place sur la Place, où elle sert d'étal.</summary>
        Drive = 3
    }
}
