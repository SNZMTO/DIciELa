using Diciela.Controls;
using NUnit.Framework;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La main (décision 173), sur le petit monde des tests, par les méthodes mêmes des outils, sans la roue : un seul
    /// outil à la fois. Prendre l'outil « Retirer » lâche la construction en main ; prendre une construction pose l'outil
    /// « Retirer » ; changer de construction la garde en main ; sans outil, la main est vide ; libérée, elle ne lâche
    /// plus rien.
    /// </summary>
    public sealed class HandTests
    {
        private TestWorld _test;
        private Hand _hand;

        [SetUp]
        public void CreateHand()
        {
            _test = new TestWorld();
            _hand = new Hand(_test.Tool, _test.Clearing);
        }

        [TearDown]
        public void DestroyHand()
        {
            _hand.Dispose();
            _test.Dispose();
        }

        [Test]
        public void TakingTheAxeDropsTheStructureInHand()
        {
            _test.Tool.Begin(_test.Cabin);

            _test.Clearing.Take();

            Assert.IsTrue(_test.Clearing.IsActive);
            Assert.IsFalse(_test.Tool.IsActive, "La cabane est lâchée.");
            Assert.IsTrue(_hand.IsHolding);
        }

        [Test]
        public void TakingAStructurePutsTheAxeDown()
        {
            _test.Clearing.Take();

            _test.Tool.Begin(_test.Cabin);

            Assert.IsTrue(_test.Tool.IsActive);
            Assert.IsFalse(_test.Clearing.IsActive, "La hache est posée.");
        }

        [Test]
        public void ChangingTheStructureInHandKeepsIt()
        {
            _test.Tool.Begin(_test.Cabin);

            _test.Tool.Begin(_test.Camp);

            Assert.AreSame(_test.Camp, _test.Tool.Current);
        }

        [Test]
        public void AnEmptyHandHoldsNothing()
        {
            Assert.IsFalse(_hand.IsHolding);

            _test.Clearing.Take();
            _test.Clearing.PutDown();

            Assert.IsFalse(_hand.IsHolding);
        }

        [Test]
        public void EscapeGoesToTheToolInHand()
        {
            Assert.IsFalse(_hand.Escape(), "La main vide laisse Échap au cran suivant.");

            _test.Tool.Begin(_test.Cabin);
            Assert.IsTrue(_hand.Escape(), "La construction en main…");
            Assert.IsFalse(_test.Tool.IsActive, "… se pose de côté.");
            Assert.IsFalse(_hand.Escape(), "Puis la main est vide.");
        }

        [Test]
        public void ReleasedTheHandPutsNothingDown()
        {
            _hand.Dispose();
            _test.Tool.Begin(_test.Cabin);

            _test.Clearing.Take();

            Assert.IsTrue(_test.Tool.IsActive, "Plus personne ne lâche la cabane.");
        }
    }
}
