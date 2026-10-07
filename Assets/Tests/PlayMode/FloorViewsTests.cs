using System.Collections;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les sols vus (décisions 209, 213 et 216), par le chemin du joueur, sur un Terrain d'essai de 40 × 40 m peint d'un
    /// seul calque, l'herbe (64 texels de côté, 62,5 cm chacun), dont la partie joue sur une copie : un chemin tracé se
    /// voit en fantôme, à la texture de son calque, posée en tuiles comme sur le Terrain ; bâti par la Chimère, il se peint
    /// du calque de sa fiche, la terre, ajouté à la copie ; des pavés bâtis ensuite ajoutent leur calque sans défaire la
    /// terre ; un chemin qu'on efface se voile, puis rend la peinture de la carte. L'original n'est jamais touché.
    /// </summary>
    public sealed class FloorViewsTests
    {
        private const int MaxSteps = 4000;
        private readonly List<Object> _created = new List<Object>();
        private TestCampTown _town;
        private TerrainData _original;
        private TerrainData _copy;
        private FloorViews _views;
        private FloorDefinition _dirt;
        private FloorDefinition _paving;

        [SetUp]
        public void Create()
        {
            _original = Keep(new TerrainData { heightmapResolution = 33, alphamapResolution = 64 });
            _original.size = new Vector3(40f, 10f, 40f);
            _original.terrainLayers = new[] { Layer("herbe") };
            _copy = Keep(Object.Instantiate(_original));
            Terrain terrain = Keep(Terrain.CreateTerrainGameObject(_copy)).GetComponent<Terrain>();
            _dirt = Floor("terre", Layer("terre"));
            _dirt.Layer.tileSize = new Vector2(2f, 4f);
            _dirt.Layer.tileOffset = new Vector2(1f, 0f);
            _paving = Floor("paves", Layer("paves"));
            _town = new TestCampTown(true, null, builds: true);
            _town.KnowsFloors(_dirt, _paving);
            var settings = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
#if UNITY_EDITOR
            TestWorld.Fill(settings, "_translucentMaterial",
                Keep(new Material(Shader.Find("Universal Render Pipeline/Lit"))));
            TestWorld.Fill(settings, "_floorGhostMaterial", Keep(new Material(Shader.Find("Diciela/SolFantome"))));
#endif
            _views = Keep(new GameObject("Sols de test")).AddComponent<FloorViews>();
            _views.Show(_town.World, terrain, _original, settings);
            _town.InstallTheCamp();
            _town.World.Submit(new AssignVillagerCommand(_town.World.Villagers.Chimera.Id, _town.PlacedCamp.Id));
            _town.World.Step();
        }

        [TearDown]
        public void Destroy()
        {
            _town.Dispose();
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        [UnityTest]
        public IEnumerator AFloorIsAGhostAtItsTextureThenItsLayerThenTheMapAgain()
        {
            World world = _town.World;
            Trace(world, 30f, _dirt);
            yield return null;
            GameObject dirtGhost = Child("Fantôme du sol terre");
            Assert.IsTrue(dirtGhost.activeSelf, "Tracé, le chemin se voit en fantôme…");
            Material ghost = dirtGhost.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreSame(_dirt.Layer.diffuseTexture, ghost.GetTexture("_BaseMap"), "… à la texture de son calque…");
            Assert.AreEqual(new Vector2(0.5f, 0.25f), ghost.GetTextureScale("_BaseMap"), "… en tuiles de 2 × 4 m…");
            Assert.AreEqual(new Vector2(0.5f, 0f), ghost.GetTextureOffset("_BaseMap"), "… décalées comme sur le Terrain.");
            Assert.IsFalse(Child("Voile des sols").activeSelf, "Un chemin qu'on trace n'est pas voilé.");
            Assert.AreEqual(1, _copy.alphamapLayers, "Pas encore bâti : rien n'est peint.");

            BuildAll(world);
            yield return null;
            Assert.IsFalse(dirtGhost.activeSelf, "Bâti, il n'a plus de fantôme…");
            Assert.AreEqual(2, _copy.alphamapLayers, "… et le calque de la terre s'ajoute à la copie.");
            CollectionAssert.AreEqual(new[] { 0f, 1f }, WeightsAt(20f, 30f), "Le chemin, de terre.");
            CollectionAssert.AreEqual(new[] { 1f, 0f }, WeightsAt(20f, 20f), "À côté, l'herbe de la carte.");

            Trace(world, 6f, _paving);
            BuildAll(world);
            yield return null;
            CollectionAssert.AreEqual(new[] { 0f, 0f, 1f }, WeightsAt(20f, 6f), "Les pavés, de leur calque.");
            CollectionAssert.AreEqual(new[] { 0f, 1f, 0f }, WeightsAt(20f, 30f), "La terre reste.");

            _town.World.Submit(new EraseFloorCommand(new GroundBox(new RotatedRect(new Vector2(20f, 30f),
                new Vector2(30f, 4f), 0f)), FloorScope.Anywhere));
            world.Step();
            yield return null;
            Assert.IsTrue(Child("Voile des sols").activeSelf, "Qu'on l'efface, il se voile de la couleur des fantômes…");
            CollectionAssert.AreEqual(new[] { 0f, 1f, 0f }, WeightsAt(20f, 30f), "… et sert jusqu'à la fin.");
            BuildAll(world);
            yield return null;
            CollectionAssert.AreEqual(new[] { 1f, 0f, 0f }, WeightsAt(20f, 30f), "Effacé : l'herbe de la carte.");
            Assert.AreEqual(1, _original.alphamapLayers, "L'original n'est jamais touché.");
        }

        // Un chemin de 2 m de large, de x = 10 à 30, à cette hauteur sur la carte, par l'ordre du joueur.
        private static void Trace(World world, float z, FloorDefinition floor)
        {
            var stroke = new GroundStroke(new[] { new Vector2(10f, z), new Vector2(30f, z) }, 1f);
            world.Submit(new TraceFloorCommand(stroke, floor, FloorScope.Anywhere));
            world.Step();
        }

        // La Chimère bâtit tous les chantiers de la file.
        private static void BuildAll(World world)
        {
            for (int step = 0; step < MaxSteps && world.ConstructionSites.Queue.Count > 0; step++)
                world.Step();
            Assert.IsEmpty(world.ConstructionSites.Queue, "Les chantiers sont bâtis.");
        }

        private GameObject Child(string name)
        {
            Transform child = _views.transform.Find(name);
            Assert.IsNotNull(child, "La vue des sols n'a pas « " + name + " ».");
            return child.gameObject;
        }

        // Les poids des calques du texel de la copie sous ce point.
        private float[] WeightsAt(float x, float z)
        {
            float[,,] weights = _copy.GetAlphamaps(Mathf.FloorToInt(x / 40f * 64f), Mathf.FloorToInt(z / 40f * 64f), 1,
                1);
            var result = new float[weights.GetLength(2)];
            for (int layer = 0; layer < result.Length; layer++)
                result[layer] = weights[0, 0, layer];
            return result;
        }

        private TerrainLayer Layer(string name)
        {
            TerrainLayer layer = Keep(new TerrainLayer { name = name });
            layer.diffuseTexture = Keep(new Texture2D(4, 4));
            return layer;
        }

        private FloorDefinition Floor(string id, TerrainLayer layer)
        {
            var floor = Keep(ScriptableObject.CreateInstance<FloorDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(floor, "_id", id);
            TestWorld.Fill(floor, "_layer", layer);
#endif
            return floor;
        }

        private T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }
    }
}
