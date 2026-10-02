namespace Diciela.EditorTools
{
    /// <summary>
    /// Les dossiers du pack POLYGON Fantasy Village (Synty) où les menus vont chercher ce qu'ils lisent : une seule
    /// écriture de chaque chemin. Rien ne s'y écrit jamais.
    /// </summary>
    internal static class PackPaths
    {
        /// <summary>Les personnages du pack : les animaux (la Chimère) et les humains (les habitants).</summary>
        public const string Characters = "Assets/Synty/PolygonFantasyVillage/Prefabs/Characters";

        /// <summary>Les pièces que les personnages portent : cheveux, chapeaux, barbes…</summary>
        public const string Pieces = Characters + "/Chr_Attach";
    }
}
