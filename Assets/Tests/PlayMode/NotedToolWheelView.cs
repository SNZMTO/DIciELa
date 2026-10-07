using System;
using System.Collections.Generic;
using Diciela.UI;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Une vue de la roue qui note ce que le présentateur lui montre : la touche de la roue, les cases de la roue ouverte,
    /// la bulle. Ce que le joueur vise et prend, les tests le disent au présentateur, comme la vraie vue.
    /// </summary>
    internal sealed class NotedToolWheelView : IToolWheelView
    {
        public string WheelKey { get; private set; }
        public IReadOnlyList<WheelSlot> Ring { get; private set; } = Array.Empty<WheelSlot>();
        public (string Name, string Detail) Label { get; private set; }

        /// <summary>Le rang, dans la roue montrée, de la case à cette place ; -1 si elle n'y est pas.</summary>
        public int IndexAt(int place)
        {
            for (int i = 0; i < Ring.Count; i++)
            {
                if (Ring[i].Place == place)
                    return i;
            }
            return -1;
        }

        /// <summary>La case à cette place dans la roue montrée ; null si elle n'y est pas.</summary>
        public WheelSlot At(int place)
        {
            int index = IndexAt(place);
            return index >= 0 ? Ring[index] : null;
        }

        public void ShowWheelKey(string key)
        {
            WheelKey = key;
        }

        public void ShowRing(IReadOnlyList<WheelSlot> slots)
        {
            Ring = slots;
        }

        public void ShowLabel(string name, string detail)
        {
            Label = (name, detail);
        }
    }
}
