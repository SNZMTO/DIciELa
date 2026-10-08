using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Les chantiers du petit monde des tests, posés par l'ordre du joueur.</summary>
    internal static class TestSites
    {
        /// <summary>
        /// Pose, par l'ordre du joueur, une haie de 2 × 2 m en (70 ; 70), dont le modèle est un quad vêtu de ce
        /// matériau : un chantier ; rend sa vue, la dernière des constructions vues.
        /// </summary>
        public static Transform PlaceASiteDressedIn(this TestWorld test, Material material)
        {
            GameObject model = test.Keep(GameObject.CreatePrimitive(PrimitiveType.Quad));
            model.GetComponent<MeshRenderer>().sharedMaterial = material;
            var hedge = test.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(hedge, "_id", "haie");
            TestWorld.Fill(hedge, "_widthMeters", 2f);
            TestWorld.Fill(hedge, "_depthMeters", 2f);
            TestWorld.Fill(hedge, "_model", model);
#endif
            test.World.Submit(new PlaceStructureCommand(hedge, new Placement(new Vector2(70f, 70f), 0f)));
            test.World.Step();
            return test.Views.transform.GetChild(test.Views.transform.childCount - 1);
        }
    }
}
