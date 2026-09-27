using System;
using Diciela.Simulation.Core;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>La conversion mètres ↔ mailles, à deux mailles par mètre (une maille fait 0,5 m).</summary>
    public sealed class GridMetricsTests
    {
        [Test]
        public void ConvertsMetersToCellsAndBack()
        {
            var metrics = new GridMetrics(2);

            Assert.AreEqual(0.5f, metrics.CellSize, 1e-6f);
            Assert.AreEqual(0, metrics.CellOf(0.49f));
            Assert.AreEqual(1, metrics.CellOf(0.5f));
            Assert.AreEqual(-1, metrics.CellOf(-0.1f));
            Assert.AreEqual(0.75f, metrics.CellCenter(1), 1e-6f);
            Assert.AreEqual(40, metrics.CellCount(20f));
            Assert.AreEqual(41, metrics.CellCount(20.1f));
        }

        [Test]
        public void RefusesLessThanOneCellPerMeter()
        {
            Assert.Throws<ArgumentException>(() => new GridMetrics(0));
        }
    }
}
