using System;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Le geste d'une sorte de tâche (TaskWork ; ARCHITECTURE, section 7) : ce qu'il prend aux tâches offertes, où il
    /// mène, ce qu'il fait en arrivant et en travaillant sur place, et ce qu'il rend s'il s'arrête. La tâche en cours le
    /// fait marcher et travailler ; lui seul parle à l'économie. Un habitant a un geste de chaque sorte, qui resservent.
    /// </summary>
    internal abstract class Gesture
    {
        /// <summary>Là où il mène maintenant.</summary>
        public ChoreGoal Goal { get; protected set; }

        /// <summary>Prend une tâche de sa sorte pour un habitant sur cet îlot, à ce point ; faux s'il n'y en a pas.</summary>
        public abstract bool TryTake(int island, Vector2 position);

        /// <summary>
        /// L'habitant est arrivé à son but : ce qu'il fait ensuite. Hors des heures de travail, il n'entame rien.
        /// </summary>
        public abstract GestureStep Arrive(bool workTime);

        /// <summary>
        /// Un pas de travail sur place, de tant d'heures d'un débutant ; <paramref name="built"/> est la construction
        /// qu'il vient de finir, s'il en finit une.
        /// </summary>
        public virtual GestureStep Work(double hours, out Structure built)
        {
            throw new InvalidOperationException("Ce geste ne se travaille pas sur place.");
        }

        /// <summary>La tâche s'arrête : ce qui est encore réservé est rendu, et ce qu'il porte retourne à la ville.</summary>
        public abstract void Stop();
    }
}
