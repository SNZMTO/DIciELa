namespace Diciela.Data
{
    /// <summary>
    /// Les clés de textes qui ne sont pas rangées dans une fiche : celles de l'interface, et celles déduites d'un nom,
    /// avec une seule façon de les former, pour l'interface comme pour le test qui vérifie qu'elles existent.
    /// </summary>
    public static class TextKeys
    {
        private const string PlacementRefusalPrefix = "pose.refus.";
        private const string DayPhasePrefix = "journee.";
        private const string DayNextPrefix = "journee.suite.";
        private const string DayNotePrefix = "journee.note.";

        // Les textes de l'écran de création (GDD, section 6) : son titre, ses deux premières lignes, le choix vide d'un
        // emplacement, le bouton qui valide.
        public const string CreationTitle = "creation.titre";
        public const string CreationHead = "creation.tete";
        public const string CreationBody = "creation.corps";
        public const string CreationNone = "creation.aucun";
        public const string CreationConfirm = "creation.valider";

        // La fiche d'un lieu (GDD, section 16 ; décision 141) : son bouton, et qui tient son poste.
        public const string SheetPutChimera = "fiche.mettre_chimere";
        public const string SheetRemoveChimera = "fiche.retirer_chimere";
        public const string SheetChimera = "fiche.la_chimere";
        public const string SheetFreeSlot = "fiche.poste_libre";

        // L'astuce (GDD, section 16 ; décision 154) : ce qu'il y a à faire ensuite, au début d'une partie.
        public const string TipTent = "astuce.tente";
        public const string TipPile = "astuce.tas";
        public const string TipCabin = "astuce.cabane";

        // Les fanions du stock (GDD, section 16) : l'infobulle d'un fanion ({marchandise} y est remplacé).
        public const string StockTooltip = "fanion.infobulle";
        public const string GoodParameter = "{marchandise}";

        // La journée, en haut à gauche (GDD, section 16) : son numéro ({jour} y est remplacé).
        public const string DayNumber = "journee.jour";
        public const string DayNumberParameter = "{jour}";

        /// <summary>La clé de la raison d'un refus de pose, d'après son nom (OutsideMap → pose.refus.OutsideMap).</summary>
        public static string PlacementRefusal(string reasonName)
        {
            return PlacementRefusalPrefix + reasonName;
        }

        /// <summary>L'infobulle de la journée, pendant cette phase : ce qui suit (Work → journee.suite.Work).</summary>
        public static string DayNext(string phaseName)
        {
            return DayNextPrefix + phaseName;
        }

        /// <summary>La suite de l'infobulle de la journée, pendant cette phase (Night → journee.note.Night).</summary>
        public static string DayNote(string phaseName)
        {
            return DayNotePrefix + phaseName;
        }

        /// <summary>La clé du nom d'une phase de la journée, d'après son nom (Evening → journee.Evening).</summary>
        public static string DayPhase(string phaseName)
        {
            return DayPhasePrefix + phaseName;
        }
    }
}
