using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Seule la simulation change la simulation (ARCHITECTURE, section 5) : ce qui modifie un système est internal, et
    /// seules les couches de la simulation ouvrent leurs internals, aux seules couches de la simulation. Ni la vue, ni
    /// les gestes, ni les tests ne reçoivent de porte dérobée. Ce test lit le code, sans rien écrire.
    /// </summary>
    public sealed class InternalAccessTests
    {
        private const string SimulationPrefix = "Diciela.Simulation";

        private static readonly Regex Declaration = new Regex(
            @"\[\s*assembly\s*:\s*(?:System\.Runtime\.CompilerServices\.)?InternalsVisibleTo\s*\(\s*""([^""]+)""");

        [Test]
        public void OnlyTheSimulationOpensItsInternalsAndOnlyToItself()
        {
            string scriptsFolder = Normalized(Path.Combine(Application.dataPath, "Scripts"));
            string simulationFolder = scriptsFolder + "/Simulation/";
            var declarations = new List<string>();
            var faults = new List<string>();

            foreach (string file in Directory.GetFiles(scriptsFolder, "*.cs", SearchOption.AllDirectories))
            {
                string path = Normalized(file);
                foreach (Match match in Declaration.Matches(File.ReadAllText(file)))
                {
                    string target = match.Groups[1].Value;
                    declarations.Add(target);
                    if (!path.StartsWith(simulationFolder) || !target.StartsWith(SimulationPrefix))
                        faults.Add(path.Substring(scriptsFolder.Length + 1) + " ouvre ses internals à " + target + ".");
                }
            }

            Assert.IsNotEmpty(declarations,
                "Aucune déclaration InternalsVisibleTo trouvée : le socle et le terrain en ont une.");
            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static string Normalized(string path)
        {
            return Path.GetFullPath(path).Replace('\\', '/');
        }
    }
}
