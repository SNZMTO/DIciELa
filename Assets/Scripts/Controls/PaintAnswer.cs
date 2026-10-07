using System;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Controls
{
    /// <summary>
    /// La réponse à ce que vise un outil de sol (décisions 206 et 210), quelle que soit la question, la Place ou les sols :
    /// le geste se trace, ou ne change rien, ou il est refusé, et la clé de sa raison dans la table des textes. Seul endroit
    /// où une réponse de la Place ou des sols se traduit.
    /// </summary>
    public readonly struct PaintAnswer : IEquatable<PaintAnswer>
    {
        /// <summary>Le geste se trace (ou rien n'est visé).</summary>
        public static readonly PaintAnswer Ok = new PaintAnswer(true, false, null);

        private PaintAnswer(bool isOk, bool changesNothing, string reasonKey)
        {
            IsOk = isOk;
            ChangesNothing = changesNothing;
            ReasonKey = reasonKey;
        }

        public bool IsOk { get; }

        /// <summary>Vrai si le geste ne change rien : il ne part pas, mais n'est pas une faute (rien à dire).</summary>
        public bool ChangesNothing { get; }

        /// <summary>La clé de la raison d'un refus ; null quand le geste se trace.</summary>
        public string ReasonKey { get; }

        /// <summary>La réponse de la Place.</summary>
        public static PaintAnswer Of(PlaceResult result)
        {
            return result == PlaceResult.Ok
                ? Ok
                : new PaintAnswer(false, result == PlaceResult.Nothing, TextKeys.PlaceRefusal(result.ToString()));
        }

        /// <summary>La réponse des sols.</summary>
        public static PaintAnswer Of(FloorResult result)
        {
            return result == FloorResult.Ok
                ? Ok
                : new PaintAnswer(false, result == FloorResult.Nothing, TextKeys.FloorRefusal(result.ToString()));
        }

        public bool Equals(PaintAnswer other)
        {
            return IsOk == other.IsOk && ChangesNothing == other.ChangesNothing && ReasonKey == other.ReasonKey;
        }

        public override bool Equals(object other)
        {
            return other is PaintAnswer answer && Equals(answer);
        }

        public override int GetHashCode()
        {
            return ReasonKey == null ? 0 : ReasonKey.GetHashCode();
        }

        public static bool operator ==(PaintAnswer a, PaintAnswer b) => a.Equals(b);
        public static bool operator !=(PaintAnswer a, PaintAnswer b) => !a.Equals(b);

        public override string ToString()
        {
            return IsOk ? "Ok" : ReasonKey;
        }
    }
}
