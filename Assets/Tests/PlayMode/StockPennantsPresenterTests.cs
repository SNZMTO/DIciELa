using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les fanions du stock et leur présentateur (GDD, section 16 ; décision 153), sur une vue qui note : rien tant que
    /// la ville n'a pas d'entrepôt ; le campement installé, un fanion par marchandise du catalogue (le Bois, 30 ; la Pierre,
    /// 20), chacun avec son infobulle ; les nombres suivent le stock, jusqu'à zéro, où le fanion reste.
    /// </summary>
    public sealed class StockPennantsPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "marchandise.bois,Bois\n" +
                                     "marchandise.pierre,Pierre\n" +
                                     "fanion.infobulle,\"{marchandise}, dans le stock de la ville\"\n";

        private sealed class NotedView : IStockPennantsView
        {
            public bool Shown { get; private set; }
            public List<StockPennant> Pennants { get; } = new List<StockPennant>();

            public void ShowStock(bool shown)
            {
                Shown = shown;
            }

            public void ShowPennants(IReadOnlyList<StockPennant> pennants)
            {
                Pennants.Clear();
                Pennants.AddRange(pennants);
            }
        }

        private TestCampTown _town;
        private NotedView _view;
        private StockPennantsPresenter _presenter;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestCampTown();
            _view = new NotedView();
            _presenter = new StockPennantsPresenter(_view, _town.World, _town.Catalog, TextTable.Parse(Texts, "fr"));
        }

        [TearDown]
        public void DestroyTown()
        {
            _presenter.Dispose();
            _town.Dispose();
        }

        [Test]
        public void ThePennantsComeWithThePile()
        {
            Assert.IsFalse(_view.Shown, "Sans entrepôt, pas de stock à montrer.");

            _town.InstallTheCamp();

            Assert.IsTrue(_view.Shown);
            Assert.AreEqual(2, _view.Pennants.Count);
            Assert.AreEqual(30, _view.Pennants[0].Count);
            Assert.AreEqual(20, _view.Pennants[1].Count);
            Assert.AreEqual("Bois, dans le stock de la ville", _view.Pennants[0].Tooltip);
        }

        [Test]
        public void ThePennantsFollowTheStockDownToZero()
        {
            _town.InstallTheCamp();
            World world = _town.World;
            world.Submit(new AssignVillagerCommand(world.Villagers.Chimera.Id, _town.PlacedCamp.Id));
            world.Submit(new PlaceStructureCommand(_town.Cabin(new GoodAmount(_town.Stone, 20)),
                new Placement(new Vector2(28f, 20f), 0f)));
            for (int step = 0; step < 6000 && _view.Pennants[1].Count > 0; step++)
                world.Step();

            Assert.AreEqual(0, _view.Pennants[1].Count, "La Pierre est partie au chantier.");
            Assert.AreEqual(2, _view.Pennants.Count, "Le fanion à zéro reste.");
            Assert.AreEqual(30, _view.Pennants[0].Count);
        }
    }
}
