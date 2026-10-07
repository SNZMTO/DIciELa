using System;

namespace Diciela.UI
{
    /// <summary>
    /// Le panneau Construire, tel que la roue des outils l'ouvre et le ferme (décisions 168 et 221) : on sait s'il est
    /// ouvert, quand il s'ouvre ou se ferme, et si la ville propose déjà la Place.
    /// </summary>
    public interface IBuildMenu
    {
        /// <summary>Vrai tant que le panneau est ouvert.</summary>
        bool IsOpen { get; }

        /// <summary>Vrai si la ville propose la Place : dès le campement posé (GDD, section 6).</summary>
        bool OffersPlaza { get; }

        /// <summary>Levé quand le panneau s'ouvre ou se ferme.</summary>
        event Action OpenChanged;

        void Open();

        void Close();
    }
}
