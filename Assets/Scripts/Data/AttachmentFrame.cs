namespace Diciela.Data
{
    /// <summary>
    /// Le point d'où se mesure la place des pièces d'un emplacement (GDD, section 6 ; SAVOIR, 2.11) : les pieds du
    /// personnage pour ce que porte le corps (tous les corps ont le même squelette), ou un point mesuré sur la tête
    /// choisie, les têtes n'ayant ni la même forme ni les yeux au même endroit. Liste fermée : chaque point a sa mesure
    /// dans le monteur des têtes.
    /// </summary>
    public enum AttachmentFrame
    {
        /// <summary>Les pieds du personnage : la queue, le cou, le dos, la ceinture.</summary>
        Body,

        /// <summary>Le haut du crâne : chapeaux, cheveux, touffes.</summary>
        Crown,

        /// <summary>Entre les yeux, à leur avant : lunettes, museau, barbe.</summary>
        Face,

        /// <summary>Entre les deux places des oreilles, sur le crâne : oreilles, boucles d'oreilles.</summary>
        Ears,

        /// <summary>Le bas de la tête, sur la ligne du milieu : ce qui se porte au cou.</summary>
        Neck
    }
}
