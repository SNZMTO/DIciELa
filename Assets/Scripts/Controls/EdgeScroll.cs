using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Le défilement quand le pointeur touche un bord de la fenêtre (GDD, section 16). Il ne s'arme qu'une fois le
    /// pointeur passé loin des bords : au lancement, tant qu'il n'est pas entré dans la vue, le pointeur vaut (0 ; 0),
    /// un coin, et la vue filerait toute seule (SAVOIR, 2.8). Perdre le focus le désarme. C'est une structure : gardée
    /// dans un champ, elle se met à jour sur place.
    /// </summary>
    public struct EdgeScroll
    {
        private bool _armed;

        /// <summary>
        /// La direction du défilement pour ce pointeur (x vers la droite, y vers le haut ; zéro sans défilement), dans
        /// une fenêtre de cette taille, avec une bande de bord de <paramref name="marginPixels"/> pixels.
        /// </summary>
        public Vector2 Update(Vector2 pointer, Vector2 windowSize, float marginPixels, bool focused)
        {
            if (!focused || !(marginPixels > 0f))
            {
                _armed = false;
                return Vector2.zero;
            }
            if (pointer.x < 0f || pointer.y < 0f || pointer.x > windowSize.x || pointer.y > windowSize.y)
                return Vector2.zero;

            var direction = Vector2.zero;
            if (pointer.x <= marginPixels)
                direction.x -= 1f;
            else if (pointer.x >= windowSize.x - marginPixels)
                direction.x += 1f;
            if (pointer.y <= marginPixels)
                direction.y -= 1f;
            else if (pointer.y >= windowSize.y - marginPixels)
                direction.y += 1f;

            if (direction == Vector2.zero)
                _armed = true;
            return _armed ? direction : Vector2.zero;
        }
    }
}
