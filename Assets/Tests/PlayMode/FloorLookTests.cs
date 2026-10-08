using System.Collections;
using System.Collections.Generic;
using Diciela.Data;
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
    /// L'aspect des sols peints (décision 227), par le chemin du joueur, sur un Terrain d'essai de 40 × 40 m peint
    /// d'herbe (128 texels de côté, 31,25 cm chacun) : un disque de 3 m de rayon, tracé puis bâti par la Chimère. Un
    /// sol au bord lisse est rond et fondu (des texels mêlés tout autour de son bord, aucun à plus de 45 cm de son
    /// cercle) ; des pavés sont cernés de leur liant, une bande de 80 cm, puis l'herbe revient ; un bord qui
    /// s'effiloche s'écarte de son cercle ici et là. Tous les poids d'un texel font un.
    /// </summary>
    public sealed class FloorLookTests
    {
        private const int MaxSteps = 6000;
        private const float Radius = 3f;
        private const float TexelMeters = 40f / 128f;
        private static readonly Vector2 Center = new Vector2(20f, 30f);
        private readonly List<Object> _created = new List<Object>();
        private TestCampTown _town;
        private TerrainData _copy;
        private TerrainLayer _grass;
        private TerrainLayer _binder;

        [SetUp]
        public void Create()
        {
            var original = Keep(new TerrainData { heightmapResolution = 33, alphamapResolution = 128 });
            original.size = new Vector3(40f, 10f, 40f);
            _grass = Layer("herbe");
            original.terrainLayers = new[] { _grass };
            _copy = Keep(Object.Instantiate(original));
            Terrain terrain = Keep(Terrain.CreateTerrainGameObject(_copy)).GetComponent<Terrain>();
            _binder = Layer("liant");
            _town = new TestCampTown(true, null, builds: true);
            var settings = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
#if UNITY_EDITOR
            TestWorld.Fill(settings, "_translucentMaterial",
                Keep(new Material(Shader.Find("Universal Render Pipeline/Lit"))));
            TestWorld.Fill(settings, "_floorGhostMaterial", Keep(new Material(Shader.Find("Diciela/SolFantome"))));
#endif
            var views = Keep(new GameObject("Sols de test")).AddComponent<FloorViews>();
            views.Show(_town.World, terrain, original, settings);
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
        public IEnumerator ASmoothFloorIsRoundAndFadesAtItsEdge()
        {
            FloorDefinition dirt = Floor("terre", null, 0f);
            yield return Build(dirt);
            int dirtLayer = 1, blended = 0;
            ForEachTexel((distance, weights) =>
            {
                if (distance < Radius - 0.45f)
                    Assert.GreaterOrEqual(weights[dirtLayer], 0.95f,
                        "Dedans, à " + distance + " m du milieu : la terre.");
                else if (distance > Radius + 0.45f)
                    Assert.LessOrEqual(weights[dirtLayer], 0.05f, "Dehors, à " + distance + " m du milieu : l'herbe.");
                else if (weights[dirtLayer] > 0.05f && weights[dirtLayer] < 0.95f)
                    blended++;
            });
            Assert.GreaterOrEqual(blended, 12, "Le bord se fond tout autour du cercle, sans marches de texel.");
        }

        [UnityTest]
        public IEnumerator PavingsAreBoundToTheGrassByTheirBinder()
        {
            FloorDefinition paving = Floor("paves", _binder, 0f);
            yield return Build(paving);
            int pavingLayer = 1, binderLayer = 2;
            Assert.AreEqual(3, _copy.alphamapLayers, "Les pavés et leur liant s'ajoutent à la copie.");
            ForEachTexel((distance, weights) =>
            {
                if (distance < Radius - 0.45f)
                    Assert.GreaterOrEqual(weights[pavingLayer], 0.95f, "Dedans : les pavés.");
                else if (distance > Radius + 0.2f && distance < Radius + 0.5f)
                    Assert.GreaterOrEqual(weights[binderLayer], 0.8f, "Juste autour, à " + distance + " m : le liant.");
                else if (distance > Radius + 1.5f)
                    Assert.GreaterOrEqual(weights[0], 0.99f, "Plus loin, à " + distance + " m : l'herbe.");
            });
        }

        [UnityTest]
        public IEnumerator AFrayingEdgeWandersAroundItsCircle()
        {
            FloorDefinition dirt = Floor("terre", null, FloorDefinition.MaxFrayMeters);
            yield return Build(dirt);
            int wandering = 0;
            ForEachTexel((distance, weights) =>
            {
                bool dirtHere = weights[1] >= 0.5f;
                if (dirtHere && distance > Radius + 0.3f || !dirtHere && distance < Radius - 0.3f)
                    wandering++;
            });
            Assert.Greater(wandering, 0, "Le bord s'écarte de son cercle de plus de 30 cm, ici ou là.");
        }

        // Trace un disque de ce sol au milieu choisi, par l'ordre du joueur, et le fait bâtir.
        private IEnumerator Build(FloorDefinition floor)
        {
            World world = _town.World;
            _town.KnowsFloors(floor);
            world.Submit(new TraceFloorCommand(new GroundDisc(Center, Radius), floor, FloorScope.Anywhere));
            for (int step = 0; step < MaxSteps && (step == 0 || world.ConstructionSites.Queue.Count > 0); step++)
                world.Step();
            Assert.IsEmpty(world.ConstructionSites.Queue, "Le sol est bâti.");
            yield return null;
        }

        // Chaque texel à moins de 6 m du milieu : sa distance au milieu, et ses poids, qui font un.
        private void ForEachTexel(System.Action<float, float[]> check)
        {
            int resolution = _copy.alphamapResolution;
            float[,,] weights = _copy.GetAlphamaps(0, 0, resolution, resolution);
            var texel = new float[weights.GetLength(2)];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    var point = new Vector2((x + 0.5f) * TexelMeters, (y + 0.5f) * TexelMeters);
                    float distance = Vector2.Distance(point, Center);
                    if (distance > 6f)
                        continue;
                    float sum = 0f;
                    for (int layer = 0; layer < texel.Length; layer++)
                    {
                        texel[layer] = weights[y, x, layer];
                        sum += texel[layer];
                    }
                    Assert.AreEqual(1f, sum, 1e-3f, "Les poids d'un texel font un.");
                    check(distance, texel);
                }
            }
        }

        private TerrainLayer Layer(string name)
        {
            TerrainLayer layer = Keep(new TerrainLayer { name = name });
            layer.diffuseTexture = Keep(new Texture2D(4, 4));
            return layer;
        }

        // Un sol de ce calque, de ce liant (null : aucun) large de 80 cm, et qui s'effiloche de tant.
        private FloorDefinition Floor(string id, TerrainLayer binder, float fray)
        {
            var floor = Keep(ScriptableObject.CreateInstance<FloorDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(floor, "_id", id);
            TestWorld.Fill(floor, "_layer", Layer(id));
            TestWorld.Fill(floor, "_borderLayer", binder);
            TestWorld.Fill(floor, "_borderMeters", 0.8f);
            TestWorld.Fill(floor, "_frayMeters", fray);
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
