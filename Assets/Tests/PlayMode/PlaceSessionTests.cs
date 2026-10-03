using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
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
    /// La Place de bout en bout, sur la scène de test et avec le vrai contenu (décisions 192 et 193), par les gestes du
    /// joueur : le campement pris à sa carte et posé au milieu de l'écran, la Chimère mise à son poste (l'ordre du bouton
    /// de sa fiche) ; l'onglet des Métiers, cliqué, offre la Place en première carte ; prise et posée à 10 m à l'est du
    /// campement, elle se bâtit avec le stock de départ, qui paie son coût ; un trajet la traverse tout droit. Le monde
    /// avance plus vite que le temps réel : le test joue ses pas lui-même, bornés.
    /// </summary>
    public sealed class PlaceSessionTests
    {
        private const int MaxFrames = 3600;
        private const int MaxSteps = 12000;
        private const int StepsPerFrame = 200;

        [UnityTearDown]
        public IEnumerator LeaveTheTestScene()
        {
            yield return TestOpening.Leave(MaxFrames);
        }

        [UnityTest]
        public IEnumerator ThePlaceComesFromTheTradesTabAndIsBuiltFromTheStartingStock()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            yield return TestOpening.Confirm(MaxFrames);
            var controls = TestOpening.FindIn<PlacementControls>(ScenePaths.GameTest);
            for (int frame = 0; frame < MaxFrames && controls.Tool == null; frame++)
                yield return null;
            Assert.IsNotNull(controls.Tool, "Le monde est né.");
            World world = Object.FindAnyObjectByType<GameRunner>().World;
            Camera camera = TestOpening.FindIn<CameraRig>(ScenePaths.GameTest).Camera;
            var bar = TestOpening.FindIn<BuildBar>(ScenePaths.GameTest);

            bar.Open();
            bar.TakeCard(0);
            controls.Tool.AimAt(camera.pixelRect.center);
            Assert.IsTrue(controls.Tool.Confirm(), "Le centre de la carte de test est libre pour le campement.");
            for (int frame = 0; frame < MaxFrames && world.Structures.ChimeraHome == null; frame++)
                yield return null;
            Structure camp = world.Structures.ChimeraHome;
            Assert.IsNotNull(camp, "Le campement est installé.");
            world.Submit(new AssignVillagerCommand(world.Villagers.Chimera.Id, camp.Id));

            bar.Open();
            VisualElement families = bar.GetComponent<UIDocument>().rootVisualElement.Q("families");
            for (int frame = 0; frame < MaxFrames && !(families.childCount == 2 && families[1].worldBound.width > 0f);
                 frame++)
                yield return null;
            Assert.AreEqual(2, families.childCount, "Le campement installé, le Logement et les Métiers.");
            TestHud.Click(families[1]);
            bar.TakeCard(0);
            StructureDefinition place = controls.Tool.Current;
            Assert.IsTrue(place != null && place.Id == "place", "La première carte des Métiers est la Place.");

            Vector2 at = camp.Placement.Center + new Vector2(10f, 0f);
            Vector3 aim = camera.WorldToScreenPoint(world.Map.Relief.GroundAt(at));
            Assert.IsTrue(camera.pixelRect.Contains(aim), "On voit l'endroit de la Place.");
            controls.Tool.AimAt(aim);
            Assert.AreEqual(PlacementResult.Ok, controls.Tool.Result, "La Place se pose à l'est du campement.");
            Assert.IsTrue(controls.Tool.Confirm());
            // L'ordre « poser » s'applique au début du pas suivant.
            world.Step();
            Structure placed = PlacedOf(world, place);
            Assert.IsNotNull(placed, "La Place est posée.");

            for (int step = 0; step < MaxSteps && placed.State != StructureState.Built; step++)
            {
                world.Step();
                if (step % StepsPerFrame == 0)
                    yield return null;
            }
            Assert.AreEqual(StructureState.Built, placed.State, "La Chimère bâtit la Place.");
            Inventory stock = world.Warehouses.StockOf(camp);
            foreach (GoodAmount goods in CatalogOf().Opening.StartingGoods)
                Assert.AreEqual(goods.Amount - CostOf(place, goods.Good), stock.CountOf(goods.Good),
                    "Le stock de départ a payé la Place en " + goods.Good.Id + ".");

            NavGrid grid = world.Grid;
            Vector2Int from = grid.CellOf(at - new Vector2(7f, 0f));
            Vector2Int to = grid.CellOf(at + new Vector2(7f, 0f));
            var path = new Path();
            Assert.IsTrue(world.PathFinder.TryFindPath(new[] { from }, new[] { to }, path), "On traverse la Place.");
            Assert.AreEqual(to.x - from.x + 1, path.Cells.Count, "Le trajet la traverse tout droit.");
            foreach (Vector2Int cell in path.Cells)
                Assert.AreEqual(from.y, cell.y, "Le trajet reste sur sa rangée.");
        }

        private static Structure PlacedOf(World world, StructureDefinition definition)
        {
            foreach (Structure structure in world.Structures.All)
            {
                if (ReferenceEquals(structure.Definition, definition))
                    return structure;
            }
            return null;
        }

        // Le catalogue du projet : le stock de départ se lit dans le contenu, jamais recopié.
        private static Catalog CatalogOf()
        {
#if UNITY_EDITOR
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Catalog"))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<Catalog>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#endif
            Assert.Fail("Le projet n'a pas de catalogue.");
            return null;
        }

        private static int CostOf(StructureDefinition structure, GoodDefinition good)
        {
            int amount = 0;
            foreach (GoodAmount cost in structure.Cost)
            {
                if (ReferenceEquals(cost.Good, good))
                    amount += cost.Amount;
            }
            return amount;
        }
    }
}
