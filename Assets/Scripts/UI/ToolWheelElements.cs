using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments de la roue dans Hud.uxml, trouvés par leur nom : ses deux portes, l'icône des outils et sa touche,
    /// et la carte de l'outil en main ; le fond plein écran, le centre, le plateau, la bulle et ses deux lignes, les
    /// boîtes où la roue range ses cases et l'éventail de la case visée, et l'icône qui vole vers le curseur.
    /// </summary>
    internal sealed class ToolWheelElements
    {
        private ToolWheelElements(VisualElement root)
        {
            Icon = root.Q<FeltButton>("outils");
            IconKey = root.Q<Label>("outils-touche");
            Card = root.Q<FeltButton>("carte-outil");
            Wheel = root.Q("roue");
            Center = root.Q("roue-centre");
            Plate = root.Q("roue-plateau");
            Bubble = root.Q("roue-bulle");
            Label = root.Q<Label>("roue-nom");
            Detail = root.Q<Label>("roue-suite");
            Slots = root.Q("roue-cases");
            Fan = root.Q("roue-eventail");
            Flight = root.Q("roue-vol");
        }

        public FeltButton Icon { get; }
        public Label IconKey { get; }
        public FeltButton Card { get; }
        public VisualElement Wheel { get; }
        public VisualElement Center { get; }
        public VisualElement Plate { get; }
        public VisualElement Bubble { get; }
        public Label Label { get; }
        public Label Detail { get; }
        public VisualElement Slots { get; }
        public VisualElement Fan { get; }
        public VisualElement Flight { get; }

        /// <summary>Les éléments de la roue sous cette racine ; null s'il en manque un.</summary>
        public static ToolWheelElements Find(VisualElement root)
        {
            var found = new ToolWheelElements(root);
            bool complete = found.Icon != null && found.IconKey != null && found.Card != null && found.Wheel != null &&
                            found.Center != null && found.Plate != null && found.Bubble != null &&
                            found.Label != null && found.Detail != null && found.Slots != null && found.Fan != null && found.Flight != null;
            return complete ? found : null;
        }

        /// <summary>
        /// Écrit le nom et l'usage de ce qu'on vise dans la bulle ; un nom vide la cache, un usage vide aussi.
        /// </summary>
        public void ShowLabel(string name, string detail)
        {
            Label.text = name;
            Detail.text = detail;
            Detail.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(detail));
            Bubble.EnableInClassList(StyleClasses.Hidden, string.IsNullOrEmpty(name));
        }
    }
}
