using System;

namespace Diciela.UI
{
    /// <summary>Ce qu'un outil de la roue allume et éteint, et s'il est allumé (décision 221).</summary>
    internal readonly struct ToolSwitch
    {
        public readonly Func<bool> IsOn;
        public readonly Action On;
        public readonly Action Off;

        public ToolSwitch(Func<bool> isOn, Action on, Action off)
        {
            IsOn = isOn;
            On = on;
            Off = off;
        }
    }
}
