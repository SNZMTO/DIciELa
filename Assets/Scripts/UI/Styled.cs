using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments que le code fabrique, habillés de leurs classes de style (Fusion.uss, Hud.uss, Creation.uss) : la
    /// seule façon de poser des classes sur un élément neuf, et d'en faire un texte que le pointeur traverse.
    /// </summary>
    internal static class Styled
    {
        /// <summary>Cet élément, avec ces classes de style.</summary>
        public static T With<T>(T element, params string[] classes) where T : VisualElement
        {
            foreach (string style in classes)
                element.AddToClassList(style);
            return element;
        }

        /// <summary>Un texte avec ces classes, que le pointeur traverse : il ne prend jamais un appui.</summary>
        public static Label Text(string text, params string[] classes)
        {
            return With(new Label(text) { pickingMode = PickingMode.Ignore }, classes);
        }
    }
}
