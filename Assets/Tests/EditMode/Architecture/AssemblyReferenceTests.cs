using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Les assemblages du projet suivent le tableau de ARCHITECTURE.md (section 2) : un assemblage inconnu, une
    /// référence de trop ou un dossier déplacé font échouer ce test. Il lit les fichiers .asmdef sans rien écrire.
    /// </summary>
    public sealed class AssemblyReferenceTests
    {
        private const string GuidPrefix = "GUID:";

        [System.Serializable]
        private sealed class AsmdefFile
        {
            public string name = "";
            public string[] references = new string[0];
            public string[] precompiledReferences = new string[0];
            public bool overrideReferences = false;
            public bool autoReferenced = true;
        }

        [Test]
        public void EveryAssemblyFollowsTheArchitecture()
        {
            string projectFolder = Path.GetDirectoryName(Application.dataPath);
            var files = new List<string>();
            files.AddRange(Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts"), "*.asmdef", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(Path.Combine(Application.dataPath, "Tests"), "*.asmdef", SearchOption.AllDirectories));
            Assert.IsNotEmpty(files, "Aucun assemblage trouvé.");

            var faults = new List<string>();
            foreach (string file in files)
                CheckAssembly(file, projectFolder, faults);

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static void CheckAssembly(string file, string projectFolder, List<string> faults)
        {
            AsmdefFile asmdef = JsonUtility.FromJson<AsmdefFile>(File.ReadAllText(file));
            if (!AssemblyRules.Rules.TryGetValue(asmdef.name, out AssemblyRules.Rule rule))
            {
                faults.Add(asmdef.name + " : assemblage absent de ARCHITECTURE.md.");
                return;
            }

            string folder = Path.GetDirectoryName(file).Substring(projectFolder.Length + 1).Replace('\\', '/');
            if (folder != rule.Folder)
                faults.Add(asmdef.name + " : dans " + folder + " au lieu de " + rule.Folder + ".");
            if (asmdef.autoReferenced)
                faults.Add(asmdef.name + " : autoReferenced doit être faux.");
            if (!asmdef.overrideReferences)
                faults.Add(asmdef.name + " : overrideReferences doit être vrai (aucune DLL d'office).");

            foreach (string reference in asmdef.references)
            {
                string name = ResolveName(reference);
                bool allowed = name.StartsWith("Diciela.")
                    ? System.Array.IndexOf(rule.Diciela, name) >= 0
                    : System.Array.IndexOf(rule.External, name) >= 0;
                if (!allowed)
                    faults.Add(asmdef.name + " : référence interdite vers " + name + ".");
            }

            foreach (string dll in asmdef.precompiledReferences)
            {
                if (System.Array.IndexOf(rule.Precompiled, dll) < 0)
                    faults.Add(asmdef.name + " : DLL interdite " + dll + ".");
            }
        }

        /// <summary>Une référence s'écrit par nom ou par GUID ; on ramène tout au nom.</summary>
        private static string ResolveName(string reference)
        {
            if (!reference.StartsWith(GuidPrefix))
                return reference;

            string path = AssetDatabase.GUIDToAssetPath(reference.Substring(GuidPrefix.Length));
            if (string.IsNullOrEmpty(path))
                return reference;
            return JsonUtility.FromJson<AsmdefFile>(File.ReadAllText(path)).name;
        }
    }
}
