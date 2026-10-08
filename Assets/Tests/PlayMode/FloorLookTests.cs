using System.Collections;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'aspect des sols peints (décisions 227 et 232), par le chemin du joueur, sur le banc des sols (TestFloorBench)
    /// peint à 128 texels de côté (31,25 cm chacun) : un disque de 3 m de rayon, tracé puis bâti par la Chimère. Un sol
    /// au bord lisse se fond sur la largeur de son fondu, centrée sur son bord (mi-terre mi-herbe en moyenne sur le
    /// cercle, plus de cent texels mêlés autour, rien de mêlé au-delà d'un demi-fondu) ; des pavés se fondent dans leur
    /// liant, qui se fond à son tour dans l'herbe au bout de sa bande ; un bord qui s'effiloche s'écarte de son cercle
    /// ici et là. Le disque est fait de mailles d'un demi-mètre : on laisse 30 cm entre son cercle et ses mailles. Tous
    /// les poids d'un texel font un.
    /// </summary>
    public sealed class FloorLookTests
    {
        private const int Resolution = 128;
        private const float Radius = 3f;
        private const float TexelMeters = TestFloorBench.SideMeters / Resolution;
        private const float Tolerance = 0.3f;
        private static readonly Vector2 Center = new Vector2(20f, 30f);
        private TestFloorBench _bench;
        private TerrainLayer _binder;

        [SetUp]
        public void Create()
        {
            _bench = new TestFloorBench(Resolution);
            _binder = _bench.Layer("liant");
        }

        [TearDown]
        public void Destroy()
        {
            _bench.Dispose();
        }

        [UnityTest]
        public IEnumerator ASmoothFloorFadesWideAroundItsEdge()
        {
            const float fade = 1f;
            FloorDefinition dirt = Floor("terre", null, 0f, fade);
            yield return Build(dirt);
            int dirtLayer = 1, blended = 0, onEdge = 0;
            float edgeSum = 0f;
            ForEachTexel((distance, weights) =>
            {
                float share = weights[dirtLayer];
                if (distance < Radius - fade / 2f - Tolerance)
                    Assert.GreaterOrEqual(share, 0.95f, "Dedans, à " + distance + " m du milieu : la terre.");
                else if (distance > Radius + fade / 2f + Tolerance)
                    Assert.LessOrEqual(share, 0.05f, "Dehors, à " + distance + " m du milieu : l'herbe.");
                if (Mathf.Abs(distance - Radius) < 0.15f)
                {
                    edgeSum += share;
                    onEdge++;
                }
                if (share > 0.05f && share < 0.95f)
                    blended++;
            });
            Assert.Greater(onEdge, 0, "Des texels sur le cercle.");
            Assert.That(edgeSum / onEdge, Is.InRange(0.35f, 0.65f),
                "Sur le cercle, mi-terre mi-herbe en moyenne : le fondu est centré sur le bord.");
            Assert.GreaterOrEqual(blended, 100, "Le fondu est large : plus de cent texels mêlés autour du cercle.");
        }

        [UnityTest]
        public IEnumerator PavingsFadeIntoTheirBinderThenTheBinderIntoTheGrass()
        {
            const float fade = 0.5f, width = 1.5f, binderFade = 1f;
            FloorDefinition paving = Floor("paves", _binder, 0f, fade, width, binderFade);
            yield return Build(paving);
            int pavingLayer = 1, binderLayer = 2;
            Assert.AreEqual(3, _bench.Copy.alphamapLayers, "Les pavés et leur liant s'ajoutent à la copie.");
            ForEachTexel((distance, weights) =>
            {
                float edge = distance - Radius;
                if (edge < -fade / 2f - Tolerance)
                    Assert.GreaterOrEqual(weights[pavingLayer], 0.95f, "Dedans : les pavés.");
                else if (Mathf.Abs(edge) < 0.1f)
                    Assert.LessOrEqual(weights[0], 0.05f, "Sur leur bord, les pavés se fondent dans leur liant.");
                else if (edge > fade / 2f + Tolerance && edge < width - binderFade / 2f - Tolerance)
                    Assert.GreaterOrEqual(weights[binderLayer], 0.8f, "Autour, à " + distance + " m : le liant.");
                else if (Mathf.Abs(edge - width) < 0.1f)
                    Assert.That(weights[0], Is.InRange(0.1f, 0.9f), "Au bout du liant, il se fond dans l'herbe.");
                else if (edge > width + binderFade / 2f + Tolerance)
                    Assert.GreaterOrEqual(weights[0], 0.99f, "Plus loin, à " + distance + " m : l'herbe.");
            });
        }

        [UnityTest]
        public IEnumerator AFrayingEdgeWandersAroundItsCircle()
        {
            FloorDefinition dirt = Floor("terre", null, FloorDefinition.MaxFrayMeters, 1f);
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
            World world = _bench.World;
            _bench.Town.KnowsFloors(floor);
            world.Submit(new TraceFloorCommand(new GroundDisc(Center, Radius), floor, FloorScope.Anywhere));
            world.Step();
            _bench.BuildAll();
            yield return null;
        }

        // Chaque texel à moins de 6 m du milieu : sa distance au milieu, et ses poids, qui font un.
        private void ForEachTexel(System.Action<float, float[]> check)
        {
            float[,,] weights = _bench.Copy.GetAlphamaps(0, 0, Resolution, Resolution);
            var texel = new float[weights.GetLength(2)];
            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
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

        // Un sol de son propre calque, qui s'effiloche de tant et se fond sur tant ; de ce liant (null : aucun), de
        // cette largeur, qui se fond sur tant.
        private FloorDefinition Floor(string id, TerrainLayer binder, float fray, float fade, float width = 0.8f,
            float binderFade = 1f)
        {
            FloorDefinition floor = _bench.Floor(id, _bench.Layer(id));
#if UNITY_EDITOR
            TestWorld.Fill(floor, "_borderLayer", binder);
            TestWorld.Fill(floor, "_borderMeters", width);
            TestWorld.Fill(floor, "_borderFadeMeters", binderFade);
            TestWorld.Fill(floor, "_frayMeters", fray);
            TestWorld.Fill(floor, "_fadeMeters", fade);
#endif
            return floor;
        }
    }
}
