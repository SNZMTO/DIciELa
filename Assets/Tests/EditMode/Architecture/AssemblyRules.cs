using System.Collections.Generic;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Le tableau des assemblages de ARCHITECTURE.md, section 2 : leur dossier et ce qu'ils ont le droit de
    /// référencer. Changer ce tableau est un changement d'architecture : ARCHITECTURE.md et une décision d'abord.
    /// </summary>
    internal static class AssemblyRules
    {
        private static readonly string[] SimulationLayers =
        {
            "Diciela.Simulation.Core", "Diciela.Simulation.Land", "Diciela.Simulation.Economy",
            "Diciela.Simulation.Society", "Diciela.Simulation"
        };

        private static readonly string[] TestRunner = { "UnityEngine.TestRunner", "UnityEditor.TestRunner" };

        public sealed class Rule
        {
            public string Folder;
            public string[] Diciela;
            public string[] External = new string[0];
            public string[] Precompiled = new string[0];
        }

        public static readonly IReadOnlyDictionary<string, Rule> Rules = new Dictionary<string, Rule>
        {
            ["Diciela.Data"] = new Rule { Folder = "Assets/Scripts/Data", Diciela = new string[0] },
            ["Diciela.Simulation.Core"] = new Rule
                { Folder = "Assets/Scripts/Simulation/Core", Diciela = new[] { "Diciela.Data" } },
            ["Diciela.Simulation.Land"] = new Rule
                { Folder = "Assets/Scripts/Simulation/Land", Diciela = Layers(1) },
            ["Diciela.Simulation.Economy"] = new Rule
                { Folder = "Assets/Scripts/Simulation/Economy", Diciela = Layers(2) },
            ["Diciela.Simulation.Society"] = new Rule
                { Folder = "Assets/Scripts/Simulation/Society", Diciela = Layers(3) },
            ["Diciela.Simulation"] = new Rule { Folder = "Assets/Scripts/Simulation", Diciela = Layers(4) },
            ["Diciela.Persistence"] = new Rule
            {
                Folder = "Assets/Scripts/Persistence", Diciela = Layers(5),
                Precompiled = new[] { "Newtonsoft.Json.dll" }
            },
            ["Diciela.View"] = new Rule { Folder = "Assets/Scripts/View", Diciela = Layers(5) },
            ["Diciela.Controls"] = new Rule
            {
                Folder = "Assets/Scripts/Controls", Diciela = With(Layers(5), "Diciela.View"),
                External = new[] { "Unity.InputSystem" }
            },
            ["Diciela.UI"] = new Rule
            {
                Folder = "Assets/Scripts/UI",
                Diciela = With(Layers(5), "Diciela.View", "Diciela.Controls", "Diciela.Persistence")
            },
            ["Diciela.Game"] = new Rule
            {
                Folder = "Assets/Scripts/Game",
                Diciela = With(Layers(5), "Diciela.Persistence", "Diciela.View", "Diciela.Controls", "Diciela.UI"),
                External = new[] { "Unity.InputSystem" }
            },
            ["Diciela.Scenarios"] = new Rule { Folder = "Assets/Scripts/Scenarios", Diciela = Layers(5) },
            ["Diciela.EditorTools"] = new Rule
            {
                Folder = "Assets/Scripts/Editor",
                Diciela = With(Layers(5), "Diciela.Persistence", "Diciela.View", "Diciela.Game", "Diciela.Scenarios")
            },
            ["Diciela.Tests.EditMode"] = new Rule
            {
                Folder = "Assets/Tests/EditMode",
                Diciela = With(Layers(5), "Diciela.Persistence", "Diciela.Scenarios"),
                External = TestRunner, Precompiled = new[] { "nunit.framework.dll" }
            },
            ["Diciela.Tests.PlayMode"] = new Rule
            {
                Folder = "Assets/Tests/PlayMode",
                Diciela = With(Layers(5), "Diciela.Persistence", "Diciela.View", "Diciela.Controls", "Diciela.UI",
                    "Diciela.Game"),
                External = With(TestRunner, "Unity.InputSystem"), Precompiled = new[] { "nunit.framework.dll" }
            },
        };

        /// <summary>Data et les <paramref name="count"/> premières couches de la simulation.</summary>
        private static string[] Layers(int count)
        {
            var names = new List<string> { "Diciela.Data" };
            for (int i = 0; i < count; i++)
                names.Add(SimulationLayers[i]);
            return names.ToArray();
        }

        private static string[] With(string[] names, params string[] more)
        {
            var all = new List<string>(names);
            all.AddRange(more);
            return all.ToArray();
        }
    }
}
