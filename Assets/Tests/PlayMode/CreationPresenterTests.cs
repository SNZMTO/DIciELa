using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le présentateur de l'écran de création (GDD, section 6), sur le petit monde des tests et une vue qui note ce
    /// qu'on lui montre et joue les flèches du joueur. Un catalogue de deux corps (le chat, roux ; le renard, gris), de
    /// deux couleurs et d'un emplacement, la tête, qui propose un chapeau et une couronne ; une table des textes écrite
    /// ici.
    /// </summary>
    public sealed class CreationPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "creation.titre,La Chimère\n" +
                                     "creation.corps,Corps\n" +
                                     "creation.couleur,Couleur\n" +
                                     "creation.aucun,Aucun\n" +
                                     "creation.precedent,‹\n" +
                                     "creation.suivant,›\n" +
                                     "creation.valider,C'est parti\n" +
                                     "corps.chat,Chat\n" +
                                     "corps.renard,Renard\n" +
                                     "couleur.roux,Roux\n" +
                                     "couleur.gris,Gris\n" +
                                     "emplacement.tete,Tête\n" +
                                     "piece.chapeau,Chapeau\n" +
                                     "piece.couronne,Couronne\n";

        private TestWorld _test;
        private BodyDefinition _fox;
        private PaletteDefinition _grey;
        private AttachmentDefinition _hat;
        private AppearancePreview _preview;
        private NotedCreationView _view;
        private CreationPresenter _presenter;
        private int _started;

        [SetUp]
        public void CreateScreen()
        {
            _test = new TestWorld();
            var kind = _test.Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            GameObject character = _test.Keep(new GameObject("Personnage de test"));
            new GameObject("head").transform.SetParent(character.transform, false);
            var red = Fiche<PaletteDefinition>("roux", "couleur.roux");
            _grey = Fiche<PaletteDefinition>("gris", "couleur.gris");
            var cat = Fiche<BodyDefinition>("chat", "corps.chat");
            _fox = Fiche<BodyDefinition>("renard", "corps.renard");
            var head = Fiche<AttachmentSlotDefinition>("tete", "emplacement.tete");
            _hat = Fiche<AttachmentDefinition>("chapeau", "piece.chapeau");
            var crown = Fiche<AttachmentDefinition>("couronne", "piece.couronne");
            var catalog = _test.Keep(ScriptableObject.CreateInstance<Catalog>());
#if UNITY_EDITOR
            TestWorld.Fill(kind, "_id", "chimere");
            foreach ((BodyDefinition body, PaletteDefinition palette) in new[] { (cat, red), (_fox, _grey) })
            {
                TestWorld.Fill(body, "_model", character);
                TestWorld.Fill(body, "_palette", palette);
            }
            TestWorld.Fill(head, "_bone", "head");
            foreach (AttachmentDefinition piece in new[] { _hat, crown })
            {
                TestWorld.Fill(piece, "_slot", head);
                TestWorld.Fill(piece, "_model", character);
            }
            TestWorld.Fill(catalog, "_chimeraKind", kind);
            TestWorld.FillList(catalog, "_bodies", cat, _fox);
            TestWorld.FillList(catalog, "_palettes", red, _grey);
            TestWorld.FillList(catalog, "_attachmentSlots", head);
            TestWorld.FillList(catalog, "_attachments", _hat, crown);
#endif
            _preview = _test.Keep(new GameObject("Aperçu de test")).AddComponent<AppearancePreview>();
            _view = new NotedCreationView();
            _started = 0;
            _presenter = new CreationPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), _preview, _test.World,
                () => _started++);
        }

        [TearDown]
        public void DestroyScreen()
        {
            _presenter.Dispose();
            _test.Dispose();
        }

        [Test]
        public void TheScreenStartsFromTheFirstBodyInItsOwnColorWithoutPieces()
        {
            Assert.AreEqual("La Chimère", _view.Title);
            Assert.AreEqual(new[] { "Corps", "Couleur", "Tête" }, _view.Labels);
            Assert.AreEqual(new[] { "Chat", "Roux", "Aucun" }, _view.Choices);
            Assert.AreEqual(1, _preview.transform.childCount, "L'aperçu montre la Chimère à venir.");
        }

        [Test]
        public void TheArrowsGoRoundEachRowWithNoneBeforeThePieces()
        {
            _view.Step(2, 1);
            Assert.AreEqual("Chapeau", _view.Choices[2]);
            _view.Step(2, 1);
            Assert.AreEqual("Couronne", _view.Choices[2]);
            _view.Step(2, 1);
            Assert.AreEqual("Aucun", _view.Choices[2], "Après la dernière pièce, on revient à « aucun ».");
            _view.Step(2, -1);
            Assert.AreEqual("Couronne", _view.Choices[2], "Avant « aucun », la dernière pièce.");
            _view.Step(1, -1);
            Assert.AreEqual("Gris", _view.Choices[1], "Avant la première couleur, la dernière.");
        }

        [Test]
        public void AnotherBodyComesInItsOwnColor()
        {
            _view.Step(1, 1);
            Assert.AreEqual(new[] { "Chat", "Gris" }, _view.Choices.GetRange(0, 2));
            _view.Step(0, 1);
            Assert.AreEqual(new[] { "Renard", "Gris" }, _view.Choices.GetRange(0, 2));
            _view.Step(0, 1);
            Assert.AreEqual(new[] { "Chat", "Roux" }, _view.Choices.GetRange(0, 2), "Le chat revient roux.");
        }

        [Test]
        public void ValidatingSendsOneOrderWithTheChosenAppearanceThenTheScreenIsDone()
        {
            _view.Step(0, 1);
            _view.Step(2, 1);
            _view.Confirm();
            _view.Confirm();
            _view.Step(2, 1);
            _test.World.Step();

            Assert.AreEqual(1, _started, "Une seule validation.");
            Appearance appearance = _test.World.Villagers.Chimera.Appearance;
            Assert.AreSame(_fox, appearance.Body);
            Assert.AreSame(_grey, appearance.Palette);
            CollectionAssert.AreEqual(new[] { _hat }, appearance.Attachments);
            Assert.AreEqual("Chapeau", _view.Choices[2], "Validé, l'écran ne change plus.");
        }

        private T Fiche<T>(string id, string nameKey) where T : ScriptableObject
        {
            var fiche = _test.Keep(ScriptableObject.CreateInstance<T>());
#if UNITY_EDITOR
            TestWorld.Fill(fiche, "_id", id);
            TestWorld.Fill(fiche, "_nameKey", nameKey);
#endif
            return fiche;
        }
    }
}
