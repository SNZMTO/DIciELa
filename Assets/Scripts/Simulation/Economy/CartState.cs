namespace Diciela.Simulation.Economy
{
    /// <summary>Où en est une charrette de l'étal (GDD, section 11).</summary>
    public enum CartState
    {
        /// <summary>Rangée à son entrepôt : on ne la voit pas.</summary>
        AtHome,

        /// <summary>Un charretier la tire vers la place de charrette qui l'a appelée.</summary>
        Pulled,

        /// <summary>Garée à sa place, sur la Place : elle sert d'étal.</summary>
        Parked
    }
}
