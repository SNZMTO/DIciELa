using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La Place dans la scène de test, de bout en bout (décision 206) : la Chimère créée et son campement posé, l'onglet
    /// des Métiers paraît, et sa première carte est la Place ; la prendre ferme le panneau, met l'outil en main, montre sa
    /// barre et le voile de la zone ; un mode se prend à sa barre ; un disque tracé agrandit la Place, et la barre le dit ;
    /// une place de charrette se pose dedans ; Échap lâche l'outil, la barre et la zone se cachent. L'interface se clique
    /// par ses éléments (TestHud) ; dans la ville, l'outil reçoit les appels que ses gestes lui font, chacun dans la même
    /// image (les gestes, à la souris réelle, ne glissent pas pendant ce temps).
    /// </summary>
    public sealed class PlaceSceneTests
    {
        private const int MaxFrames = 3600;
        private const float DiscRadius = 6f;
        // La classe d'un bloc caché du HUD (Fusion.uss).
        private const string Hidden = "bloc-cache";

        [UnityTearDown]
        public IEnumerator LeaveTheTestScene()
        {
            yield return TestOpening.Leave(MaxFrames);
        }

        [UnityTest]
        public IEnumerator ThePlaceIsTracedFromItsCard()
        {
            LogAssert.Expect(LogType.Log, new Regex("^DICIELA : monde créé"));
            Catalog catalog = null;
            TextTable texts = null;
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                new LoadSceneParameters(LoadSceneMode.Additive));
            catalog = BuildBarTests.LoadSingle<Catalog>();
            InterfaceSettings settings = BuildBarTests.LoadSingle<InterfaceSettings>();
            texts = TextTable.Parse(settings.Texts.text, settings.Language);
#endif
            yield return TestOpening.Confirm(MaxFrames);
            World world = Object.FindAnyObjectByType<GameRunner>().World;
            yield return Until(() => world.Villagers.Chimera != null, "La Chimère naît.");

            // Le campement, par l'ordre du joueur, à la première place libre près du point d'arrivée.
            StructureDefinition camp = HomeOf(catalog);
            world.Submit(new PlaceStructureCommand(camp, FirstAllowed(world, camp, world.Map.ArrivalPoint)));
            yield return Until(() => world.Structures.ChimeraHome != null, "Le campement est posé.");

            var bar = Object.FindAnyObjectByType<BuildBar>();
            VisualElement root = bar.GetComponent<UIDocument>().rootVisualElement;
            bar.Open();
            yield return Until(() => root.Q("families").childCount == 2, "Le Logement, puis les Métiers.");
            TestHud.Click(root.Q("families")[1]);
            yield return null;
            VisualElement cards = root.Q("structures");
            Assert.AreEqual(texts.Get(catalog.Place.NameKey), cards[0].Q<Label>().text, "La Place mène les Métiers.");

            TestHud.Click(cards[0]);
            yield return null;
            PlaceTool tool = Object.FindAnyObjectByType<PlaceControls>().Tool;
            var zone = Object.FindAnyObjectByType<PlaceZoneView>();
            Assert.IsTrue(tool.IsActive, "La carte met l'outil de la Place en main…");
            Assert.IsFalse(bar.IsOpen, "… ferme le panneau…");
            Assert.IsFalse(root.Q("place-barre").ClassListContains(Hidden), "… montre sa barre…");
            Assert.IsTrue(zone.transform.Find("Voile de la Place").gameObject.activeSelf, "… et la zone.");

            TestHud.Click(root.Q("place-modes")[(int)PlaceToolMode.Disc]);
            Assert.AreEqual(PlaceToolMode.Disc, tool.Mode, "Un mode se prend à la barre.");
            Camera camera = Object.FindAnyObjectByType<CameraRig>().Camera;
            Vector2 center = FirstClearDisc(world, world.Structures.ChimeraHome.Placement.Center);
            tool.BeginDrawing(OnScreen(camera, world, center));
            tool.AimAt(OnScreen(camera, world, center + new Vector2(DiscRadius, 0f)));
            Assert.AreEqual(PlaceResult.Ok, tool.Result, "L'aperçu dit que le disque se trace.");
            tool.EndDrawing();
            yield return Until(() => world.Place.CellCount > 0, "Le disque part au relâchement.");
            Assert.AreEqual(Mathf.PI * DiscRadius * DiscRadius, world.Place.AreaSquareMeters, 15f);
            yield return null;
            StringAssert.StartsWith(Mathf.RoundToInt(world.Place.AreaSquareMeters) + " / ",
                root.Q<Label>("place-superficie").text, "La barre dit la superficie.");

            TestHud.Click(root.Q("place-modes")[(int)PlaceToolMode.CartSpot]);
            tool.AimAt(OnScreen(camera, world, center));
            Assert.AreEqual(PlaceResult.Ok, tool.Result, "Une place de charrette tient au milieu du disque.");
            tool.Click();
            yield return Until(() => world.Place.Marks.Count == 1, "Le clic la pose.");

            Assert.IsTrue(tool.Escape(), "Échap lâche l'outil…");
            Assert.IsTrue(root.Q("place-barre").ClassListContains(Hidden), "… sa barre se cache…");
            Assert.IsFalse(zone.transform.Find("Voile de la Place").gameObject.activeSelf, "… et la zone aussi.");

            LogAssert.Expect(LogType.Log, new Regex("^DICIELA : monde arrêté"));
            yield return TestOpening.Leave(MaxFrames);
        }

        private static IEnumerator Until(System.Func<bool> done, string what)
        {
            // La borne compte les images, jamais les secondes : une machine lente attend plus longtemps.
            for (int frame = 0; frame < MaxFrames && !done(); frame++)
                yield return null;
            Assert.IsTrue(done(), what);
        }

        private static StructureDefinition HomeOf(Catalog catalog)
        {
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                if (structure.IsChimeraHome)
                    return structure;
            }
            Assert.Fail("Le catalogue n'a pas de logis pour la Chimère.");
            return null;
        }

        // La première place où les règles de pose acceptent la construction, en tournant autour de ce point.
        private static Placement FirstAllowed(World world, StructureDefinition structure, Vector2 near)
        {
            foreach (Vector2 point in Around(near))
            {
                var placement = new Placement(point, 0f);
                if (world.PlacementRules.Check(structure, placement) == PlacementResult.Ok)
                    return placement;
            }
            Assert.Fail("Aucune place libre près du point d'arrivée.");
            return default;
        }

        // Le milieu du premier disque libre de tout obstacle autour de ce point : la Place s'y trace pleine.
        private static Vector2 FirstClearDisc(World world, Vector2 near)
        {
            var change = new List<Vector2Int>();
            float cellArea = world.Grid.Metrics.CellSize * world.Grid.Metrics.CellSize;
            foreach (Vector2 point in Around(near))
            {
                if ((point - near).magnitude < DiscRadius + 4f)
                    continue;
                if (world.Place.CheckPaint(new GroundDisc(point, DiscRadius), change) == PlaceResult.Ok &&
                    change.Count * cellArea > Mathf.PI * DiscRadius * DiscRadius * 0.97f)
                    return point;
            }
            Assert.Fail("Aucun disque libre autour du campement.");
            return near;
        }

        private static IEnumerable<Vector2> Around(Vector2 near)
        {
            for (float radius = 4f; radius <= 24f; radius += 2f)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = i * Mathf.PI / 6f;
                    yield return near + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                }
            }
        }

        private static Vector2 OnScreen(Camera camera, World world, Vector2 ground)
        {
            return camera.WorldToScreenPoint(world.Map.Relief.GroundAt(ground));
        }
    }
}
