using System;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Un bouton de feutre (GDD, section 16) : une face qui se clique. Le survol et l'appui viennent de la feuille de
    /// style (:hover, :active) ; ce que dit le bouton est un enfant, rempli par son présentateur.
    /// </summary>
    [UxmlElement]
    public partial class FeltButton : Facet
    {
        /// <summary>Levé quand le joueur clique le bouton.</summary>
        public event Action Clicked;

        public FeltButton()
        {
            this.AddManipulator(new Clickable(() => Clicked?.Invoke()));
        }
    }
}
