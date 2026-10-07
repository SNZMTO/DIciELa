using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Un bouton (ou une touche) qu'on glisse avec la souris : le glisser ne commence qu'au-delà d'un seuil en pixels,
    /// et un appui relâché sans avoir glissé est un clic, décidé au relâchement (SAVOIR, 2.8). Un appui qui commence
    /// sur l'interface lui appartient jusqu'au relâchement : ni glisser ni clic pour la ville ; de même, un appui commencé
    /// quand l'outil n'était pas en main appartient à ce qui l'était (<see cref="HeldElsewhere"/>). Suivi image par
    /// image ; un appui vu pour la première fois part de là où on le voit, même s'il avait commencé plus tôt (pendant le
    /// chargement de la carte, par exemple). C'est une structure : gardée dans un champ, elle se met à jour sur place.
    /// </summary>
    public struct DragGesture
    {
        private bool _pressed;
        private bool _elsewhere;
        private Vector2 _last;

        /// <summary>
        /// Le suivi du geste d'un outil qui n'est pas en main : l'appui en cours, s'il y en a un, appartient à ce qui est
        /// en main. L'outil pris pendant cet appui n'en fait ni glisser ni clic jusqu'au relâchement (une touche qui
        /// change d'outil, le bouton tenu, ne fait pas cliquer le nouvel outil là où l'on tenait l'ancien).
        /// </summary>
        public static DragGesture HeldElsewhere(bool pressed)
        {
            return new DragGesture { _pressed = pressed, _elsewhere = pressed };
        }

        /// <summary>Là où le bouton a été vu enfoncé.</summary>
        public Vector2 Start { get; private set; }

        /// <summary>Vrai pendant le glisser, une fois le seuil passé.</summary>
        public bool IsDragging { get; private set; }

        /// <summary>Le déplacement du pointeur depuis la mise à jour précédente, pendant le glisser ; zéro sinon.</summary>
        public Vector2 Delta { get; private set; }

        /// <summary>Vrai à l'image où le bouton est relâché sans avoir glissé : un clic.</summary>
        public bool Clicked { get; private set; }

        /// <summary>Vrai à l'image où le bouton est relâché après avoir glissé : la fin du glisser.</summary>
        public bool Dropped { get; private set; }

        /// <summary>
        /// Suit le bouton et le pointeur ; <paramref name="onInterface"/> dit si le pointeur est sur l'interface. Vrai
        /// à l'image où le glisser commence.
        /// </summary>
        public bool Update(bool pressed, Vector2 pointer, float thresholdPixels, bool onInterface)
        {
            Delta = Vector2.zero;
            Clicked = false;
            Dropped = false;
            if (!pressed)
            {
                Clicked = _pressed && !IsDragging && !_elsewhere;
                Dropped = IsDragging;
                _pressed = false;
                IsDragging = false;
                return false;
            }

            if (!_pressed)
            {
                _pressed = true;
                _elsewhere = onInterface;
                Start = pointer;
                _last = pointer;
                return false;
            }

            bool starts = !_elsewhere && !IsDragging && (pointer - Start).magnitude >= thresholdPixels;
            if (starts)
                IsDragging = true;
            if (IsDragging)
            {
                Delta = pointer - _last;
                _last = pointer;
            }
            return starts;
        }
    }
}
