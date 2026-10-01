using System.Collections;
using System.Text.RegularExpressions;
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
    /// Le panneau Construire dans la scène de test, une fois la Chimère créée, ouvert comme le fait la roue des outils :
    /// il montre le catalogue du jeu, dans la langue de ses réglages (lus dans le projet), et dit aux gestes quand le
    /// pointeur est sur lui. Le pointeur compte depuis le bas de l'écran, UI Toolkit depuis le haut : l'attendu se
    /// calcule par la conversion d'UI Toolkit, inversée.
    /// </summary>
    public sealed class BuildBarTests
    {
        private const int MaxFrames = 3600;

        [UnityTest]
        public IEnumerator TheBarShowsTheCatalogAndCatchesThePointer()
        {
            LogAssert.Expect(LogType.Log, new Regex("^DICIELA : monde créé"));
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePaths.GameTest,
                new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            yield return TestOpening.Confirm(MaxFrames);
            BuildBar bar = null;
            VisualElement structures = null;
            // La borne compte les images, jamais les secondes : une machine lente attend plus longtemps.
            for (int frame = 0; frame < MaxFrames; frame++)
            {
                yield return null;
                bar = Object.FindAnyObjectByType<BuildBar>();
                if (bar != null && !bar.IsOpen)
                    bar.Open();
                structures = bar != null ? bar.GetComponent<UIDocument>().rootVisualElement.Q("structures") : null;
                // Ouvert, le panneau se mesure à l'image suivante : caché, il mesure 0.
                if (structures != null && structures.childCount > 0 && structures.worldBound.width > 0f)
                    break;
            }
            Assert.IsNotNull(structures, "La barre de la scène de test doit exister.");
            Assert.That(structures.childCount, Is.GreaterThan(0), "La barre doit montrer des constructions.");

#if UNITY_EDITOR
            Catalog catalog = LoadSingle<Catalog>();
            InterfaceSettings settings = LoadSingle<InterfaceSettings>();
            TextTable texts = TextTable.Parse(settings.Texts.text, settings.Language);
            Assert.AreEqual(texts.Get(catalog.Families[0].Structures[0].NameKey), structures[0].Q<Label>().text);
#endif

            VisualElement root = bar.GetComponent<UIDocument>().rootVisualElement;
            Vector2 origin = RuntimePanelUtils.ScreenToPanel(root.panel, Vector2.zero);
            Vector2 unit = RuntimePanelUtils.ScreenToPanel(root.panel, Vector2.one) - origin;
            Vector2 center = root.Q("bar").worldBound.center;
            var fromTop = new Vector2((center.x - origin.x) / unit.x, (center.y - origin.y) / unit.y);
            var hud = new HudArea(bar.GetComponent<UIDocument>());
            Assert.IsTrue(hud.Contains(new Vector2(fromTop.x, Screen.height - fromTop.y)),
                "Le milieu de la barre est sur l'interface.");
            Assert.IsFalse(hud.Contains(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)),
                "Le milieu de l'écran est la ville.");

            LogAssert.Expect(LogType.Log, new Regex("^DICIELA : monde arrêté"));
            yield return SceneManager.UnloadSceneAsync(ScenePaths.GameTest);
            for (int frame = 0; frame < MaxFrames && SceneManager.GetSceneByPath(ScenePaths.MapTest).isLoaded; frame++)
                yield return null;
        }

#if UNITY_EDITOR
        /// <summary>La seule fiche de ce type dans le projet (les tests qui lisent les vraies données).</summary>
        internal static T LoadSingle<T>() where T : Object
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);
            Assert.AreEqual(1, guids.Length, "Il faut une seule fiche " + typeof(T).Name + " dans le projet.");
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
        }
#endif
    }
}
