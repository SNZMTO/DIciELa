using System;
using Diciela.Data;

namespace Diciela.Controls
{
    /// <summary>
    /// Ce que pose un outil de sol (décision 210), dit en toutes lettres : la zone de la Place, un sol, ou rien (l'outil
    /// Chemins tant que la ville ne sait faire aucun sol ; sa gomme efface encore). Deux encres sont égales si elles
    /// posent la même chose.
    /// </summary>
    public readonly struct PaintInk : IEquatable<PaintInk>
    {
        private PaintInk(bool isZone, FloorDefinition floor)
        {
            IsZone = isZone;
            Floor = floor;
        }

        /// <summary>Rien à poser.</summary>
        public static PaintInk None => default;

        /// <summary>La zone de la Place.</summary>
        public static PaintInk Zone => new PaintInk(true, null);

        /// <summary>Ce sol.</summary>
        public static PaintInk Of(FloorDefinition floor)
        {
            if (floor == null)
                throw new ArgumentNullException(nameof(floor));
            return new PaintInk(false, floor);
        }

        /// <summary>Vrai pour la zone de la Place.</summary>
        public bool IsZone { get; }

        /// <summary>Le sol à poser ; null pour la zone et pour rien.</summary>
        public FloorDefinition Floor { get; }

        /// <summary>Vrai si l'outil ne pose rien.</summary>
        public bool IsNone => !IsZone && Floor == null;

        public bool Equals(PaintInk other)
        {
            return IsZone == other.IsZone && ReferenceEquals(Floor, other.Floor);
        }

        public override bool Equals(object obj)
        {
            return obj is PaintInk other && Equals(other);
        }

        public override int GetHashCode()
        {
            return IsZone ? 1 : Floor == null ? 0 : Floor.GetHashCode();
        }
    }
}
