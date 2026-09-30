using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// L'apparence (GDD, section 6) : une tête et un corps, chacun de sa couleur, au plus une pièce par emplacement,
    /// chacune de la sienne ; une apparence incohérente ne se construit pas. La Chimère naît avec celle de son ordre, et la
    /// garde. Le catalogue propose l'apparence de départ : sa première tête et son premier corps, dans leurs couleurs
    /// d'origine, et les pièces que ce corps porte dans le pack, de sa couleur.
    /// </summary>
    public sealed class AppearanceTests
    {
        private TestChimera _test;
        private PaletteDefinition _grey;
        private HeadDefinition _head;
        private BodyDefinition _body;
        private AttachmentSlotDefinition _top;

        [SetUp]
        public void CreateData()
        {
            _test = new TestChimera();
            _grey = _test.Keep(TestData.Palette("gris"));
            _head = _test.Keep(TestData.Head("renard", _grey));
            _body = _test.Keep(TestData.Body("renard", _grey));
            _top = _test.Keep(TestData.Slot("chapeau"));
        }

        [TearDown]
        public void DestroyData()
        {
            _test.Dispose();
        }

        [Test]
        public void AnAppearanceNeedsAHeadABodyTheirColorsAndAListOfPieces()
        {
            var none = Array.Empty<WornAttachment>();
            Assert.Throws<ArgumentNullException>(() => new Appearance(null, _grey, _body, _grey, none));
            Assert.Throws<ArgumentNullException>(() => new Appearance(_head, null, _body, _grey, none));
            Assert.Throws<ArgumentNullException>(() => new Appearance(_head, _grey, null, _grey, none));
            Assert.Throws<ArgumentNullException>(() => new Appearance(_head, _grey, _body, null, none));
            Assert.Throws<ArgumentNullException>(() => new Appearance(_head, _grey, _body, _grey, null));
        }

        [Test]
        public void AnAppearanceRefusesAPieceMissingItsSlotOrItsColorAndTwoPiecesInOneSlot()
        {
            AttachmentDefinition hat = _test.Keep(TestData.Attachment("chapeau", _top));
            AttachmentDefinition crown = _test.Keep(TestData.Attachment("couronne", _top));
            AttachmentDefinition loose = _test.Keep(TestData.Attachment("perdue", null));

            Assert.Throws<ArgumentException>(() => Wearing(new WornAttachment(null, _grey)));
            Assert.Throws<ArgumentException>(() => Wearing(new WornAttachment(loose, _grey)));
            Assert.Throws<ArgumentException>(() => Wearing(new WornAttachment(hat, null)));
            Assert.Throws<ArgumentException>(() => Wearing(new WornAttachment(hat, _grey),
                new WornAttachment(crown, _grey)));
        }

        [Test]
        public void AnAppearanceKeepsItsPiecesWhateverBecomesOfTheListItCameFrom()
        {
            PaletteDefinition red = _test.Keep(TestData.Palette("roux"));
            AttachmentDefinition hat = _test.Keep(TestData.Attachment("chapeau", _top));
            AttachmentDefinition tail = _test.Keep(TestData.Attachment("queue", _test.Keep(TestData.Slot("queue"))));
            var pieces = new List<WornAttachment> { new WornAttachment(hat, red), new WornAttachment(tail, _grey) };

            var appearance = new Appearance(_head, red, _body, _grey, pieces);
            pieces.Clear();

            Assert.AreSame(_head, appearance.Head);
            Assert.AreSame(red, appearance.HeadPalette);
            Assert.AreSame(_body, appearance.Body);
            Assert.AreSame(_grey, appearance.BodyPalette);
            Assert.AreEqual(2, appearance.Attachments.Count);
            Assert.AreSame(hat, appearance.Attachments[0].Attachment);
            Assert.AreSame(red, appearance.Attachments[0].Palette);
            Assert.AreSame(tail, appearance.Attachments[1].Attachment);
        }

        [Test]
        public void TheChimeraIsBornWithTheAppearanceOfHerOrderAndKeepsIt()
        {
            PaletteDefinition green = _test.Keep(TestData.Palette("vert"));
            Appearance chosen = Wearing(new WornAttachment(_test.Keep(TestData.Attachment("chapeau", _top)), green));
            var world = new World(_test.Settings, TestData.FlatMap(40f), TestData.EmptyCatalog);

            world.Submit(new CreateChimeraCommand(_test.Kind, chosen));
            world.Submit(new CreateChimeraCommand(_test.Kind, _test.Appearance));
            world.Step();

            Assert.AreSame(chosen, world.Villagers.Chimera.Appearance, "Le second ordre est refusé : elle garde la sienne.");
        }

        [Test]
        public void TheStartingAppearanceIsTheFirstHeadAndBodyInTheirColorsWearingWhatEachWearsInThePack()
        {
            PaletteDefinition red = _test.Keep(TestData.Palette("roux"));
            AttachmentDefinition tail = _test.Keep(TestData.Attachment("queue", _test.Keep(TestData.Slot("queue"))));
            AttachmentDefinition ears = _test.Keep(TestData.Attachment("oreilles", _test.Keep(TestData.Slot("oreilles"))));
            HeadDefinition cat = _test.Keep(TestData.Head("chat", red, ears));
            BodyDefinition dress = _test.Keep(TestData.Body("chat", _grey, tail));

            Appearance starting = Appearance.StartingIn(_test.Keep(TestData.Catalog(new[] { cat, _head },
                new[] { dress, _body })));

            Assert.AreSame(cat, starting.Head);
            Assert.AreSame(red, starting.HeadPalette);
            Assert.AreSame(dress, starting.Body);
            Assert.AreSame(_grey, starting.BodyPalette);
            Assert.AreEqual(2, starting.Attachments.Count);
            Assert.AreSame(ears, starting.Attachments[0].Attachment);
            Assert.AreSame(red, starting.Attachments[0].Palette, "Les oreilles du chat sont de la couleur de sa tête.");
            Assert.AreSame(tail, starting.Attachments[1].Attachment);
            Assert.AreSame(_grey, starting.Attachments[1].Palette, "La queue du chat est de la couleur de son corps.");
            Assert.Throws<ArgumentException>(() => Appearance.StartingIn(_test.Keep(TestData.Catalog(
                Array.Empty<HeadDefinition>(), new[] { _body }))));
        }

        private Appearance Wearing(params WornAttachment[] pieces)
        {
            return new Appearance(_head, _grey, _body, _grey, pieces);
        }
    }
}
