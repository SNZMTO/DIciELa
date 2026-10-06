namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Pourquoi un ordre sur la Place (peindre, gommer, poser ou retirer une place de charrette) se fait ou non (décision
    /// 204). L'aperçu et l'ordre lisent la même réponse, rendue par la même fonction (ARCHITECTURE, section 5). Les
    /// valeurs ne changent jamais de numéro.
    /// </summary>
    public enum PlaceResult
    {
        Ok = 0,

        /// <summary>La forme n'ajoute rien à la Place, ou n'en retire rien.</summary>
        Nothing = 1,

        /// <summary>La Place dépasserait la superficie que permet son niveau.</summary>
        TooLarge = 2,

        /// <summary>La Place ne serait plus d'un seul tenant.</summary>
        NotOnePiece = 3,

        /// <summary>La gomme passerait sous une place de charrette : on retire d'abord la place.</summary>
        MarkInTheWay = 4,

        /// <summary>La place de charrette déborde de la Place.</summary>
        OutsideThePlace = 5,

        /// <summary>Elle chevauche une autre place de charrette.</summary>
        MarksOverlap = 6,

        /// <summary>La Place a déjà toutes les places de charrette que permet son niveau.</summary>
        TooManyMarks = 7,

        /// <summary>Devant elle, au bout du timon, on ne peut pas se tenir pour la garer.</summary>
        NoRoomToPark = 8,

        /// <summary>Aucune place de charrette n'a ce numéro.</summary>
        UnknownMark = 9,

        /// <summary>Sa charrette arrive, ou porte encore des marchandises : on la vide d'abord.</summary>
        CartBusy = 10
    }
}
