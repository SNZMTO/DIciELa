using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que le présentateur de la roue attend de sa vue (MVP ; décision 221) : écrire la touche de la roue, montrer les
    /// cases de la roue ouverte et la bulle de ce qu'on vise. Les textes arrivent déjà traduits ; ce que le joueur vise et
    /// prend, la vue le dit au présentateur.
    /// </summary>
    public interface IToolWheelView
    {
        /// <summary>Écrit la touche de la roue, sur l'icône des outils.</summary>
        void ShowWheelKey(string key);

        /// <summary>Montre les cases de la roue, chacune à sa place, avec ses options en éventail.</summary>
        void ShowRing(IReadOnlyList<WheelSlot> slots);

        /// <summary>Écrit le nom et l'usage de ce qu'on vise dans la bulle du milieu ; un nom vide la cache.</summary>
        void ShowLabel(string name, string detail);
    }
}
