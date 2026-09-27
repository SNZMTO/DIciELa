namespace Diciela.Data
{
    /// <summary>
    /// Les clés de textes qui ne sont pas rangées dans une fiche, mais déduites d'un nom : une seule façon de les
    /// former, pour l'interface comme pour le test qui vérifie qu'elles existent.
    /// </summary>
    public static class TextKeys
    {
        private const string PlacementRefusalPrefix = "pose.refus.";

        /// <summary>La clé de la raison d'un refus de pose, d'après son nom (OutsideMap → pose.refus.OutsideMap).</summary>
        public static string PlacementRefusal(string reasonName)
        {
            return PlacementRefusalPrefix + reasonName;
        }
    }
}
