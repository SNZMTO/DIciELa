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

        // Les textes de l'écran de création (GDD, section 6) : son titre, ses deux premières lignes, le choix vide d'un
        // emplacement, le bouton qui valide.
        public const string CreationTitle = "creation.titre";
        public const string CreationHead = "creation.tete";
        public const string CreationBody = "creation.corps";
        public const string CreationNone = "creation.aucun";
        public const string CreationConfirm = "creation.valider";

        // La journée, en haut à gauche (GDD, section 16) : son numéro ({jour} y est remplacé).
        public const string DayNumber = "journee.jour";
        public const string DayNumberParameter = "{jour}";

        /// <summary>La clé de la raison d'un refus de pose, d'après son nom (OutsideMap → pose.refus.OutsideMap).</summary>
        public static string PlacementRefusal(string reasonName)
        {
            return PlacementRefusalPrefix + reasonName;
        }

        /// <summary>La clé du nom d'une phase de la journée, d'après son nom (Evening → journee.Evening).</summary>
        public static string DayPhase(string phaseName)
        {
            return DayPhasePrefix + phaseName;
        }
    }
}
