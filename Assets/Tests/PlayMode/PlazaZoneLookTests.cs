using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La Place vue sans escalier (décision 227), sur le petit monde des tests : une zone ronde de 3 m de rayon, tracée
    /// par l'ordre du joueur, se voit en voile et en contour lissés. Un escalier de mailles n'aurait de sommets qu'aux
    /// coins des mailles, tous les 25 cm ; le voile lissé en a entre eux, là où son bord coupe une maille, et il ne
    /// déborde pas du cercle.
    /// </summary>
    public sealed class PlazaZoneLookTests
    {
        private const float Radius = 3f;
        private static readonly Vector2 Center = new Vector2(50f, 50f);

        [Test]
        public void TheZoneIsSeenSmoothNotInSteps()
        {
            using (var test = new TestWorld(withPlaza: true))
            {
                var settings = test.Keep(ScriptableObject.CreateInstance<PlacementSettings>());
                var zone = test.Keep(new GameObject("Place de test")).AddComponent<PlazaZoneView>();
#if UNITY_EDITOR
                TestWorld.Fill(settings, "_translucentMaterial",
                    test.Keep(new Material(Shader.Find("Universal Render Pipeline/Lit"))));
                TestWorld.Fill(zone, "_settings", settings);
#endif
                test.World.Submit(new PaintPlazaCommand(new GroundDisc(Center, Radius)));
                test.World.Step();
                zone.Show(test.World);
                zone.SetShown(true);

                Mesh veilMesh = zone.transform.Find("Voile de la Place").GetComponent<MeshFilter>().sharedMesh;
                Vector3[] veil = veilMesh.vertices;
                float cell = test.World.Grid.Metrics.CellSize;
                int between = 0;
                foreach (Vector3 vertex in veil)
                {
                    Assert.LessOrEqual(Vector2.Distance(new Vector2(vertex.x, vertex.z), Center), Radius + 0.3f,
                        "Le voile ne déborde pas de son cercle.");
                    if (OffTheCorners(vertex.x, cell) || OffTheCorners(vertex.z, cell))
                        between++;
                }
                Assert.Greater(between, veil.Length / 10, "Le bord du voile coupe les mailles : pas d'escalier.");
                Assert.Greater(zone.transform.Find("Lignes de la Place").GetComponent<MeshFilter>().sharedMesh
                    .vertexCount, 0, "Le contour se voit.");
            }
        }

        // Vrai si cette coordonnée n'est pas sur un coin de maille.
        private static bool OffTheCorners(float meters, float cell)
        {
            float steps = meters / cell;
            return Mathf.Abs(steps - Mathf.Round(steps)) > 0.05f;
        }
    }
}
