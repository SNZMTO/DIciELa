using Diciela.Controls;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Un faux panneau d'interface pour les tests des gestes : un rectangle de l'écran, en pixels.</summary>
    internal sealed class TestInterface : IInterfaceArea
    {
        public TestInterface(Rect area)
        {
            Area = area;
        }

        public Rect Area { get; }

        public bool Contains(Vector2 screenPoint)
        {
            return Area.Contains(screenPoint);
        }
    }
}
