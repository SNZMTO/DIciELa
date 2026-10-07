namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur du curseur des outils attend de sa vue (MVP ; décision 221) : la flèche du système, les
    /// mains vides ; sinon l'allure du curseur de l'outil en main ; et la croix rouge d'un geste qui serait refusé. Où il
    /// se montre (sur la ville, pas sur l'interface), la vue le sait, image après image.
    /// </summary>
    public interface IToolCursorView
    {
        /// <summary>Les mains vides : la flèche.</summary>
        void ShowArrow();

        /// <summary>Un outil en main : son curseur.</summary>
        void ShowLook(ToolCursorLook look);

        /// <summary>Montre la croix rouge, ou la cache.</summary>
        void ShowRefused(bool refused);
    }
}
