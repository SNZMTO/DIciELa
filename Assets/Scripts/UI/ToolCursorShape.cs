using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Une silhouette de curseur (décision 221) : la classe de la feuille de style qui la dessine, et sa pointe, là où
    /// l'outil agit, en pixels du curseur ; c'est elle qui se pose sur le pointeur.
    /// </summary>
    internal readonly struct ToolCursorShape
    {
        public readonly string Silhouette;
        public readonly Vector2 Tip;

        public ToolCursorShape(string silhouette, Vector2 tip)
        {
            Silhouette = silhouette;
            Tip = tip;
        }
    }
}
