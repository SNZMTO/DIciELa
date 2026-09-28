using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// La simulation reste du C# pur : ce test lit son code, sans rien écrire (ARCHITECTURE, section 12).
    /// Les premiers tests vérifient l'analyseur lui-même sur des exemples écrits à la main.
    /// </summary>
    public sealed class SimulationPurityTests
    {
        [Test]
        public void ScannerFindsForbiddenUses()
        {
            const string source = "class A : MonoBehaviour\n{\n  float t = Time.deltaTime;\n  int r = Random.Range(0, 3);\n}";

            List<string> violations = SourceScanner.FindViolations(source);

            Assert.AreEqual(3, violations.Count, string.Join("\n", violations));
        }

        [Test]
        public void ScannerIgnoresCommentsAndStrings()
        {
            const string source = "// jamais de MonoBehaviour ici\n/* ni Time.deltaTime */\nstring s = \"Random\";";

            Assert.IsEmpty(SourceScanner.FindViolations(source));
        }

        [Test]
        public void ScannerFindsMutableStaticFieldsOnly()
        {
            const string source = "private static int _count;\n" +
                                  "private static readonly int Max = 3;\n" +
                                  "public static int Twice(int x) { return 2 * x; }\n" +
                                  "public static event System.Action Happened;\n" +
                                  "public static int Three => 3;";

            List<string> violations = SourceScanner.FindViolations(source);

            Assert.AreEqual(new[] { "ligne 1 : champ statique modifiable", "ligne 4 : champ statique modifiable" },
                violations);
        }

        [Test]
        public void ScannerFindsWhatASheetMustNotGive()
        {
            // ARCHITECTURE, section 9 : une fiche ne donne que des nombres, et son identifiant est Id, jamais .name.
            const string source = "AnimationCurve courbe;\nGradient couleurs;\nstring n = fiche.name;\nstring m = fiche.Name;";

            List<string> violations = SourceScanner.FindViolations(source);

            Assert.AreEqual(new[]
            {
                "ligne 1 : AnimationCurve ou Gradient (une fiche ne donne que des nombres)",
                "ligne 2 : AnimationCurve ou Gradient (une fiche ne donne que des nombres)",
                "ligne 3 : .name (l'identifiant d'une fiche est Id)"
            }, violations);
        }

        [Test]
        public void SimulationCodeUsesNothingForbidden()
        {
            string simulationFolder = Path.Combine(Application.dataPath, "Scripts", "Simulation");
            string[] files = Directory.GetFiles(simulationFolder, "*.cs", SearchOption.AllDirectories);
            Assert.IsNotEmpty(files, "Aucun fichier de simulation trouvé dans " + simulationFolder);

            var report = new List<string>();
            foreach (string file in files)
            {
                foreach (string violation in SourceScanner.FindViolations(File.ReadAllText(file)))
                    report.Add(Path.GetFileName(file) + ", " + violation);
            }

            Assert.IsEmpty(report, string.Join("\n", report));
        }
    }
}
