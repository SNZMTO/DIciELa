using Diciela.Controls;
using Diciela.Data;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les familles de constructions des tests, faites en mémoire et rangées dans le catalogue du petit monde, comme
    /// l'Inspector le ferait : chacune son nom, son pastel et ses constructions, sans icône ; ce qu'une construction
    /// offre et coûte ; et le campement posé par Construire.
    /// </summary>
    internal static class TestFamilies
    {
        /// <summary>Une famille de ce nom et de ce pastel, qui propose ces constructions, dans cet ordre.</summary>
        public static StructureFamilyDefinition Family(this TestWorld test, string nameKey, Pastel pastel,
            params StructureDefinition[] structures)
        {
            var family = test.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(family, "_nameKey", nameKey);
            TestWorld.Fill(family, "_pastel", pastel);
            TestWorld.FillList(family, "_structures", structures);
#endif
            return family;
        }

        /// <summary>
        /// Range ces familles dans le catalogue du monde, dans cet ordre, à la place de celles qu'il avait.
        /// </summary>
        public static void SetFamilies(this TestWorld test, params StructureFamilyDefinition[] families)
        {
#if UNITY_EDITOR
            TestWorld.FillList(test.Catalog, "_families", families);
#endif
        }

        /// <summary>Le Logement, rose : le campement, puis la cabane ; seule famille du catalogue.</summary>
        public static StructureFamilyDefinition Housing(this TestWorld test)
        {
            StructureFamilyDefinition housing = test.Family("famille.logement", Pastel.Rose, test.Camp, test.Cabin);
            test.SetFamilies(housing);
            return housing;
        }

        /// <summary>
        /// Le campement, pris par Construire dans cette roue et posé au centre du petit monde, comme le ferait le
        /// joueur ; posé, il quitte la main.
        /// </summary>
        public static void PlantTheCamp(this TestWorld test, ToolWheelPresenter wheel)
        {
            wheel.Choose(ToolChoice.Build);
            test.Tool.AimAt(test.OnScreen(TestWorld.Center));
            Assert.IsTrue(test.Tool.Confirm(), "Le centre du petit monde est libre.");
            test.World.Step();
        }

        /// <summary>Une marchandise de ce nom, sans icône.</summary>
        public static GoodDefinition Good(this TestWorld test, string nameKey)
        {
            var good = test.Keep(ScriptableObject.CreateInstance<GoodDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(good, "_nameKey", nameKey);
#endif
            return good;
        }

        /// <summary>Donne à cette construction ces lits et ce coût, comme l'Inspector.</summary>
        public static void Offer(StructureDefinition structure, int beds, params GoodAmount[] cost)
        {
#if UNITY_EDITOR
            TestWorld.Fill(structure, "_beds", beds);
            TestCampTown.FillGoods(structure, "_cost", cost);
#endif
        }
    }
}
