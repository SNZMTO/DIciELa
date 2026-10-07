using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Le visage d'une case ou d'un jeton de la roue (décision 221) : l'icône que nomme sa classe, ou une vignette (la
    /// texture d'un sol), à la taille que donne une classe de la feuille de style.
    /// </summary>
    internal static class WheelPicture
    {
        private const string PictureClass = "roue-vignette";

        public static void Paint(VisualElement face, string sizeClass, string icon, Texture2D picture)
        {
            face.ClearClassList();
            face.AddToClassList(sizeClass);
            if (picture != null)
            {
                face.AddToClassList(PictureClass);
                face.style.backgroundImage = new StyleBackground(picture);
                return;
            }
            face.style.backgroundImage = StyleKeyword.Null;
            if (icon.Length > 0)
                face.AddToClassList(icon);
        }
    }
}
