using System;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les éléments de la roue des outils dans Hud.uxml, trouvés par leur nom : l'icône des outils et sa touche, le fond
    /// plein écran, le centre, le plateau, la bulle et ses deux lignes, et les outils au rang de leur choix
    /// (ToolChoice), chacun avec sa touche, nommés par la table des outils (WheelTool).
    /// </summary>
    internal sealed class ToolWheelElements
    {
        private const string KeySuffix = "-touche";

        private ToolWheelElements(VisualElement root)
        {
            Icon = root.Q<FeltButton>("outils");
            IconKey = root.Q<Label>("outils" + KeySuffix);
            Wheel = root.Q("roue");
            Center = root.Q("roue-centre");
            Plate = root.Q("roue-plateau");
            Bubble = root.Q("roue-bulle");
            Label = root.Q<Label>("roue-nom");
            Detail = root.Q<Label>("roue-suite");
            Tools = new FeltButton[WheelTool.All.Count];
            ToolKeys = new Label[Tools.Length];
            for (int i = 0; i < Tools.Length; i++)
            {
                Tools[i] = root.Q<FeltButton>(WheelTool.All[i].ElementName);
                ToolKeys[i] = root.Q<Label>(WheelTool.All[i].ElementName + KeySuffix);
            }
        }

        public FeltButton Icon { get; }
        public Label IconKey { get; }
        public VisualElement Wheel { get; }
        public VisualElement Center { get; }
        public VisualElement Plate { get; }
        public VisualElement Bubble { get; }
        public Label Label { get; }
        public Label Detail { get; }
        public FeltButton[] Tools { get; }
        public Label[] ToolKeys { get; }

        /// <summary>Les éléments de la roue sous cette racine ; null s'il en manque un.</summary>
        public static ToolWheelElements Find(VisualElement root)
        {
            var found = new ToolWheelElements(root);
            bool complete = found.Icon != null && found.IconKey != null && found.Wheel != null &&
                            found.Center != null && found.Plate != null && found.Bubble != null &&
                            found.Label != null && found.Detail != null && Array.IndexOf(found.Tools, null) < 0 &&
                            Array.IndexOf(found.ToolKeys, null) < 0;
            return complete ? found : null;
        }
    }
}
