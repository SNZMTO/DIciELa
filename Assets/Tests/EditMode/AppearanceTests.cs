using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'apparence (GDD, section 6) : un corps, sa couleur, au plus une pièce par emplacement ; une apparence
    /// incohérente ne se construit pas. La Chimère naît avec celle de son ordre, et la garde. Le catalogue propose
    /// l'apparence de départ : son premier corps, dans sa couleur d'origine, sans pièce.
    /// </summary>
    public sealed class AppearanceTests
    {
        private TestChimera _test;
        private PaletteDefinition _palette;
        private BodyDefinition _body;
        private AttachmentSlotDefinition _head;

        [SetUp]
        public void CreateData()
        {
            _test = new TestChimera();
            _palette = _test.Keep(TestData.Palette("gris"));
            _body = _test.Keep(TestData.Body("renard", _palette));
            _head = _test.Keep(TestData.Slot("tete"));
        }

        [TearDown]
        public void DestroyData()
        {
            _test.Dispose();
        }

        [Test]
        public void AnAppearanceNeedsABodyAColorAndAListOfPieces()
        {
            var none = Array.Empty<AttachmentDefinition>();
            Assert.Throws<ArgumentNullException>(() => new Appearance(null, _palette, none));
            Assert.Throws<ArgumentNullException>(() => new Appearance(_body, null, none));
            Assert.Throws<ArgumentNullException>(() => new Appearance(_body, _palette, null));
        }

        [Test]
        public void AnAppearanceRefusesAMissingPieceAPieceWithoutSlotAndTwoPiecesInOneSlot()
        {
            AttachmentDefinition hat = _test.Keep(TestData.Attachment("chapeau", _head));
            AttachmentDefinition crown = _test.Keep(TestData.Attachment("couronne", _head));
            AttachmentDefinition loose = _test.Keep(TestData.Attachment("perdue", null));

            Assert.Throws<ArgumentException>(() => new Appearance(_body, _palette, new AttachmentDefinition[] { null }));
            Assert.Throws<ArgumentException>(() => new Appearance(_body, _palette, new[] { loose }));
            Assert.Throws<ArgumentException>(() => new Appearance(_body, _palette, new[] { hat, crown }));
        }

        [Test]
        public void AnAppearanceKeepsItsPiecesWhateverBecomesOfTheListItCameFrom()
        {
            AttachmentDefinition hat = _test.Keep(TestData.Attachment("chapeau", _head));
            AttachmentDefinition tail = _test.Keep(TestData.Attachment("queue", _test.Keep(TestData.Slot("queue"))));
            var pieces = new List<AttachmentDefinition> { hat, tail };

            var appearance = new Appearance(_body, _palette, pieces);
            pieces.Clear();

            Assert.AreSame(_body, appearance.Body);
            Assert.AreSame(_palette, appearance.Palette);
            CollectionAssert.AreEqual(new[] { hat, tail }, appearance.Attachments);
        }

        [Test]
        public void TheChimeraIsBornWithTheAppearanceOfHerOrderAndKeepsIt()
        {
            PaletteDefinition green = _test.Keep(TestData.Palette("vert"));
            var chosen = new Appearance(_body, green, new[] { _test.Keep(TestData.Attachment("chapeau", _head)) });
            var world = new World(_test.Settings, TestData.FlatMap(40f));

            world.Submit(new CreateChimeraCommand(_test.Kind, chosen));
            world.Submit(new CreateChimeraCommand(_test.Kind, _test.Appearance));
            world.Step();

            Assert.AreSame(chosen, world.Villagers.Chimera.Appearance, "Le second ordre est refusé : elle garde la sienne.");
        }

        [Test]
        public void TheStartingAppearanceIsTheFirstBodyInItsOwnColorWithoutPieces()
        {
            PaletteDefinition brown = _test.Keep(TestData.Palette("brun"));
            BodyDefinition second = _test.Keep(TestData.Body("chat", brown));

            Appearance starting = Appearance.StartingIn(_test.Keep(TestData.Catalog(_body, second)));

            Assert.AreSame(_body, starting.Body);
            Assert.AreSame(_palette, starting.Palette);
            Assert.IsEmpty(starting.Attachments);
            Assert.Throws<ArgumentException>(() => Appearance.StartingIn(_test.Keep(TestData.Catalog())));
        }
    }
}
