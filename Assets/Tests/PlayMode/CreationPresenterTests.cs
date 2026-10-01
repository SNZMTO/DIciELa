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
    /// qu'on lui montre et joue les flèches du joueur. Un catalogue de deux têtes et deux corps (le chat, roux ; le
    /// renard, gris), montés sur le personnage de TestRig ; deux couleurs ; un emplacement, le chapeau, qui propose un
    /// chapeau et une couronne, gris d'origine ; une table des textes écrite ici.
    /// </summary>
    public sealed class CreationPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "creation.titre,La Chimère\n" +
                                     "creation.tete,Tête\n" +
                                     "creation.corps,Corps\n" +
                                     "creation.aucun,Aucun\n" +
                                     "creation.precedent,<\n" +
                                     "creation.suivant,>\n" +
                                     "creation.valider,C'est parti\n" +
                                     "tete.chat,Chat\n" +
                                     "tete.renard,Renard\n" +
                                     "corps.chat,Chat\n" +
                                     "corps.renard,Renard\n" +
                                     "couleur.roux,Roux\n" +
                                     "couleur.gris,Gris\n" +
                                     "emplacement.chapeau,Chapeau\n" +
                                     "piece.chapeau,Chapeau\n" +
                                     "piece.couronne,Couronne\n";

        private TestWorld _test;
        private TestRig _rig;
        private HeadDefinition _foxHead;
        private BodyDefinition _catBody;
        private PaletteDefinition _red;
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
            _rig = new TestRig();
            var kind = _rig.Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            _red = Fiche<PaletteDefinition>("roux", "couleur.roux");
            _grey = Fiche<PaletteDefinition>("gris", "couleur.gris");
            var catHead = Fiche<HeadDefinition>("chat", "tete.chat");
            _foxHead = Fiche<HeadDefinition>("renard", "tete.renard");
            _catBody = Fiche<BodyDefinition>("chat", "corps.chat");
            var foxBody = Fiche<BodyDefinition>("renard", "corps.renard");
            var top = Fiche<AttachmentSlotDefinition>("chapeau", "emplacement.chapeau");
            _hat = Fiche<AttachmentDefinition>("chapeau", "piece.chapeau");
            var crown = Fiche<AttachmentDefinition>("couronne", "piece.couronne");
            var catalog = _rig.Keep(ScriptableObject.CreateInstance<Catalog>());
#if UNITY_EDITOR
            TestWorld.Fill(kind, "_id", "chimere");
            foreach ((HeadDefinition head, PaletteDefinition palette) in new[] { (catHead, _red), (_foxHead, _grey) })
            {
                TestWorld.Fill(head, "_mesh", _rig.Head.Mesh);
                TestWorld.Fill(head, "_palette", palette);
            }
            foreach ((BodyDefinition body, PaletteDefinition palette) in new[] { (_catBody, _red), (foxBody, _grey) })
            {
                TestWorld.Fill(body, "_model", _rig.Character);
                TestWorld.Fill(body, "_mesh", _rig.Body.Mesh);
                TestWorld.Fill(body, "_palette", palette);
            }
            TestWorld.Fill(top, "_bone", "head");
            foreach (AttachmentDefinition piece in new[] { _hat, crown })
            {
                TestWorld.Fill(piece, "_slot", top);
                TestWorld.Fill(piece, "_model", _rig.Character.transform.Find("root/pelvis/Queue d'office").gameObject);
                TestWorld.Fill(piece, "_palette", _grey);
                TestWorld.Fill(piece, "_scale", Vector3.one);
            }
            TestWorld.Fill(catalog, "_chimeraKind", kind);
            TestWorld.FillList(catalog, "_heads", catHead, _foxHead);
            TestWorld.FillList(catalog, "_bodies", _catBody, foxBody);
            TestWorld.FillList(catalog, "_palettes", _red, _grey);
            TestWorld.FillList(catalog, "_attachmentSlots", top);
            TestWorld.FillList(catalog, "_attachments", _hat, crown);
