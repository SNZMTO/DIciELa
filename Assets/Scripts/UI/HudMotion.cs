using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les petits mouvements de l'interface (GDD, section 16 ; décision 225), faits des transitions de la feuille de
    /// style : USS n'a pas d'images clés. Une entrée pose un état de départ (une classe à durée nulle), puis le retire à
    /// l'image suivante : l'élément rejoint son état en transition, après un retard s'il part après ses frères (posé au
    /// départ, effacé une fois le mouvement fini). Une danse enchaîne deux penchés. Un nombre défile jusqu'à sa valeur.
    /// Aucun ne retient un geste : ce sont des transitions et des minuteries de l'élément.
    /// </summary>
    internal static class HudMotion
    {
        private const string WobbleLeft = "dandine-gauche";
        private const string WobbleRight = "dandine-droite";
        // Une image au moins, à 60 images par seconde : l'état de départ est dessiné avant d'être quitté.
        private const long StartMs = 16;
        // Après le mouvement le plus long (200 ms), le retard posé en ligne s'efface.
        private const long ClearMs = 250;
        private const long WobbleStepMs = 70;
        private const float RollMs = 200f;

        /// <summary>
        /// Joue l'entrée de cet élément : il prend cet état de départ, puis le quitte à l'image suivante, après ce retard.
        /// atStart et atRelease posent et retirent ce qu'une classe ne dit pas (un décalage propre à l'élément).
        /// </summary>
        public static void Enter(VisualElement element, string startClass, long delayMs = 0, Action atStart = null,
            Action atRelease = null)
        {
            element.AddToClassList(startClass);
            atStart?.Invoke();
            element.schedule.Execute(() =>
            {
                if (delayMs > 0)
                    element.style.transitionDelay =
                        new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(delayMs, TimeUnit.Millisecond) });
                element.RemoveFromClassList(startClass);
                atRelease?.Invoke();
            }).StartingIn(StartMs);
            if (delayMs > 0)
                element.schedule.Execute(() => element.style.transitionDelay = StyleKeyword.Null)
                    .StartingIn(StartMs + delayMs + ClearMs);
        }

        /// <summary>Cet élément se dandine : il penche à gauche, puis à droite, puis se redresse.</summary>
        public static void Wobble(VisualElement element)
        {
            element.AddToClassList(WobbleLeft);
            element.schedule.Execute(() =>
            {
                element.RemoveFromClassList(WobbleLeft);
                element.AddToClassList(WobbleRight);
            }).StartingIn(WobbleStepMs);
            element.schedule.Execute(() => element.RemoveFromClassList(WobbleRight)).StartingIn(2 * WobbleStepMs);
        }

        /// <summary>
        /// Fait défiler le nombre en tête de ce texte, de from jusqu'à to, suivi de rest ; rend le défilement, pour
        /// l'arrêter quand un autre le remplace.
        /// </summary>
        public static IVisualElementScheduledItem Roll(Label label, int from, int to, string rest)
        {
            bool done = false;
            long elapsed = 0;
            return label.schedule.Execute(state =>
            {
                // deltaTime : le temps depuis le dernier passage ; on l'additionne pour savoir où en est le défilement.
                elapsed += state.deltaTime;
                float t = Mathf.Clamp01(elapsed / RollMs);
                float eased = 1f - (1f - t) * (1f - t) * (1f - t);
                label.text = Mathf.RoundToInt(Mathf.Lerp(from, to, eased)).ToString(CultureInfo.InvariantCulture) + rest;
                done = t >= 1f;
            }).Every(16).Until(() => done);
        }
    }
}
