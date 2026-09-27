namespace Diciela.Game
{
    /// <summary>
    /// Les chemins des scènes du jeu (ARCHITECTURE, section 10), lus par les menus de l'éditeur et par les tests. En jeu,
    /// la scène racine lit sa carte dans sa fiche de session.
    /// </summary>
    public static class ScenePaths
    {
        /// <summary>La scène racine du jeu : démarrage, caméra, interface.</summary>
        public const string Game = "Assets/Scenes/Jeu.unity";

        /// <summary>La carte peinte par le développeur.</summary>
        public const string Map = "Assets/Scenes/Carte.unity";

        /// <summary>La scène racine des tests : elle charge la carte de test.</summary>
        public const string GameTest = "Assets/Scenes/Test/JeuTest.unity";

        /// <summary>Une petite carte plate, que charge la scène de test.</summary>
        public const string MapTest = "Assets/Scenes/Test/CarteTest.unity";
    }
}
