namespace Diciela.Data
{
    /// <summary>
    /// Comment une pièce d'un emplacement vient au contact du personnage (SAVOIR, 2.11), le long d'un axe, jusqu'à toucher
    /// la surface de la tête ou du corps choisis : les têtes et les corps n'ont pas le même relief. Liste fermée : chaque
    /// sens a sa carte de surface.
    /// </summary>
    public enum ContactAxis
    {
        /// <summary>Pas de contact : la pièce reste au point de son repère, plus son décalage.</summary>
        None,

        /// <summary>Elle descend sur le dessus du crâne : chapeaux, cheveux.</summary>
        Down,

        /// <summary>Elle recule contre l'avant de la face ou du corps : lunettes, museau, barbe ; écharpe, bourse.</summary>
        Back,

        /// <summary>Elle avance contre le dos du corps : sacs, capes.</summary>
        Forward
    }
}
