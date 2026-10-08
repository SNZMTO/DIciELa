using System.Collections;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La fiche d'un lieu, vue (GDD, section 16), dans le HUD du jeu : sur le campement installé du petit monde des tests,
    /// quand la Chimère emporte une charge, la ligne de la Pierre dit le nouveau nombre sans être refaite.
    /// </summary>
    public sealed class LocationSheetTests
    {
        [UnityTest]
        public IEnumerator WhenTheStockMovesItsLineIsRefilledInPlace()
        {
#if UNITY_EDITOR
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/UI/Theme.tss");
            var layout = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Hud.uxml");
            var settings = TestAssets.LoadSingle<InterfaceSettings>();
            using (var town = new TestCampTown())
            {
                var panel = town.Keep(ScriptableObject.CreateInstance<PanelSettings>());
                panel.themeStyleSheet = theme;
                GameObject host = town.Keep(new GameObject("Fiche de test"));
                host.SetActive(false);
                var document = host.AddComponent<UIDocument>();
                document.panelSettings = panel;
                document.visualTreeAsset = layout;
                var sheet = host.AddComponent<LocationSheet>();
                host.SetActive(true);
                yield return null;

                town.InstallTheCamp();
                World world = town.World;
                world.Submit(new AssignVillagerCommand(world.Villagers.Chimera.Id, town.PlacedCamp.Id));
                world.Submit(new PlaceStructureCommand(town.Cabin(new GoodAmount(town.Stone, 20)),
                    new Placement(new Vector2(28f, 20f), 0f)));
                world.Step();
                sheet.Show(world, TextTable.Parse(settings.Texts.text, settings.Language));
                sheet.Touch(town.PlacedCamp);
                VisualElement stock = document.rootVisualElement.Q("fiche-stock");
                Assert.AreEqual(2, stock.childCount, "Le Bois et la Pierre du campement.");
                VisualElement stoneLine = stock[1];

                Inventory camp = world.Warehouses.StockOf(town.PlacedCamp);
                for (int step = 0; step < 3000 && camp.CountOf(town.Stone) == 20; step++)
                    world.Step();

                Assert.Less(camp.CountOf(town.Stone), 20, "Une charge de Pierre est partie au chantier.");
                Assert.AreSame(stoneLine, stock[1], "La même ligne, remplie sur place.");
                Assert.AreEqual(camp.CountOf(town.Stone).ToString(), ((Label)stoneLine[2]).text);
                sheet.Hide();
            }
#else
            yield break;
#endif
        }
    }
}
