using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// L'allure du curseur d'un outil en main (décision 221) : sa silhouette (une classe de la feuille de style) et sa
    /// pointe, en pixels du curseur ; la couleur de son outil ; ses pastilles : ce qu'il pose (l'icône de la zone, ou la
    /// vignette d'un sol), sa forme (celle du mode Forme, ou le style d'un tracé), sa symétrie (des classes de la
    /// feuille de style). Une pastille vide ne paraît pas.
    /// </summary>
    public readonly struct ToolCursorLook
    {
        public ToolCursorLook(string silhouette, Vector2 tip, string pastel, HudFace ink, string formIcon,
            string symmetryIcon)
        {
            Silhouette = silhouette;
            Tip = tip;
            Pastel = pastel;
            Ink = ink;
            FormIcon = formIcon ?? "";
            SymmetryIcon = symmetryIcon ?? "";
        }

        public string Silhouette { get; }
        public Vector2 Tip { get; }
        public string Pastel { get; }
        public HudFace Ink { get; }
        public string FormIcon { get; }
        public string SymmetryIcon { get; }
    }
}
