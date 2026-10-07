using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// L'allure du curseur d'un outil en main (décision 221) : sa silhouette (une classe de la feuille de style) et sa
    /// pointe, en pixels du curseur ; la couleur de son outil ; ses pastilles : ce qu'il pose (l'icône de la zone, ou la
    /// vignette d'un sol), sa forme (celle du mode Forme, ou le style d'un tracé), sa symétrie. Une pastille vide ne
    /// paraît pas.
    /// </summary>
    public readonly struct ToolCursorLook
    {
        public ToolCursorLook(string silhouette, Vector2 tip, string pastel, string inkIcon, Texture2D inkPicture,
            string formIcon, string symmetryIcon)
        {
            Silhouette = silhouette;
            Tip = tip;
            Pastel = pastel;
            InkIcon = inkIcon ?? "";
            InkPicture = inkPicture;
            FormIcon = formIcon ?? "";
            SymmetryIcon = symmetryIcon ?? "";
        }

        public string Silhouette { get; }
        public Vector2 Tip { get; }
        public string Pastel { get; }
        public string InkIcon { get; }
        public Texture2D InkPicture { get; }
        public string FormIcon { get; }
        public string SymmetryIcon { get; }
    }
}
