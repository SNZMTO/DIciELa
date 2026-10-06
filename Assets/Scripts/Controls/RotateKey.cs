using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// La touche qui tourne ce qu'on pose (GDD, section 9 ; SAVOIR, 2.8) : brève, un cran au relâchement ; tenue en
    /// bougeant la souris, au degré, le curseur tenu à sa place, et la visée attend jusqu'à l'image qui suit le
    /// relâchement, où le curseur rendu est revenu. La pose et la place de charrette s'en servent. C'est une structure :
    /// gardée dans un champ, elle se met à jour sur place.
    /// </summary>
    internal struct RotateKey
    {
        private DragGesture _gesture;
        private CursorGrip _grip;

        /// <summary>
        /// Suit la touche et le pointeur ; <paramref name="turnPixels"/> dit de combien de pixels tourner au degré,
        /// <paramref name="step"/> s'il faut tourner d'un cran. Vrai si l'on peut viser à cette image.
        /// </summary>
        public bool Update(bool pressed, Vector2 pointer, Vector2 look, float thresholdPixels, out float turnPixels,
            out bool step)
        {
            bool gripWasHeld = _grip.IsHeld;
            bool starts = _gesture.Update(pressed, pointer, thresholdPixels, false);
            turnPixels = 0f;
            step = false;
            if (_gesture.IsDragging)
            {
                turnPixels = starts ? _gesture.Delta.x : look.x;
                _grip.Hold(_gesture.Start);
            }
            else
            {
                _grip.Release();
                step = _gesture.Clicked;
            }
            // Le curseur rendu n'est à sa place qu'à l'image suivante : on ne vise pas avec l'ancienne position.
            bool justReleased = gripWasHeld && !_grip.IsHeld;
            return !pressed && !justReleased;
        }

        /// <summary>
        /// L'outil n'est pas en main : le curseur est rendu, et un appui en cours appartient à ce qui l'était
        /// (DragGesture.HeldElsewhere).
        /// </summary>
        public void Reset(bool pressed)
        {
            _grip.Release();
            _gesture = DragGesture.HeldElsewhere(pressed);
        }
    }
}
