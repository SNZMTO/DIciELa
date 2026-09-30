using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Le cadre de bois de l'interface (GDD, section 16 ; décision 145) : son ombre portée, puis sa face (le liseré, le
    /// biseau, le bois et le lin), deux images générées qu'il pose sous son contenu, à sa taille. Ses mesures viennent de
    /// Cadre.uss, générée avec les images. L'ombre est une image et non un filtre : un filtre sur un grand bloc se rend
    /// flou en 4K. Ce qu'il contient se range dans un enfant : vider le cadre lui-même retirerait son ombre et sa face.
    /// </summary>
    [UxmlElement]
    public partial class WoodFrame : VisualElement
    {
        public const string FrameClass = "cadre";
        private const string ShadowClass = "cadre-ombre";
        private const string FaceClass = "cadre-face";

        public WoodFrame()
        {
            AddToClassList(FrameClass);
            Add(Decor(ShadowClass));
            Add(Decor(FaceClass));
        }

        private static VisualElement Decor(string style)
        {
            var decor = new VisualElement { pickingMode = PickingMode.Ignore };
            decor.AddToClassList(style);
            return decor;
        }
    }
}
