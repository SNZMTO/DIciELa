using System;

namespace Diciela.View
{
    /// <summary>
    /// La prévision (GDD, section 16 ; décisions 228 à 230) : une vue où la ville se montre comme si tous ses chantiers
    /// étaient finis, pour mieux s'y projeter et construire. Ce n'est qu'une vue : la simulation n'en sait rien, le
    /// temps continue, on y construit. Un seul interrupteur, qu'allument sa touche et son bouton, qu'Échap éteint ; les
    /// vues du monde, le voile de rêverie et le bouton écoutent quand il change.
    /// </summary>
    public sealed class Foresight
    {
        /// <summary>Levé quand la prévision s'allume ou s'éteint.</summary>
        public event Action Changed;

        /// <summary>Vrai tant que la prévision est allumée.</summary>
        public bool IsOn { get; private set; }

        /// <summary>Allume la prévision, ou l'éteint si elle l'était.</summary>
        public void Toggle()
        {
            Set(!IsOn);
        }

        /// <summary>Allume ou éteint la prévision ; rien ne change si elle l'était déjà.</summary>
        public void Set(bool on)
        {
            if (on == IsOn)
                return;
            IsOn = on;
            Changed?.Invoke();
        }
    }
}
