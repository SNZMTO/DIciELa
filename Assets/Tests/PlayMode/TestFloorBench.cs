using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le banc des sols vus : un Terrain d'essai de 40 × 40 m peint d'un seul calque, l'herbe, à la résolution de
    /// peinture choisie (ou comme le test la peint d'abord), dont la partie joue sur une copie (l'original n'est jamais
    /// touché) ; le petit monde du campement, dont la Chimère bâtit ; la vue des sols, ses fantômes d'un matériau
    /// d'essai. Ce qu'il crée part avec lui.
    /// </summary>
    internal sealed class TestFloorBench : IDisposable
    {
        public const float SideMeters = 40f;
        private const int MaxSteps = 6000;
        private readonly List<Object> _created = new List<Object>();

        public TestFloorBench(int alphamapResolution, Action<TestFloorBench> paint = null)
        {
            Original = Keep(new TerrainData { heightmapResolution = 33, alphamapResolution = alphamapResolution });
            Original.size = new Vector3(SideMeters, 10f, SideMeters);
            Original.terrainLayers = new[] { Layer("herbe") };
            paint?.Invoke(this);
            Copy = Keep(Object.Instantiate(Original));
            Terrain terrain = Keep(Terrain.CreateTerrainGameObject(Copy)).GetComponent<Terrain>();
            Town = new TestCampTown(true, null, builds: true);
            var settings = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
#if UNITY_EDITOR
            TestWorld.Fill(settings, "_translucentMaterial",
                Keep(new Material(Shader.Find("Universal Render Pipeline/Lit"))));
            TestWorld.Fill(settings, "_floorGhostMaterial", Keep(new Material(Shader.Find("Diciela/SolFantome"))));
#endif
            Views = Keep(new GameObject("Sols de test")).AddComponent<FloorViews>();
            Views.Show(Town.World, terrain, Original, settings);
            Town.InstallTheCamp();
            Town.World.Submit(new AssignVillagerCommand(Town.World.Villagers.Chimera.Id, Town.PlacedCamp.Id));
            Town.World.Step();
        }

        public TerrainData Original { get; }
        public TerrainData Copy { get; }
        public TestCampTown Town { get; }
        public FloorViews Views { get; }
        public World World => Town.World;

        /// <summary>Un calque d'essai de ce nom, à une texture de 4 × 4.</summary>
        public TerrainLayer Layer(string name)
        {
            TerrainLayer layer = Keep(new TerrainLayer { name = name });
            layer.diffuseTexture = Keep(new Texture2D(4, 4));
            return layer;
        }

        /// <summary>Un sol de ce calque, sans liant ni effilochage tant que le test ne lui en donne pas.</summary>
        public FloorDefinition Floor(string id, TerrainLayer layer)
        {
            var floor = Keep(ScriptableObject.CreateInstance<FloorDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(floor, "_id", id);
            TestWorld.Fill(floor, "_layer", layer);
#endif
            return floor;
        }

        /// <summary>
        /// Trace, par l'ordre du joueur, un chemin de ce sol, de 2 m de large, de x = 10 à 30, à cette hauteur.
        /// </summary>
        public void Trace(float z, FloorDefinition floor)
        {
            var stroke = new GroundStroke(new[] { new Vector2(10f, z), new Vector2(30f, z) }, 1f);
            World.Submit(new TraceFloorCommand(stroke, floor, FloorScope.Anywhere));
            World.Step();
        }

        /// <summary>La Chimère bâtit tous les chantiers de la file.</summary>
        public void BuildAll()
        {
            for (int step = 0; step < MaxSteps && World.ConstructionSites.Queue.Count > 0; step++)
                World.Step();
            Assert.IsEmpty(World.ConstructionSites.Queue, "Les chantiers sont bâtis.");
        }

        /// <summary>L'objet de ce nom sous la vue des sols (un fantôme, le voile).</summary>
        public GameObject Child(string name)
        {
            Transform child = Views.transform.Find(name);
            Assert.IsNotNull(child, "La vue des sols n'a pas « " + name + " ».");
            return child.gameObject;
        }

        /// <summary>Les poids des calques du texel de la copie sous ce point.</summary>
        public float[] WeightsAt(float x, float z)
        {
            int resolution = Copy.alphamapResolution;
            float[,,] weights = Copy.GetAlphamaps(Mathf.FloorToInt(x / SideMeters * resolution),
                Mathf.FloorToInt(z / SideMeters * resolution), 1, 1);
            var result = new float[weights.GetLength(2)];
            for (int layer = 0; layer < result.Length; layer++)
                result[layer] = weights[0, 0, layer];
            return result;
        }

        public T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        public void Dispose()
        {
            Town.Dispose();
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }
    }
}
