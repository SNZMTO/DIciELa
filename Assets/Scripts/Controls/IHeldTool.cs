using System;

namespace Diciela.Controls
{
    /// <summary>
    /// Un outil qu'on a en main (poser, retirer) : tant qu'il l'est, le clic gauche sur la ville est à lui, pas au geste
    /// « toucher ». Échap le lâche ; la main (Hand) le lâche aussi quand on en prend un autre.
    /// </summary>
    public interface IHeldTool : IEscapable
    {
        /// <summary>Vrai tant qu'il est en main.</summary>
        bool IsActive { get; }

        /// <summary>Levé quand on le prend en main, qu'on le lâche, ou qu'on change ce qu'il tient.</summary>
        event Action HeldChanged;
    }
}
