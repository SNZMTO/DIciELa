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
    /// change, sauf sur une ligne où le joueur a choisi une autre pièce ; une tête qui tient une ligne soudée à son crâne
    /// n'y propose rien. Trois têtes sur le corps du chat : le chat (ses oreilles), le renard (les siennes), le raton
    /// laveur (oreilles soudées) ; une ligne Oreilles qui propose les deux paires et des oreilles de gobelin.
    /// </summary>
    public sealed class CreationHeadPiecesTests
    {
        private const string Texts = "clé,fr\n" +
                                     "creation.titre,La Chimère\n" +
                                     "creation.tete,Tête\n" +
                                     "creation.corps,Corps\n" +
                                     "creation.aucun,Aucun\n" +
                                     "creation.soudees,Celles de la tête\n" +
                                     "creation.precedent,<\n" +
                                     "creation.suivant,>\n" +
                                     "creation.valider,C'est parti\n" +
                                     "tete.chat,Chat\n" +
                                     "tete.renard,Renard\n" +
                                     "tete.raton,Raton\n" +
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

        [SetUp]
        public void CreateScreen()
        {
            _test = new TestWorld();
            _rig = new TestRig();
            var kind = _rig.Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            var red = Fiche<PaletteDefinition>("roux", "couleur.roux");
            var grey = Fiche<PaletteDefinition>("gris", "couleur.gris");
            var ears = Fiche<AttachmentSlotDefinition>("oreilles", "emplacement.oreilles");
            var catEars = Fiche<AttachmentDefinition>("chat", "piece.chat");
            var foxEars = Fiche<AttachmentDefinition>("renard", "piece.renard");
            var goblinEars = Fiche<AttachmentDefinition>("gobelin", "piece.gobelin");
            var cat = Fiche<HeadDefinition>("chat", "tete.chat");
            var fox = Fiche<HeadDefinition>("renard", "tete.renard");
            var raccoon = Fiche<HeadDefinition>("raton", "tete.raton");
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
            foreach ((HeadDefinition head, PaletteDefinition palette) in new[] { (cat, red), (fox, grey), (raccoon, grey) })
            {
                TestWorld.Fill(head, "_mesh", _rig.Head.Mesh);
                TestWorld.Fill(head, "_palette", palette);
            }
            TestWorld.FillList(cat, "_worn", catEars);
            TestWorld.FillList(fox, "_worn", foxEars);
            TestWorld.FillList(raccoon, "_fused", ears);
            TestWorld.Fill(body, "_model", _rig.Character);
            TestWorld.Fill(body, "_mesh", _rig.Body.Mesh);
            TestWorld.Fill(body, "_palette", red);
            TestWorld.Fill(catalog, "_chimeraKind", kind);
            TestWorld.FillList(catalog, "_heads", cat, fox, raccoon);
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
        public void AHeadThatHoldsItsEarsFusedOffersNothingOnThatRowAndWearsNone()
        {
            _view.Step(Head, -1);
            Assert.AreEqual(("Celles de la tête", ""), (_view.Choices[Ears], _view.Colors[Ears]));
            _view.Step(Ears, 1);
            _view.Recolor(Ears, 1);
            Assert.AreEqual(("Celles de la tête", ""), (_view.Choices[Ears], _view.Colors[Ears]),
                "Les flèches n'y font rien.");
            _view.Confirm();
            _test.World.Step();

            Assert.IsEmpty(_test.World.Villagers.Chimera.Appearance.Attachments, "Aucune paire ne s'ajoute aux siennes.");
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
