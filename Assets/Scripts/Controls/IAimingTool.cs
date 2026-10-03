using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Un outil qui vise le sol et se pose d'un clic (poser, déplacer) : les gestes de la pose (PlacementControls) le
    /// pilotent, quel qu'il soit, tant qu'il est en main. Ils le visent à chaque image, le tournent d'un cran ou au degré,
    /// le confirment d'un clic gauche et l'annulent d'un clic droit.
    /// </summary>
    public interface IAimingTool
    {
        /// <summary>Vrai tant qu'il est en main.</summary>
        bool IsActive { get; }

        /// <summary>Vise le sol sous ce point de l'écran.</summary>
        void AimAt(Vector2 screenPoint);

        /// <summary>Le pointeur ne vise plus le sol (il est sur l'interface, ou dans le ciel).</summary>
        void LoseAim();

        /// <summary>Tourne d'un cran.</summary>
        void RotateStep();

        /// <summary>Tourne de cet angle, en degrés ; un angle positif tourne vers l'est, comme la caméra.</summary>
        void RotateBy(float degrees);

        /// <summary>Le clic gauche : vrai si quelque chose est fait.</summary>
        bool Confirm();

        /// <summary>Le clic droit.</summary>
        void Cancel();
    }
}
