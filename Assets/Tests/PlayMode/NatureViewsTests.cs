using System.Collections.Generic;
using System.Text.RegularExpressions;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les arbres peints et vus (décision 157), sur un Terrain en mémoire de 40 × 40 m qui porte trois arbres : deux d'un
    /// modèle rangé dans la sorte « arbre » (un cylindre du moteur, modèle d'essai), un d'un modèle rangé nulle part. La
    /// lecture ne garde que les deux premiers, à leur place, leur angle et leur largeur, avec leur numéro d'instance, et
    /// dit le modèle orphelin ; marqué puis retiré par la Chimère, un arbre disparaît du Terrain (sa taille passe à 0),
    /// l'autre reste.
    /// </summary>
    public sealed class NatureViewsTests
    {
        private readonly List<Object> _created = new List<Object>();
        private TerrainData _data;
        private NatureDefinition _tree;

        [SetUp]
        public void PaintTheTerrain()
        {
            GameObject ranged = Keep(GameObject.CreatePrimitive(PrimitiveType.Cylinder));
            GameObject orphan = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            _data = Keep(new TerrainData { heightmapResolution = 33 });
            _data.size = new Vector3(40f, 10f, 40f);
            _data.treePrototypes = new[] { new TreePrototype { prefab = ranged }, new TreePrototype { prefab = orphan } };
            _data.SetTreeInstances(new[]
            {
                Tree(0, 28f, 22f, 1.5f, Mathf.PI / 2f), Tree(1, 10f, 10f, 1f, 0f), Tree(0, 10f, 30f, 1f, 0f)
            }, false);

            _tree = Keep(ScriptableObject.CreateInstance<NatureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(_tree, "_id", "arbre");
            TestWorld.FillList(_tree, "_models", ranged);
            TestWorld.Fill(_tree, "_workHours", 0.5f);
#endif
        }

        [TearDown]
        public void DestroyAll()
        {
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        [Test]
        public void TheReaderKeepsTheRangedTreesAndNamesTheOrphanModel()
        {
            var instances = new List<int>();
            LogAssert.Expect(LogType.Warning, new Regex("Cube.*n'est rangé dans aucune sorte"));
            MapDescription map = MapReader.Read(_data, Vector3.zero, null, new[] { _tree }, instances);

            Assert.AreEqual(2, map.Nature.Count);
            Assert.AreEqual(new[] { 0, 2 }, instances.ToArray(), "Le numéro d'instance de chacun sur le Terrain.");
            Assert.Less(Vector2.Distance(new Vector2(28f, 22f), map.Nature[0].Placement.Center), 1e-3f);
            Assert.AreEqual(90f, map.Nature[0].Placement.RotationDegrees, 1e-3f);
            Assert.AreEqual(1.5f, map.Nature[0].Scale, 1e-4f);
            Assert.AreSame(_tree, map.Nature[1].Definition);
        }

        [Test]
        public void AClearedTreeLeavesTheTerrainAndTheOtherStays()
        {
            var instances = new List<int>();
            LogAssert.ignoreFailingMessages = true;
            MapDescription map;
            try
            {
                map = MapReader.Read(_data, Vector3.zero, null, new[] { _tree }, instances);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
            Terrain terrain = Keep(Terrain.CreateTerrainGameObject(_data)).GetComponent<Terrain>();
            var town = new TestPileTown(true, map);
            var settings = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
            var views = Keep(new GameObject("Nature")).AddComponent<NatureViews>();
            try
            {
#if UNITY_EDITOR
                TestWorld.Fill(settings, "_translucentMaterial",
                    Keep(new Material(Shader.Find("Universal Render Pipeline/Lit"))));
                TestWorld.Fill(views, "_settings", settings);
#endif
                views.Show(town.World.Nature, terrain, instances, town.World.Map.Relief);
                town.PlantTheTent();
                town.World.Submit(new AssignVillagerCommand(town.World.Villagers.Chimera.Id, town.PlacedPile.Id));
                town.World.Submit(new MarkForClearingCommand(new[] { 1 }));
                NatureItem first = town.World.Nature.Find(1);
                for (int step = 0; step < 6000 && !first.IsRemoved; step++)
                    town.World.Step();

                Assert.IsTrue(first.IsRemoved, "La Chimère a retiré l'arbre marqué.");
                Assert.AreEqual(0f, _data.GetTreeInstance(0).heightScale, "Retiré, il a quitté le Terrain.");
                Assert.AreEqual(0f, _data.GetTreeInstance(0).widthScale);
                Assert.AreEqual(1f, _data.GetTreeInstance(2).heightScale, 1e-4f, "L'autre reste.");
            }
            finally
            {
                views.Hide();
                town.Dispose();
            }
        }

        private static TreeInstance Tree(int prototype, float x, float z, float scale, float rotation)
        {
            return new TreeInstance
            {
                prototypeIndex = prototype,
                position = new Vector3(x / 40f, 0f, z / 40f),
                widthScale = scale,
                heightScale = scale,
                rotation = rotation,
                color = Color.white,
                lightmapColor = Color.white
            };
        }

        private T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }
    }
}
