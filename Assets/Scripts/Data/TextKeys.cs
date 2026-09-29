namespace Diciela.Data
{
    /// <summary>
    /// Les clés de textes qui ne sont pas rangées dans une fiche : celles de l'interface, et celles déduites d'un nom,
    /// avec une seule façon de les former, pour l'interface comme pour le test qui vérifie qu'elles existent.
    /// </summary>
    public static class TextKeys
    {
        private const string PlacementRefusalPrefix = "pose.refus.";

        // Les textes de l'écran de création (GDD, section 6) : son titre, ses deux premières lignes, le choix vide d'un
        // emplacement, les flèches, le bouton qui valide.
        public const string CreationTitle = "creation.titre";
        public const string CreationHead = "creation.tete";
        public const string CreationBody = "creation.corps";
        public const string CreationNone = "creation.aucun";
        public const string CreationPrevious = "creation.precedent";
        public const string CreationNext = "creation.suivant";
        public const string CreationConfirm = "creation.valider";

        /// <summary>La clé de la raison d'un refus de pose, d'après son nom (OutsideMap → pose.refus.OutsideMap).</summary>
        public static string PlacementRefusal(string reasonName)
        {
            return PlacementRefusalPrefix + reasonName;
        }
    }
}
