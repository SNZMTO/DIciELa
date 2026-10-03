using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Un sol qu'on traverse, vu (décision 193), sur un petit monde en pente : 100 × 100 m, le sol à 10 m à l'ouest et à
    /// 20 m à l'est, il monte d'un dixième de mètre par mètre. Le sol posé par l'ordre du joueur en (50 ; 50) a pour
    /// modèle le plan d'Unity (10 × 10 m, onze sommets de côté, à plat sur son origine) : sur son chantier, chacun de ses
    /// sommets est à la hauteur du relief sous lui (10 m plus un dixième de son x), et son fantôme aussi ; une cabane,
    /// elle, reste d'un bloc. Le maillage du modèle ne change pas.
    /// </summary>
    public sealed class FloorViewsTests
    {
        private TestWorld _test;

        [SetUp]
        public void CreateWorld()
        {
            _test = new TestWorld(new Relief(100f, 100f, 2, 2, new[] { 10f, 20f, 10f, 20f }));
        }

        [TearDown]
        public void DestroyWorld()
        {
            _test.Dispose();
        }

        [Test]
        public void APlannedFloorAndItsGhostFollowTheSlope()
        {
            GameObject plane = _test.Keep(GameObject.CreatePrimitive(PrimitiveType.Plane));
            Mesh original = plane.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] before = original.vertices;
            StructureDefinition floor = Floor(plane);

            _test.World.Submit(new PlaceStructureCommand(floor, new Placement(new Vector2(50f, 50f), 0f)));
            _test.World.Step();

            Transform view = _test.Views.transform.GetChild(0);
            AssertFollowsTheSlope(view.GetComponent<MeshFilter>(), "Le chantier");
            Transform ghost = view.Find("Fantôme");
            Assert.IsNotNull(ghost, "Le fantôme est rangé sous le sol.");
            AssertFollowsTheSlope(ghost.GetComponent<MeshFilter>(), "Son fantôme");
            CollectionAssert.AreEqual(before, original.vertices, "Le maillage du modèle ne change pas.");
        }

        [Test]
        public void ACabinStaysInOnePiece()
        {
            _test.World.Submit(new PlaceStructureCommand(_test.Cabin, new Placement(new Vector2(50f, 50f), 0f)));
            _test.World.Step();

            MeshFilter cube = _test.Views.transform.GetChild(0).GetComponent<MeshFilter>();
            Assert.AreSame(_test.Cabin.Model.GetComponent<MeshFilter>().sharedMesh, cube.sharedMesh,
                "Une construction qu'on ne traverse pas garde le maillage de son modèle.");
        }

        private StructureDefinition Floor(GameObject model)
        {
            var floor = _test.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(floor, "_id", "place");
            TestWorld.Fill(floor, "_widthMeters", 10f);
            TestWorld.Fill(floor, "_depthMeters", 10f);
            TestWorld.Fill(floor, "_hasDoor", false);
            TestWorld.Fill(floor, "_walkable", true);
            TestWorld.Fill(floor, "_model", model);
#endif
            return floor;
        }

        // Chaque sommet est à la hauteur de la pente sous lui : 10 m plus un dixième de son x.
        private static void AssertFollowsTheSlope(MeshFilter filter, string what)
        {
            Vector3[] vertices = filter.sharedMesh.vertices;
            Assert.AreEqual(121, vertices.Length, what + " : le plan a onze sommets de côté.");
            foreach (Vector3 vertex in vertices)
            {
                Vector3 world = filter.transform.TransformPoint(vertex);
                Assert.AreEqual(10f + 0.1f * world.x, world.y, 1e-3f, what + " : le sommet en x = " + world.x + ".");
            }
        }
    }
}
