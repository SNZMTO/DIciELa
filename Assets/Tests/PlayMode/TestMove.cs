using Diciela.Data;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les gestes de la pose et de l'outil Déplacer sur le petit monde des tests, partagés par leurs tests : poser une
    /// construction par l'outil de pose, et toucher le modèle d'une construction, l'outil Déplacer en main.
    /// </summary>
    internal static class TestMove
    {
        /// <summary>Pose cette construction ici, par l'outil de pose, puis le repose ; rend ce qui est posé.</summary>
        public static Structure Place(TestWorld test, StructureDefinition definition, Vector3 ground)
        {
            test.Tool.Begin(definition);
            test.Tool.AimAt(test.OnScreen(ground));
            Assert.IsTrue(test.Tool.Confirm(), "La place est libre.");
            test.World.Step();
            test.Tool.Cancel();
            return test.World.Structures.All[test.World.Structures.All.Count - 1];
        }

        /// <summary>Vise le modèle d'une construction, à cette hauteur au-dessus de ce point du sol, et clique.</summary>
        public static bool Touch(TestWorld test, Vector3 ground, float height)
        {
            test.Mover.AimAt(test.OnScreen(ground + new Vector3(0f, height, 0f)));
            return test.Mover.Confirm();
        }
    }
}
