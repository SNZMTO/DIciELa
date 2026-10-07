using System;

namespace Diciela.Controls
{
    /// <summary>
    /// Un outil qu'on a en main (poser, retirer, déplacer, les outils de sol) : tant qu'il l'est, le clic gauche sur la
    /// ville est à lui, pas au geste « toucher ». Le clic droit (Cancel) défait d'abord ce qu'il tient (le fantôme qu'on
    /// déplace, le geste en cours d'un outil de sol), sinon le lâche ; Échap fait de même, par la main (Hand), qui le
    /// lâche aussi tout à fait quand on en prend un autre.
    /// </summary>
    public interface IHeldTool
    {
        /// <summary>Vrai tant qu'il est en main.</summary>
        bool IsActive { get; }

        /// <summary>Levé quand on le prend en main, qu'on le lâche, ou qu'on change ce qu'il tient.</summary>
        event Action HeldChanged;

        /// <summary>Le clic droit : défait ce qu'il tient, sinon le lâche.</summary>
        void Cancel();

        /// <summary>Le lâche tout à fait, avec ce qu'il tient.</summary>
        void PutDown();
    }
}
