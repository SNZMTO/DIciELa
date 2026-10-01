using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les petites vies du HUD (décision 164) : le soleil de la journée tourne lentement ; les fanions du stock se
    /// balancent sous leur tringle, chacun décalé du précédent. Une seule tâche planifiée, une trentaine de fois par
    /// seconde, sur quelques éléments : rien ne s'alloue et rien ne se remet en page (une rotation ne change que le
    /// dessin). Vitesses et amplitudes viennent de la feuille de style (--tour, --balancement, --balancement-duree,
    /// --balancement-decalage) ; un élément qui n'en a pas ne bouge pas.
    /// </summary>
    public sealed class HudLife : IDisposable
    {
        private const long TickMilliseconds = 33;
        private const string SunClass = "journee-icone";
        private const string PennantsName = "fanions";
        private static readonly CustomStyleProperty<float> TurnProperty = new CustomStyleProperty<float>("--tour");
        private static readonly CustomStyleProperty<float> SwayProperty = new CustomStyleProperty<float>("--balancement");
        private static readonly CustomStyleProperty<float> SwaySecondsProperty =
            new CustomStyleProperty<float>("--balancement-duree");
        private static readonly CustomStyleProperty<float> SwayPhaseProperty =
            new CustomStyleProperty<float>("--balancement-decalage");

        private readonly VisualElement _sun;
        private readonly VisualElement _pennants;
        private readonly IVisualElementScheduledItem _tick;

        public HudLife(UIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            VisualElement root = document.rootVisualElement;
            _sun = root.Q(className: SunClass);
            _pennants = root.Q(PennantsName);
            if (_sun == null || _pennants == null)
                throw new InvalidOperationException("Le HUD n'a pas son soleil ou ses fanions (Hud.uxml).");

            _tick = root.schedule.Execute(Tick).Every(TickMilliseconds);
        }

        /// <summary>Arrête les petites vies et remet chaque élément droit.</summary>
        public void Dispose()
        {
            _tick.Pause();
            _sun.style.rotate = StyleKeyword.Null;
            for (int i = 0; i < _pennants.childCount; i++)
                _pennants[i].style.rotate = StyleKeyword.Null;
        }

        private void Tick(TimerState state)
        {
            float seconds = Time.unscaledTime;
            if (_sun.customStyle.TryGetValue(TurnProperty, out float turn))
                _sun.style.rotate = new Rotate(new Angle(Mathf.Repeat(seconds * turn, 360f), AngleUnit.Degree));
            for (int i = 0; i < _pennants.childCount; i++)
            {
                VisualElement pennant = _pennants[i];
                ICustomStyle style = pennant.customStyle;
                if (!style.TryGetValue(SwayProperty, out float amplitude) ||
                    !style.TryGetValue(SwaySecondsProperty, out float period) || period <= 0f)
                    continue;
                style.TryGetValue(SwayPhaseProperty, out float phase);
                float angle = amplitude * Mathf.Sin(2f * Mathf.PI * seconds / period + i * phase);
                pennant.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
            }
        }
    }
}
