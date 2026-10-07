using System.Collections.Generic;

namespace Diciela.Controls
{
    /// <summary>
    /// Un outil de la roue pour ses gestes (décision 168) : son choix, le nom de son action dans la carte « Tools » de
    /// l'asset d'actions, et la touche qui la lie d'origine, au caractère (SAVOIR, section 3). La table des outils est ici,
    /// au rang de leur choix : les gestes la parcourent, et le monteur des actions aussi. Un outil de plus est une ligne
    /// de plus, ici et dans la table de l'interface (WheelTool).
    /// </summary>
    public readonly struct ToolAction
    {
        public static readonly IReadOnlyList<ToolAction> All = new[]
        {
            new ToolAction(ToolChoice.Build, "Build", "<Keyboard>/#(b)"),
            new ToolAction(ToolChoice.Clear, "Clear", "<Keyboard>/#(x)"),
            new ToolAction(ToolChoice.Move, "Move", "<Keyboard>/#(m)"),
            new ToolAction(ToolChoice.Paths, "Paths", "<Keyboard>/#(c)")
        };

        public readonly ToolChoice Choice;
        public readonly string Name;
        public readonly string DefaultBinding;

        private ToolAction(ToolChoice choice, string name, string defaultBinding)
        {
            Choice = choice;
            Name = name;
            DefaultBinding = defaultBinding;
        }

        /// <summary>L'outil de ce choix.</summary>
        public static ToolAction Of(ToolChoice choice)
        {
            return All[(int)choice];
        }
    }
}