#endif
            _preview = _rig.Keep(new GameObject("Aperçu de test")).AddComponent<AppearancePreview>();
            _view = new NotedCreationView();
            _started = 0;
            _presenter = new CreationPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), _preview, _test.World,
                () => _started++);
        }

        [TearDown]
        public void DestroyScreen()
        {
            _presenter.Dispose();
            _rig.Dispose();
            _test.Dispose();
        }

        [Test]
        public void TheScreenStartsFromTheFirstHeadAndBodyInTheirColorsWithoutPieces()
        {
            Assert.AreEqual("La Chimère", _view.Title);
            Assert.AreEqual(new[] { "Tête", "Corps", "Chapeau" }, _view.Labels);
            Assert.AreEqual(new[] { "tete", "corps", "chapeau" }, _view.RowIds, "L'identifiant de chaque ligne choisit son icône.");
            Assert.AreEqual(new[] { "Chat", "Chat", "Aucun" }, _view.Choices);
            Assert.AreEqual(new[] { "Roux", "Roux", "" }, _view.Colors, "Sans pièce, pas de couleur.");
            Assert.AreEqual(1, _preview.transform.childCount, "L'aperçu montre la Chimère à venir.");
        }

        [Test]
        public void ThePieceArrowsGoRoundWithNoneFirstAndBringThePieceInItsOwnColor()
        {
            _view.Step(2, 1);
            Assert.AreEqual(("Chapeau", "Gris"), (_view.Choices[2], _view.Colors[2]));
            _view.Recolor(2, 1);
            Assert.AreEqual("Roux", _view.Colors[2]);
            _view.Step(2, 1);
            Assert.AreEqual(("Couronne", "Gris"), (_view.Choices[2], _view.Colors[2]), "Une autre pièce, sa couleur.");
            _view.Step(2, 1);
            Assert.AreEqual(("Aucun", ""), (_view.Choices[2], _view.Colors[2]), "Après la dernière, « aucun ».");
            _view.Recolor(2, 1);
            Assert.AreEqual("", _view.Colors[2], "Sans pièce, rien à colorer.");
            _view.Step(2, -1);
            Assert.AreEqual("Couronne", _view.Choices[2], "Avant « aucun », la dernière pièce.");
        }

        [Test]
        public void TurnedSheStaysTurnedWhenTheChoiceChanges()
        {
            _preview.Turn(90f);
            _view.Step(2, 1);
            Transform shown = _preview.transform.GetChild(_preview.transform.childCount - 1);
            Assert.AreEqual(90f, shown.eulerAngles.y, 0.01f, "Coiffée d'un chapeau, elle garde son angle.");
        }

        [Test]
        public void TheHeadAndTheBodyTakeTheirColorsApartAndAnotherHeadComesInItsOwn()
        {
            _view.Recolor(0, -1);
            Assert.AreEqual(new[] { "Gris", "Roux" }, _view.Colors.GetRange(0, 2), "La tête change seule de couleur.");
            _view.Step(0, 1);
            Assert.AreEqual(("Renard", "Gris"), (_view.Choices[0], _view.Colors[0]));
            _view.Step(1, 1);
            Assert.AreEqual(("Renard", "Gris"), (_view.Choices[1], _view.Colors[1]));
            _view.Step(0, 1);
            Assert.AreEqual(("Chat", "Roux"), (_view.Choices[0], _view.Colors[0]), "La tête du chat revient rousse.");
        }

        [Test]
        public void ValidatingSendsOneOrderWithTheChosenAppearanceThenTheScreenIsDone()
        {
            _view.Step(0, 1);
            _view.Recolor(1, 1);
            _view.Step(2, 1);
            _view.Recolor(2, 1);
            _view.Confirm();
            _view.Confirm();
            _view.Step(2, 1);
            _test.World.Step();

            Assert.AreEqual(1, _started, "Une seule validation.");
            Appearance appearance = _test.World.Villagers.Chimera.Appearance;
            Assert.AreSame(_foxHead, appearance.Head);
            Assert.AreSame(_grey, appearance.HeadPalette);
            Assert.AreSame(_catBody, appearance.Body);
            Assert.AreSame(_grey, appearance.BodyPalette);
            Assert.AreEqual(1, appearance.Attachments.Count);
            Assert.AreSame(_hat, appearance.Attachments[0].Attachment);
            Assert.AreSame(_red, appearance.Attachments[0].Palette);
            Assert.AreEqual("Chapeau", _view.Choices[2], "Validé, l'écran ne change plus.");
        }

        private T Fiche<T>(string id, string nameKey) where T : ScriptableObject
        {
            var fiche = _rig.Keep(ScriptableObject.CreateInstance<T>());
#if UNITY_EDITOR
            TestWorld.Fill(fiche, "_id", id);
            TestWorld.Fill(fiche, "_nameKey", nameKey);
#endif
            return fiche;
        }
    }
}
