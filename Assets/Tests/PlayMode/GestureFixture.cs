using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le banc des tests des gestes : l'Input System coupé des vrais appareils (InputTestFixture), une souris et un
    /// clavier simulés. Un événement simulé est lu au début de l'image suivante ; le test reprend après les Update de
    /// cette image, mais avant ses LateUpdate, où la pose vise et où la caméra se place : il attend donc une image de
    /// plus avant de regarder.
    /// </summary>
    public abstract class GestureFixture : InputTestFixture
    {
        // Un déplacement de souris bien au-delà du seuil du glisser (6 px par défaut), en pixels.
        protected const float Moved = 40f;

        protected Mouse Mouse { get; private set; }
        protected Keyboard Keyboard { get; private set; }

        public override void Setup()
        {
            base.Setup();
            Mouse = InputSystem.AddDevice<Mouse>();
            Keyboard = InputSystem.AddDevice<Keyboard>();
        }

        /// <summary>Attend que les gestes aient joué les événements simulés jusqu'ici, et que la vue soit posée.</summary>
        protected static IEnumerator Played()
        {
            yield return null;
            yield return null;
        }

        /// <summary>Un clic bref : enfoncé à une image, relâché à la suivante, joué jusqu'au bout.</summary>
        protected IEnumerator Click(ButtonControl button)
        {
            Press(button);
            yield return Played();
            Release(button);
            yield return Played();
        }
    }
}
