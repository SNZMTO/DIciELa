using System;

namespace Diciela.UI
{
    /// <summary>
    /// Le panneau Construire, tel que la roue des outils l'ouvre et le ferme (décision 168) : on sait s'il est ouvert, et
    /// quand il s'ouvre ou se ferme.
    /// </summary>
    public interface IBuildMenu
    {
        /// <summary>Vrai tant que le panneau est ouvert.</summary>
        bool IsOpen { get; }

        /// <summary>Levé quand le panneau s'ouvre ou se ferme.</summary>
        event Action OpenChanged;

        void Open();

        void Close();
    }
}
