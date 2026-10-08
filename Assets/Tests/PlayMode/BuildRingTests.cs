using System.Linq;
using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La roue de Construire (GDD, section 16 ; décision 226), par le présentateur de la roue, comme sa vue l'appelle,
    /// sur le petit monde des tests : deux familles, le Logement (le campement, la cabane, la maison) et le Décor (le
    /// banc). Le campement d'abord : Construire le met en main, et sa roue ne montre que lui ; posé, il s'en va, et le
    /// reste paraît, chaque famille à sa place (la première en haut, la deuxième à droite), la case Outils en bas à
    /// gauche. La bulle dit le nom d'une construction, ses lits et son coût ; prendre une construction la met en main,
    /// prendre sa famille la garde ; le même chiffre deux fois passe à la construction suivante de la famille ;
    /// Construire reprend la dernière construction prise. Une vue qui note, des touches et une table des textes écrites
    /// ici.
    /// </summary>
    public sealed class BuildRingTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "famille.decor,Décor\n" +
                                     "construction.campement,Campement\n" +
                                     "construction.cabane,Cabane\n" +
                                     "construction.maison,Maison\n" +
                                     "construction.banc,Banc\n" +
                                     "roue.famille.suite,Pousse plus loin\n" +
                                     "roue.outils,Outils\n" +
                                     "carte.lits,{lits} lits\n" +
                                     "construction.cout,{nombre} {marchandise}\n" +
                                     "liste.separateur,\", \"\n" +
                                     "marchandise.bois,Bois\n" +
                                     "marchandise.pierre,Pierre\n";

        private TestWorld _test;
        private StructureDefinition _house;
        private StructureDefinition _bench;
        private NotedToolWheelView _view;
        private ToolWheelPresenter _presenter;

        [SetUp]
        public void CreateWheel()
        {
            _test = new TestWorld(withPlaza: true);
            _house = _test.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            _bench = _test.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(_house, "_nameKey", "construction.maison");
            TestWorld.Fill(_bench, "_nameKey", "construction.banc");
#endif
            TestFamilies.Offer(_test.Cabin, 2, new GoodAmount(_test.Good("marchandise.bois"), 10),
                new GoodAmount(_test.Good("marchandise.pierre"), 5));
            _test.SetFamilies(_test.Family("famille.logement", Pastel.Rose, _test.Camp, _test.Cabin, _house),
                _test.Family("famille.decor", Pastel.Peche, _bench));
            _view = new NotedToolWheelView();
            _presenter = new ToolWheelPresenter(_view, TextTable.Parse(Texts, "fr"), new FixedToolKeys(), _test.Catalog,
                _test.World, _test.Tool, _test.Clearing, _test.Mover, _test.Paths, _test.Plaza);
        }

        [TearDown]
        public void DestroyWheel()
        {
            _test.Dispose();
        }

        [Test]
        public void BeforeTheCampTheWheelOffersOnlyTheCamp()
        {
            _presenter.Choose(ToolChoice.Build);
            Assert.AreSame(_test.Camp, _test.Tool.Current, "Construire prend la première construction proposée.");

            _presenter.Open(false);
            Assert.AreEqual(2, _view.Ring.Count, "Le Logement et la case Outils : le Décor attend le campement.");
            Assert.AreEqual("Logement", _view.At(0).Name, "La première famille, en haut…");
            Assert.AreEqual("rose", _view.At(0).Pastel, "… à son pastel…");
            Assert.IsTrue(_view.At(0).IsOn, "… enfoncée : la construction en main est d'elle.");
            Assert.AreEqual(new[] { "Campement" }, Names(_view.At(0)));
            Assert.AreEqual("Outils", _view.At(5).Name, "La case Outils, en bas à gauche.");
        }

        [Test]
        public void OncePlantedTheCampLeavesAndTheRestComes()
        {
            _test.PlantTheCamp(_presenter);
            Assert.IsFalse(_test.Tool.IsActive, "Posé, le campement quitte la main : il est unique.");

            _presenter.Choose(ToolChoice.Build);
            Assert.AreSame(_test.Cabin, _test.Tool.Current,
                "Le campement ne se pose plus : Construire prend la cabane.");
            _presenter.Open(false);
            Assert.AreEqual(new[] { "Cabane", "Maison" }, Names(_view.At(0)));
            Assert.AreEqual("Décor", _view.At(2).Name, "La deuxième famille, à droite.");
            Assert.AreEqual(new[] { "Banc" }, Names(_view.At(2)));
        }

        [Test]
        public void TheBubbleSaysTheNameTheBedsAndTheCost()
        {
            _test.PlantTheCamp(_presenter);
            _presenter.Choose(ToolChoice.Build);
            _presenter.Open(false);

            _presenter.Aim(_view.IndexAt(0), 0);
            Assert.AreEqual(("Cabane", "2 lits\n10 Bois, 5 Pierre"), _view.Label);
            _presenter.Aim(_view.IndexAt(0), -1);
            Assert.AreEqual(("Logement", "Pousse plus loin"), _view.Label);
            _presenter.Aim(_view.IndexAt(2), 0);
            Assert.AreEqual(("Banc", ""), _view.Label, "Le banc n'offre rien et ne coûte rien.");
        }

        [Test]
        public void AConstructionTakenIsInHandAndItsFamilyKeepsIt()
        {
            _test.PlantTheCamp(_presenter);
            _presenter.Choose(ToolChoice.Build);
            _presenter.Open(false);

            Assert.IsFalse(_presenter.Pick(_view.IndexAt(0), 1), "Prendre une construction ferme la roue.");
            Assert.AreSame(_house, _test.Tool.Current);
            _presenter.Open(false);
            Assert.IsTrue(_view.At(0).Options[1].IsOn, "Dans l'éventail, la construction en main est enfoncée.");
            _presenter.Pick(_view.IndexAt(0), -1);
            Assert.AreSame(_house, _test.Tool.Current, "Prendre sa famille la garde en main.");
            _presenter.Open(false);
            _presenter.Pick(_view.IndexAt(2), -1);
            Assert.AreSame(_bench, _test.Tool.Current, "Une autre famille met en main sa première construction.");

            _presenter.Open(false);
            Assert.IsTrue(_presenter.Pick(_view.IndexAt(5), -1), "La case Outils retourne la roue, ouverte…");
            Assert.AreEqual(5, _view.Ring.Count, "… sur les outils.");
        }

        [Test]
        public void TheSameDigitTwiceTakesTheNextConstructionOfTheFamily()
        {
            _test.PlantTheCamp(_presenter);
            _presenter.Choose(ToolChoice.Build);

            Assert.IsFalse(_presenter.TakeSlot(0));
            Assert.AreSame(_house, _test.Tool.Current, "La cabane en main, le 1 passe à la maison…");
            _presenter.TakeSlot(0);
            Assert.AreSame(_test.Cabin, _test.Tool.Current, "… puis revient à la cabane.");
            _presenter.TakeSlot(2);
            Assert.AreSame(_bench, _test.Tool.Current, "Le 3 prend le Décor, à sa place.");
            _presenter.TakeSlot(0);
            Assert.AreSame(_test.Cabin, _test.Tool.Current, "Le 1, d'une autre famille : sa première construction.");
            Assert.IsTrue(_presenter.TakeSlot(5), "Le 6, la case Outils, demande la roue des outils.");
        }

        [Test]
        public void BuildTakesBackTheLastConstructionAndASecondBuildDropsIt()
        {
            _test.PlantTheCamp(_presenter);
            _presenter.Choose(ToolChoice.Build);
            _presenter.TakeSlot(2);

            _presenter.Choose(ToolChoice.Build);
            Assert.IsFalse(_test.Tool.IsActive, "Un second Construire la lâche.");
            _presenter.Choose(ToolChoice.Build);
            Assert.AreSame(_bench, _test.Tool.Current, "Construire reprend la dernière construction prise.");
        }

        private static string[] Names(WheelSlot slot)
        {
            return slot.Options.Select(option => option.Name).ToArray();
        }
    }
}
