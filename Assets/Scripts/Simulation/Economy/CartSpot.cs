using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Une place de charrette de la Place (GDD, section 11 ; décision 204) : la marque que le joueur y a posée
    /// (<see cref="PlaceMark"/>), là où se gare la charrette qu'elle appelle, et son cap. Quelle charrette elle a appelée,
    /// la charrette le dit (<see cref="Cart.Spot"/>). Sa consigne dit ce qu'on porte à la charrette qui y est garée
    /// (décision 202) ; seuls les étals la changent (Stalls).
    /// </summary>
    public sealed class CartSpot
    {
        private readonly List<ConsigneLine> _consigne = new List<ConsigneLine>();

        internal CartSpot(PlaceMark mark)
        {
            Mark = mark;
        }

        /// <summary>La marque de la Place qu'elle est.</summary>
        public PlaceMark Mark { get; }

        /// <summary>Son numéro : celui de sa marque, celui des ordres.</summary>
        public int Id => Mark.Id;

        /// <summary>Le milieu de la charrette garée, au sol, en mètres.</summary>
        public Vector2 Position => Mark.Position;

        /// <summary>Vers où pointe l'avant de la charrette garée, son timon, en degrés depuis le nord vers l'est.</summary>
        public float HeadingDegrees => Mark.HeadingDegrees;

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
    }
}
