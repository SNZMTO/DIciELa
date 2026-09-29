using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les pièces qu'une tête porte d'office, à l'écran de création (GDD, section 6) : elles suivent la tête quand on en
    /// change, sauf sur une ligne où le joueur a choisi une autre pièce ; une tête qui n'en a pas laisse la ligne vide,
    /// et la suivante y remet les siennes. Trois têtes sur le corps du chat : le chat (ses oreilles), le renard (les
    /// siennes), la grenouille (sans oreilles) ; une ligne Oreilles qui propose les deux paires et des oreilles de gobelin.
    /// </summary>
    public sealed class CreationHeadPiecesTests
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
                                     "tete.grenouille,Grenouille\n" +
                                     "corps.chat,Chat\n" +
                                     "couleur.roux,Roux\n" +
                                     "couleur.gris,Gris\n" +
                                     "emplacement.oreilles,Oreilles\n" +
                                     "piece.chat,Oreilles de chat\n" +
                                     "piece.renard,Oreilles de renard\n" +
                                     "piece.gobelin,Oreilles de gobelin\n";

        private const int Head = 0;
        private const int Ears = 2;

        private TestWorld _test;
        private TestRig _rig;
        private NotedCreationView _view;
        private CreationPresenter _presenter;
        private AttachmentSlotDefinition _catalogEars;

        [SetUp]
        public void CreateScreen()
        {
            _test = new TestWorld();
            _rig = new TestRig();
            var kind = _rig.Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            var red = Fiche<PaletteDefinition>("roux", "couleur.roux");
            var grey = Fiche<PaletteDefinition>("gris", "couleur.gris");
            var ears = _catalogEars = Fiche<AttachmentSlotDefinition>("oreilles", "emplacement.oreilles");
            var catEars = Fiche<AttachmentDefinition>("chat", "piece.chat");
            var foxEars = Fiche<AttachmentDefinition>("renard", "piece.renard");
            var goblinEars = Fiche<AttachmentDefinition>("gobelin", "piece.gobelin");
            var cat = Fiche<HeadDefinition>("chat", "tete.chat");
            var fox = Fiche<HeadDefinition>("renard", "tete.renard");
            var frog = Fiche<HeadDefinition>("grenouille", "tete.grenouille");
            var body = Fiche<BodyDefinition>("chat", "corps.chat");
            var catalog = _rig.Keep(ScriptableObject.CreateInstance<Catalog>());
#if UNITY_EDITOR
            TestWorld.Fill(kind, "_id", "chimere");
            TestWorld.Fill(ears, "_bone", "head");
            foreach ((AttachmentDefinition piece, PaletteDefinition palette) in new[]
                         { (catEars, red), (foxEars, grey), (goblinEars, grey) })
            {
                TestWorld.Fill(piece, "_slot", ears);
                TestWorld.Fill(piece, "_model", _rig.Character.transform.Find("root/pelvis/Queue d'office").gameObject);
                TestWorld.Fill(piece, "_palette", palette);
                TestWorld.Fill(piece, "_scale", Vector3.one);
            }
            foreach ((HeadDefinition head, PaletteDefinition palette) in new[] { (cat, red), (fox, grey), (frog, grey) })
            {
                TestWorld.Fill(head, "_mesh", _rig.Head.Mesh);
                TestWorld.Fill(head, "_palette", palette);
            }
            TestWorld.FillList(cat, "_worn", catEars);
            TestWorld.FillList(fox, "_worn", foxEars);
            TestWorld.Fill(body, "_model", _rig.Character);
            TestWorld.Fill(body, "_mesh", _rig.Body.Mesh);
            TestWorld.Fill(body, "_palette", red);
            TestWorld.Fill(catalog, "_chimeraKind", kind);
            TestWorld.FillList(catalog, "_heads", cat, fox, frog);
            TestWorld.FillList(catalog, "_bodies", body);
            TestWorld.FillList(catalog, "_palettes", red, grey);
            TestWorld.FillList(catalog, "_attachmentSlots", ears);
            TestWorld.FillList(catalog, "_attachments", catEars, foxEars, goblinEars);
#endif
            var preview = _rig.Keep(new GameObject("Aperçu de test")).AddComponent<AppearancePreview>();
            _view = new NotedCreationView();
            _presenter = new CreationPresenter(_view, catalog, TextTable.Parse(Texts, "fr"), preview, _test.World,
                () => { });
        }

        [TearDown]
        public void DestroyScreen()
        {
            _presenter.Dispose();
            _rig.Dispose();
            _test.Dispose();
        }

        [Test]
        public void AnotherHeadBringsItsOwnEarsUnlessThePlayerChoseOthers()
        {
            Assert.AreEqual(("Oreilles de chat", "Roux"), (_view.Choices[Ears], _view.Colors[Ears]),
                "On part du chat, avec ses oreilles.");
            _view.Step(Head, 1);
            Assert.AreEqual(("Oreilles de renard", "Gris"), (_view.Choices[Ears], _view.Colors[Ears]),
                "La tête du renard vient avec les siennes, dans leur couleur.");
            _view.Step(Ears, 1);
            Assert.AreEqual("Oreilles de gobelin", _view.Choices[Ears]);
            _view.Step(Head, -1);
            Assert.AreEqual("Oreilles de gobelin", _view.Choices[Ears], "Des oreilles choisies restent.");
        }

        [Test]
        public void ARowThatCannotBeEmptyNeverOffersNone()
        {
#if UNITY_EDITOR
            TestWorld.Fill(_catalogEars, "_required", true);
#endif
            for (int step = 0; step < 4; step++)
            {
                _view.Step(Ears, 1);
                Assert.AreNotEqual("Aucun", _view.Choices[Ears], "Le museau, par exemple : jamais « aucun ».");
            }
            _view.Step(Head, 1);
            _view.Step(Head, 1);
            Assert.AreNotEqual("Aucun", _view.Choices[Ears], "Une tête qui n'en porte pas laisse la pièce montrée.");
        }

        [Test]
        public void AHeadWithoutEarsLeavesTheRowEmptyAndTheNextOneBringsItsOwn()
        {
            _view.Step(Head, -1);
            Assert.AreEqual(("Aucun", ""), (_view.Choices[Ears], _view.Colors[Ears]),
                "La grenouille n'a pas d'oreilles : celles du chat s'en vont.");
            _view.Step(Head, 1);
            Assert.AreEqual("Oreilles de chat", _view.Choices[Ears], "Le chat revient avec les siennes.");
            _view.Step(Head, -1);
            _view.Confirm();
            _test.World.Step();

            Assert.IsEmpty(_test.World.Villagers.Chimera.Appearance.Attachments, "Une grenouille sans oreilles.");
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
