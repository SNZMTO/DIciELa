using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Un bouton (ou une touche) qu'on glisse avec la souris : le glisser ne commence qu'au-delà d'un seuil en pixels,
    /// et un appui relâché sans avoir glissé est un clic, décidé au relâchement (SAVOIR, 2.8). Suivi image par image ;
    /// un appui vu pour la première fois part de là où on le voit, même s'il avait commencé plus tôt (pendant le
    /// chargement de la carte, par exemple). C'est une structure : gardée dans un champ, elle se met à jour sur place.
    /// </summary>
    public struct DragGesture
    {
        private bool _pressed;
        private Vector2 _last;

        /// <summary>Là où le bouton a été vu enfoncé.</summary>
        public Vector2 Start { get; private set; }

        /// <summary>Vrai à l'image où l'appui est vu pour la première fois.</summary>
        public bool Began { get; private set; }

        /// <summary>Vrai pendant le glisser, une fois le seuil passé.</summary>
        public bool IsDragging { get; private set; }

        /// <summary>Le déplacement du pointeur depuis la mise à jour précédente, pendant le glisser ; zéro sinon.</summary>
        public Vector2 Delta { get; private set; }

        /// <summary>Vrai à l'image où le bouton est relâché sans avoir glissé : un clic.</summary>
        public bool Clicked { get; private set; }

        /// <summary>Suit le bouton et le pointeur ; vrai à l'image où le glisser commence.</summary>
        public bool Update(bool pressed, Vector2 pointer, float thresholdPixels)
        {
            Delta = Vector2.zero;
            Began = false;
            Clicked = false;
            if (!pressed)
            {
                Clicked = _pressed && !IsDragging;
                _pressed = false;
                IsDragging = false;
                return false;
            }

            if (!_pressed)
            {
                _pressed = true;
                Began = true;
                Start = pointer;
                _last = pointer;
                return false;
            }

            bool starts = !IsDragging && (pointer - Start).magnitude >= thresholdPixels;
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
