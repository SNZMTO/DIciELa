using System.Collections;
using System.Text.RegularExpressions;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Construire dans la scène de test, une fois la Chimère créée (décision 226) : sa touche met en main la première
    /// construction que le catalogue du jeu propose, le campement ; sa roue, ouverte comme Tab tenu au milieu de
    /// l'écran, prend le pointeur et montre en haut la première famille du jeu, que la bulle nomme dans la langue des
    /// réglages (lus dans le projet) quand on la vise ; relâcher sur elle garde le campement en main et ferme la roue,
    /// et le milieu de l'écran redevient la ville.
    /// </summary>
    public sealed class BuildWheelSceneTests
    {
        private const int MaxFrames = 3600;
        // Assez loin du milieu pour viser la case du haut, assez près pour rester sur le plateau.
        private const float AimPixels = 120f;

        [UnityTearDown]
        public IEnumerator LeaveTheTestScene()
        {
            yield return TestOpening.Leave(MaxFrames);
        }

        [UnityTest]
        public IEnumerator BuildTakesTheCampAndItsWheelShowsTheGamesFirstFamily()
        {
            LogAssert.Expect(LogType.Log, new Regex("^DICIELA : monde créé"));
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            yield return TestOpening.Confirm(MaxFrames);
            var controls = TestOpening.FindIn<PlacementControls>(ScenePaths.GameTest);
            // La borne compte les images, jamais les secondes : une machine lente attend plus longtemps.
            for (int frame = 0; frame < MaxFrames && controls.Tool == null; frame++)
                yield return null;
            Assert.IsNotNull(controls.Tool, "La ville de la scène de test s'est ouverte.");
            var wheel = TestOpening.FindIn<ToolWheel>(ScenePaths.GameTest);

            wheel.Choose(ToolChoice.Build);
            StructureDefinition held = controls.Tool.Current;
            Assert.IsTrue(held != null && held.IsChimeraHome, "Construire met en main le campement, proposé d'abord.");

            VisualElement root = wheel.GetComponent<UIDocument>().rootVisualElement;
            var middle = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var up = new Vector2(middle.x, middle.y + AimPixels);
            wheel.OpenAt(middle);
            yield return TestHud.Settle();
            wheel.PointAt(up);
            yield return TestHud.Settle();
#if UNITY_EDITOR
            Catalog catalog = TestAssets.LoadSingle<Catalog>();
            InterfaceSettings settings = TestAssets.LoadSingle<InterfaceSettings>();
            TextTable texts = TextTable.Parse(settings.Texts.text, settings.Language);
            Assert.AreEqual(texts.Get(catalog.Families[0].NameKey), root.Q<Label>("roue-nom").text,
                "Visée, la case du haut est la première famille du jeu.");
#endif
            Assert.IsNotNull(root.Q("roue-cases").Q(className: "vise"), "Une case est visée.");
            var hud = new HudArea(wheel.GetComponent<UIDocument>());
            Assert.IsTrue(hud.Contains(middle), "Ouverte, la roue prend le pointeur : la ville n'y répond pas.");

            wheel.ReleaseAt(up);
            Assert.AreSame(held, controls.Tool.Current, "Relâcher sur la famille du campement le garde en main.");
            yield return TestHud.Settle();
            Assert.IsFalse(hud.Contains(middle), "La roue fermée, le milieu de l'écran est la ville.");

            LogAssert.Expect(LogType.Log, new Regex("^DICIELA : monde arrêté"));
            yield return TestOpening.Leave(MaxFrames);
        }
    }
}
