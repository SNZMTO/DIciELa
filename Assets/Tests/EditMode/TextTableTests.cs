using System;
using Diciela.Data;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>La table des textes, sur un petit fichier écrit à la main (fins de ligne de Windows, une ligne vide).</summary>
    public sealed class TextTableTests
    {
        private const string Csv = "clé,fr,en\r\n" +
                                   "bonjour,Bonjour,Hello\r\n" +
                                   "\r\n" +
                                   "porte,\"Devant une porte, rien ; « jamais » \"\"non\"\"\",In front of a door\r\n" +
                                   "vide,,Empty\r\n";

        [Test]
        public void ReadsTheColumnOfItsLanguage()
        {
            TextTable english = TextTable.Parse(Csv, "en");

            Assert.AreEqual("Bonjour", TextTable.Parse(Csv, "fr").Get("bonjour"));
            Assert.AreEqual("Hello", english.Get("bonjour"));
            Assert.AreEqual("en", english.Language);
        }

        [Test]
        public void AQuotedFieldKeepsItsCommasAndQuotes()
        {
            Assert.AreEqual("Devant une porte, rien ; « jamais » \"non\"", TextTable.Parse(Csv, "fr").Get("porte"));
        }

        [Test]
        public void AMissingTextShowsItsKey()
        {
            TextTable french = TextTable.Parse(Csv, "fr");

            Assert.IsFalse(french.Has("vide"));
            Assert.AreEqual("[vide]", french.Get("vide"));
            Assert.AreEqual("[inconnue]", french.Get("inconnue"));
            Assert.AreEqual("Empty", TextTable.Parse(Csv, "en").Get("vide"));
        }

        [Test]
        public void RefusesAMissingLanguageOrADuplicateKey()
        {
            Assert.Throws<ArgumentException>(() => TextTable.Parse(Csv, "de"));
            Assert.Throws<ArgumentException>(() => TextTable.Parse("clé,fr\nx,a\nx,b\n", "fr"));
        }
    }
}
