using System.Collections;
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
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La mise en place des tests de la Place (PlaceSessionTests), sur la scène de test et avec le vrai contenu, par les
    /// gestes du joueur : le campement pris à sa carte et posé au milieu de l'écran, la Chimère mise à son poste (l'ordre
    /// du bouton de sa fiche) ; l'onglet des Métiers, cliqué, offre la Place en première carte ; prise et posée droit
    /// devant la caméra, à un mètre du campement, elle est bâtie par la Chimère. Elle laisse au test le monde, le
    /// campement, la Place bâtie et son milieu. Le monde avance plus vite que le temps réel : elle joue ses pas elle-même,
    /// bornés.
    /// </summary>
    internal sealed class PlaceSession
    {
        public const int MaxFrames = 3600;
        public const int MaxSteps = 12000;
        public const int StepsPerFrame = 200;
        // Le jour laissé entre la Place et le campement, et au-delà de ses bords pour la traverser, en mètres.
        public const float GapMeters = 1f;

        public World World { get; private set; }
        public Structure Camp { get; private set; }
        public Structure Place { get; private set; }
        public Vector2 At { get; private set; }

        /// <summary>Monte la session, geste après geste ; un test PlayMode l'attend (yield return).</summary>
        public IEnumerator Build()
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

            // Droit devant la caméra : on la voit, quelle que soit la forme de la vue du jeu ; assez loin que les empreintes,
            // des carrés droits, laissent un mètre entre elles, quel que soit l'angle de la vue (l'écart se compte sur l'axe
            // que la direction suit le plus). La caméra, partie tout près de la Chimère, recule à sa distance de départ : on
            // attend d'y voir la Place.
            Vector3 forward = camera.transform.forward;
            Vector2 direction = new Vector2(forward.x, forward.z).normalized;
            float halves = (Mathf.Max(camp.Definition.WidthMeters, camp.Definition.DepthMeters) + place.WidthMeters) * 0.5f;
            float along = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            Vector2 at = camp.Placement.Center + direction * (halves / along + GapMeters);
            Vector3 ground = world.Map.Relief.GroundAt(at);
            for (int frame = 0; frame < MaxFrames && !camera.pixelRect.Contains(camera.WorldToScreenPoint(ground)); frame++)
                yield return null;
            Vector3 aim = camera.WorldToScreenPoint(ground);
            Assert.IsTrue(camera.pixelRect.Contains(aim), "On voit l'endroit de la Place (" + at + ", vu en " + aim +
                " ; caméra en " + camera.transform.position + ", vers " + camera.transform.forward + ", écran " +
                camera.pixelRect + ", campement en " + camp.Placement.Center + ").");
            controls.Tool.AimAt(aim);
            Assert.AreEqual(PlacementResult.Ok, controls.Tool.Result, "La Place se pose devant le campement.");
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
            World = world;
            Camp = camp;
            Place = placed;
            At = at;
        }

        /// <summary>Le catalogue du projet : le stock de départ se lit dans le contenu, jamais recopié.</summary>
        public static Catalog CatalogOf()
        {
#if UNITY_EDITOR
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Catalog"))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<Catalog>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#endif
            Assert.Fail("Le projet n'a pas de catalogue.");
            return null;
        }

        /// <summary>Ce que cette construction coûte de cette marchandise.</summary>
        public static int CostOf(StructureDefinition structure, GoodDefinition good)
        {
            int amount = 0;
            foreach (GoodAmount cost in structure.Cost)
            {
                if (ReferenceEquals(cost.Good, good))
                    amount += cost.Amount;
            }
            return amount;
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
    }
}
