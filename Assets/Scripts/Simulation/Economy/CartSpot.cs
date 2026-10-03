using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Une place de charrette de la Place (GDD, section 11) : là où se gare la charrette qu'elle appelle, et son cap, tirés
    /// au hasard quand la Place est bâtie (CartSpotLayout), et qui la suivent quand on la déplace. Quelle charrette elle a
    /// appelée, la charrette le dit (<see cref="Cart.Spot"/>). Sa consigne dit ce qu'on porte à la charrette qui y est
    /// garée (décision 202) ; seuls les étals la changent (Stalls).
    /// </summary>
    public sealed class CartSpot
    {
        private readonly List<ConsigneLine> _consigne = new List<ConsigneLine>();

        internal CartSpot(Structure place, int index, Vector2 position, float headingDegrees)
        {
            Place = place;
            Index = index;
            Position = position;
            HeadingDegrees = headingDegrees;
        }

        /// <summary>La Place qui l'offre.</summary>
        public Structure Place { get; }

        /// <summary>Son rang sur la Place, à partir de 0.</summary>
        public int Index { get; }

        /// <summary>Le milieu de la charrette garée, au sol, en mètres.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Vers où pointe l'avant de la charrette garée, son timon, en degrés depuis le nord vers l'est.</summary>
        public float HeadingDegrees { get; private set; }

        /// <summary>Sa consigne : ses lignes, dans l'ordre où elles se servent.</summary>
        public IReadOnlyList<ConsigneLine> Consigne => _consigne;

        /// <summary>
        /// Là où se tient qui gare ou garnit la charrette : devant elle, au bout de son timon de tant de mètres.
        /// </summary>
        public Vector2 StandAt(float pullMeters)
        {
            return Position + Heading.Direction(HeadingDegrees) * pullMeters;
        }

        /// <summary>La ligne de cette marchandise ; faux si la consigne n'en a pas.</summary>
        public bool TryGetLine(GoodDefinition good, out ConsigneLine line)
        {
            for (int i = 0; i < _consigne.Count; i++)
            {
                if (ReferenceEquals(_consigne[i].Good, good))
                {
                    line = _consigne[i];
                    return true;
                }
            }
            line = default;
            return false;
        }

        /// <summary>Met cette ligne à la place de celle de la même marchandise, ou au bout de la consigne.</summary>
        internal void SetLine(ConsigneLine line)
        {
            for (int i = 0; i < _consigne.Count; i++)
            {
                if (ReferenceEquals(_consigne[i].Good, line.Good))
                {
                    _consigne[i] = line;
                    return;
                }
            }
            _consigne.Add(line);
        }

        /// <summary>Retire la ligne de cette marchandise ; faux s'il n'y en avait pas.</summary>
        internal bool RemoveLine(GoodDefinition good)
        {
            for (int i = 0; i < _consigne.Count; i++)
            {
                if (ReferenceEquals(_consigne[i].Good, good))
                {
                    _consigne.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Sa Place a changé de place : elle suit.</summary>
        internal void MoveTo(Vector2 position, float headingDegrees)
        {
            Position = position;
            HeadingDegrees = headingDegrees;
        }
    }
}
