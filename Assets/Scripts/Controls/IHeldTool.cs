using System;

namespace Diciela.Controls
{
    /// <summary>
    /// Un outil qu'on a en main (poser, retirer, déplacer, tracer la Place) : tant qu'il l'est, le clic gauche sur la
    /// ville est à lui, pas au geste « toucher ». Échap le lâche, ou défait d'abord ce qu'il tient (le fantôme qu'on
    /// déplace, le geste en cours de la Place) ; la main (Hand) le lâche tout à fait quand on en prend un autre.
    /// </summary>
    public interface IHeldTool : IEscapable
    {
        /// <summary>Vrai tant qu'il est en main.</summary>
        bool IsActive { get; }

        /// <summary>Levé quand on le prend en main, qu'on le lâche, ou qu'on change ce qu'il tient.</summary>
        event Action HeldChanged;

        /// <summary>Le lâche tout à fait, avec ce qu'il tient.</summary>
        void PutDown();
    }
}
