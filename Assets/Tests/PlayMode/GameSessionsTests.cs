using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Game;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Deux parties de suite sur la scène de test, comme deux entrées en Play d'un projet qui ne recharge ni le code ni
    /// la scène : chacune crée un seul monde, sa Chimère validée à l'écran de création, et une pose, comme le joueur la
    /// fait (le panneau Construire, sa première carte, le campement, puis un clic), y donne un seul ordre, une seule
    /// ligne de journal et une seule construction vue. Chaque partie joue sur une copie du sol : l'asset
    /// du Terrain de la carte de test, lu sur le disque, sort des deux parties inchangé (la copie, elle, pourra changer
    /// quand on terrassera).
    /// </summary>
    public sealed class GameSessionsTests
    {
        private const int Sessions = 2;
        private const int MaxFrames = 1200;

        private int _created;
        private int _placed;

        [UnityTearDown]
        public IEnumerator LeaveTheTestScene()
        {
            yield return TestOpening.Leave(MaxFrames);
        }

        private void Count(string message, string stackTrace, LogType type)
        {
            if (message.StartsWith("DICIELA : monde créé"))
                _created++;
            if (message.StartsWith("DICIELA : construction n° 1 ("))
                _placed++;
        }

        [UnityTest]
        public IEnumerator TwoSessionsInARowEachPlaceOnce()
        {
#if UNITY_EDITOR
            TerrainData asset = TestMapGround();
            float[,] heightsBefore = Heights(asset);
#endif
            Application.logMessageReceived += Count;
            try
            {
                for (int session = 1; session <= Sessions; session++)
                {
#if UNITY_EDITOR
                    yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                        ScenePaths.GameTest, new LoadSceneParameters(LoadSceneMode.Additive));
#endif
                    yield return TestOpening.Confirm(MaxFrames);
                    var controls = FindIn<PlacementControls>(ScenePaths.GameTest);
                    for (int frame = 0; frame < MaxFrames && controls.Tool == null; frame++)
                        yield return null;
                    Assert.IsNotNull(controls.Tool, "Le monde de la partie " + session + " est né.");
                    TerrainData terrain = MapRoot.FindIn(SceneManager.GetSceneByPath(ScenePaths.MapTest)).Terrain.terrainData;
#if UNITY_EDITOR
                    Assert.IsFalse(UnityEditor.AssetDatabase.Contains(terrain), "La partie joue sur une copie du sol.");
#endif

                    var bar = FindIn<BuildBar>(ScenePaths.GameTest);
                    bar.Open();
                    bar.TakeCard(0);
                    StructureDefinition held = controls.Tool.Current;
                    Assert.IsTrue(held != null && held.IsChimeraHome, "La première carte est le campement.");
                    controls.Tool.AimAt(FindIn<CameraRig>(ScenePaths.GameTest).Camera.pixelRect.center);
                    Assert.IsTrue(controls.Tool.Confirm(), "Le centre de la carte de test est libre.");
                    for (int frame = 0; frame < MaxFrames && _placed < session; frame++)
                        yield return null;

                    Assert.AreEqual(session, _created, "Un seul monde par partie.");
                    Assert.AreEqual(session, _placed, "Une seule pose par partie, numérotée depuis 1.");
                    Assert.AreEqual(1, FindIn<StructureViews>(ScenePaths.GameTest).transform.childCount);
#if UNITY_EDITOR
                    Assert.AreEqual(heightsBefore, Heights(asset), "L'asset du Terrain de la carte n'est jamais modifié.");
#endif

                    yield return TestOpening.Leave(MaxFrames);
                }
            }
            finally
            {
                Application.logMessageReceived -= Count;
            }
        }

#if UNITY_EDITOR
        /// <summary>Le sol de la carte de test tel qu'il est sur le disque : l'asset de Terrain dont dépend sa scène.</summary>
        private static TerrainData TestMapGround()
        {
            foreach (string path in UnityEditor.AssetDatabase.GetDependencies(ScenePaths.MapTest, false))
            {
                var ground = UnityEditor.AssetDatabase.LoadAssetAtPath<TerrainData>(path);
                if (ground != null)
                    return ground;
            }
            Assert.Fail("La carte de test n'a pas de Terrain.");
            return null;
        }
#endif

        private static float[,] Heights(TerrainData terrain)
        {
            return terrain.GetHeights(0, 0, terrain.heightmapResolution, terrain.heightmapResolution);
        }

        /// <summary>Le premier composant de ce type dans une scène chargée, même sur un objet inactif.</summary>
        private static T FindIn<T>(string scenePath) where T : Component
        {
            foreach (GameObject root in SceneManager.GetSceneByPath(scenePath).GetRootGameObjects())
            {
                var found = root.GetComponentInChildren<T>(true);
                if (found != null)
                    return found;
            }
            Assert.Fail("La scène « " + scenePath + " » n'a pas de " + typeof(T).Name + ".");
            return null;
        }
    }
}
