using System;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Le geste d'une sorte de tâche (TaskWork ; ARCHITECTURE, section 7) : ce qu'il prend aux tâches offertes, où il
    /// mène, ce qu'il fait en arrivant et en travaillant sur place, et ce qu'il rend s'il s'arrête. La tâche en cours
    /// le fait marcher et travailler ; lui seul parle à l'économie. Il peut ralentir la marche, et suit chacun de ses
    /// pas. Un habitant a un geste de chaque sorte, qui resservent.
    /// </summary>
    internal abstract class Gesture
    {
        /// <summary>Là où il mène maintenant.</summary>
        public ChoreGoal Goal { get; protected set; }

        /// <summary>Prend une tâche de sa sorte pour un habitant sur cet îlot, à ce point ; faux s'il n'y en a pas.</summary>
        public abstract bool TryTake(int island, Vector2 position);

        /// <summary>
        /// L'habitant est arrivé à son but, en ce point : ce qu'il fait ensuite. Hors des heures de travail, il n'entame
        /// rien.
        /// </summary>
        public abstract GestureStep Arrive(Vector2 position, bool workTime);

        /// <summary>Son pas, en part du pas de l'habitant : moins vite s'il tire quelque chose.</summary>
        public virtual float SpeedFactor => 1f;

        /// <summary>L'habitant a marché jusqu'ici : ce qu'il tire le suit.</summary>
        public virtual void Walked(Vector2 position)
        {
        }

        /// <summary>
        /// Un pas de travail sur place, de tant d'heures d'un débutant. Ce qu'il finit de bâtir, le monde le met en
        /// service à la fin du pas.
        /// </summary>
        public virtual GestureStep Work(double hours)
        {
            throw new InvalidOperationException("Ce geste ne se travaille pas sur place.");
        }

        /// <summary>La tâche s'arrête : ce qui est encore réservé est rendu, et ce qu'il porte retourne à la ville.</summary>
        public abstract void Stop();
    }
}
