using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments de la roue dans Hud.uxml, trouvés par leur nom : l'icône des outils et sa touche, le fond plein
    /// écran, le centre, le plateau, la bulle et ses deux lignes, et les boîtes où la roue range ses cases et l'éventail
    /// de la case visée.
    /// </summary>
    internal sealed class ToolWheelElements
    {
        private ToolWheelElements(VisualElement root)
        {
            Icon = root.Q<FeltButton>("outils");
            IconKey = root.Q<Label>("outils-touche");
            Wheel = root.Q("roue");
            Center = root.Q("roue-centre");
            Plate = root.Q("roue-plateau");
            Bubble = root.Q("roue-bulle");
            Label = root.Q<Label>("roue-nom");
            Detail = root.Q<Label>("roue-suite");
            Slots = root.Q("roue-cases");
            Fan = root.Q("roue-eventail");
        }

        public FeltButton Icon { get; }
        public Label IconKey { get; }
        public VisualElement Wheel { get; }
        public VisualElement Center { get; }
        public VisualElement Plate { get; }
        public VisualElement Bubble { get; }
        public Label Label { get; }
        public Label Detail { get; }
        public VisualElement Slots { get; }
        public VisualElement Fan { get; }

        /// <summary>Les éléments de la roue sous cette racine ; null s'il en manque un.</summary>
        public static ToolWheelElements Find(VisualElement root)
        {
            var found = new ToolWheelElements(root);
            bool complete = found.Icon != null && found.IconKey != null && found.Wheel != null &&
                            found.Center != null && found.Plate != null && found.Bubble != null &&
                            found.Label != null && found.Detail != null && found.Slots != null && found.Fan != null;
            return complete ? found : null;
        }
    }
}
