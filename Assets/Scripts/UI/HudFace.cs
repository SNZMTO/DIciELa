using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que montre une case de la roue, un jeton de son éventail, une puce de la carte ou une pastille du curseur
    /// (décision 226) : une icône de l'interface, que nomme sa classe de style ; une icône des données (celle d'une
    /// famille, d'une marchandise) ; ou une vignette (la texture d'un sol, la photo d'une construction) ; rien, s'il
    /// n'a aucune des trois. Il se peint sur un élément, à la taille que donne une classe de la feuille de style.
    /// </summary>
    public readonly struct HudFace
    {
        private const string PictureClass = "roue-vignette";

        private readonly string _iconClass;

        private HudFace(string iconClass, VectorImage icon, Texture2D picture)
        {
            _iconClass = iconClass;
            Icon = icon;
            Picture = picture;
        }

        /// <summary>L'icône de l'interface que nomme cette classe de style ; une classe vide ne montre rien.</summary>
        public static HudFace Styled(string iconClass)
        {
            return new HudFace(iconClass, null, null);
        }

        /// <summary>Une icône des données ; null ne montre rien.</summary>
        public static HudFace Of(VectorImage icon)
        {
            return new HudFace(null, icon, null);
        }

        /// <summary>Une vignette ; null ne montre rien.</summary>
        public static HudFace Photo(Texture2D picture)
        {
            return new HudFace(null, null, picture);
        }

        /// <summary>La classe de son icône de l'interface ; vide s'il n'en a pas.</summary>
        public string IconClass => _iconClass ?? "";

        /// <summary>Son icône des données ; null s'il n'en a pas.</summary>
        public VectorImage Icon { get; }

        /// <summary>Sa vignette ; null s'il n'en a pas.</summary>
        public Texture2D Picture { get; }

        public bool IsEmpty => IconClass.Length == 0 && Icon == null && Picture == null;

        /// <summary>
        /// Se peint sur cet élément, à la taille que donne cette classe ; ses autres classes s'en vont.
        /// </summary>
        public void Paint(VisualElement element, string sizeClass)
        {
            element.ClearClassList();
            element.AddToClassList(sizeClass);
            if (Picture != null)
            {
                element.AddToClassList(PictureClass);
                element.style.backgroundImage = new StyleBackground(Picture);
                return;
            }
            if (Icon != null)
            {
                element.style.backgroundImage = Background.FromVectorImage(Icon);
                return;
            }
            element.style.backgroundImage = StyleKeyword.Null;
            if (IconClass.Length > 0)
                element.AddToClassList(IconClass);
        }
    }
}
