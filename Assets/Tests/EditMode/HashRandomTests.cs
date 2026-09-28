using System;
using Diciela.Simulation.Core;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le hasard par hachage. Les valeurs attendues viennent de l'implémentation de référence de SplitMix64 (Sebastiano
    /// Vigna), recalculées à part, en JavaScript avec des entiers de 64 bits : partie de zéro, la suite commence par
    /// E220A8397B1DCDAF, 6E789E6AA1B965F4, 06C45D188009454F, ses premières valeurs publiées.
    /// </summary>
    public sealed class HashRandomTests
    {
        [Test]
        public void FromZeroItIsTheReferenceSplitMix64()
        {
            var random = new HashRandom(0, 0);

            Assert.AreEqual(0xE220A8397B1DCDAFUL, random.NextBits());
            Assert.AreEqual(0x6E789E6AA1B965F4UL, random.NextBits());
            Assert.AreEqual(0x06C45D188009454FUL, random.NextBits());
        }

        [Test]
        public void TwoIntegersGiveTheirOwnSuite()
        {
            // Le départ : les deux entiers côte à côte en 64 bits (a en haut), brassés une fois.
            var one = new HashRandom(1, 0);
            Assert.AreEqual(0xCE30761CD7373F6DUL, one.NextBits());
            Assert.AreEqual(0xA82740738736E4C9UL, one.NextBits());

            Assert.AreEqual(0xEEA602B88A8A7DB1UL, new HashRandom(7, 123456).NextBits());
            Assert.AreEqual(0xA577782BC52A9F5AUL, new HashRandom(-1, -1).NextBits());

            // Mis à l'échelle de 13 valeurs : les 32 bits du haut de EEA602B8… fois 13, décalés de 32 bits.
            Assert.AreEqual(12, new HashRandom(7, 123456).Range(13));
        }

        [Test]
        public void RangeCoversItsValuesEvenly()
        {
            // 60 000 tirages sur 10 valeurs : 6 000 attendus pour chacune, à 4 écarts types près (± 300).
            var counts = new int[10];
            for (int seed = 0; seed < 60000; seed++)
            {
                var random = new HashRandom(seed, 42);
                counts[random.Range(10)]++;
                double unit = random.Unit();
                Assert.IsTrue(unit >= 0.0 && unit < 1.0, "Un nombre de 0 à 1 exclu : " + unit);
            }
            for (int value = 0; value < 10; value++)
                Assert.AreEqual(6000, counts[value], 300, "La valeur " + value + " sort " + counts[value] + " fois.");
        }

        [Test]
        public void RefusesARangeWithoutValues()
        {
            var random = new HashRandom(3, 4);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.Range(0));
        }
    }
}
