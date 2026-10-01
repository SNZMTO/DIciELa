using System;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Aller dormir (GDD, section 6) : l'habitant marche jusqu'à la maille devant la porte de son logis, son lit, puis
    /// s'y couche, sans bouger (la vue le montre couché sur sa couche, ou entré), jusqu'à ce qu'on le réveille. Issue
    /// d'échec : en chemin, son lit n'est plus joignable ; il y renonce, et celui qui l'a envoyé décide de la suite. La
    /// marche est celle de l'habitant : pendant qu'il va dormir, il ne flâne pas.
    /// </summary>
    internal sealed class Bedtime
    {
        private readonly Walk _walk;
        private readonly float _metersPerStep;

        public Bedtime(Walk walk, float metersPerStep)
        {
            if (ReferenceEquals(walk, null))
                throw new ArgumentNullException(nameof(walk));

            _walk = walk;
            _metersPerStep = metersPerStep;
        }

        /// <summary>Vrai pendant qu'il marche vers son lit.</summary>
        public bool IsGoing { get; private set; }

        /// <summary>Vrai quand il dort.</summary>
        public bool IsAsleep { get; private set; }

        /// <summary>La maille de son lit, devant sa porte : celle de son dernier départ réussi.</summary>
        public Vector2Int Bed { get; private set; }

        /// <summary>Part de ce point vers le milieu de cette maille, son lit ; faux s'il n'est pas joignable d'ici.</summary>
        public bool TryStart(Vector2 from, Vector2Int bed)
        {
            IsGoing = _walk.TryStart(from, bed);
            if (IsGoing)
                Bed = bed;
            return IsGoing;
        }

        /// <summary>
        /// Un pas vers son lit ; arrivé, il s'endort. Faux s'il y renonce : son lit n'est plus joignable.
        /// </summary>
        public bool Step(ref Vector2 position)
        {
            WalkOutcome outcome = _walk.Advance(ref position, _metersPerStep);
            if (outcome == WalkOutcome.Walking)
                return true;
            IsGoing = false;
            IsAsleep = outcome == WalkOutcome.Arrived;
            return IsAsleep;
        }

        /// <summary>
        /// Il a quitté son trajet pour sortir d'une construction posée sur lui : il repart d'ici vers son lit. Faux s'il y
        /// renonce : son lit n'est plus joignable.
        /// </summary>
        public bool Resume(Vector2 position)
        {
            IsGoing = _walk.TryResume(position);
            return IsGoing;
        }

        /// <summary>Il se lève, ou cesse d'aller dormir, là où il est.</summary>
        public void Stop()
        {
            if (IsGoing)
                _walk.Stop();
            IsGoing = false;
            IsAsleep = false;
        }
    }
}
