using System;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>La carte reçue au lancement : son point d'arrivée, le centre s'il n'y en a pas, jamais hors de la carte.</summary>
    public sealed class MapDescriptionTests
    {
        private static Relief Relief(float sizeX, float sizeZ)
        {
            return new Relief(sizeX, sizeZ, 2, 2, new float[4]);
        }

        [Test]
        public void KeepsTheArrivalPointItReceives()
        {
            var map = new MapDescription(Relief(40f, 20f), new Vector2(3.5f, 17f));

            Assert.AreEqual(new Vector2(3.5f, 17f), map.ArrivalPoint);
        }

        [Test]
        public void WithoutArrivalPointItIsTheCenter()
        {
            Assert.AreEqual(new Vector2(20f, 10f), new MapDescription(Relief(40f, 20f)).ArrivalPoint);
        }

        [Test]
        public void RefusesAnArrivalPointOffTheMap()
        {
            // Le bord est et le bord nord sont exclus : la dernière maille s'arrête juste avant eux.
            Assert.Throws<ArgumentException>(() => new MapDescription(Relief(40f, 20f), new Vector2(-0.1f, 5f)));
            Assert.Throws<ArgumentException>(() => new MapDescription(Relief(40f, 20f), new Vector2(40f, 5f)));
            Assert.Throws<ArgumentException>(() => new MapDescription(Relief(40f, 20f), new Vector2(5f, 20f)));
            Assert.DoesNotThrow(() => new MapDescription(Relief(40f, 20f), new Vector2(0f, 19.9f)));
        }
    }
}
