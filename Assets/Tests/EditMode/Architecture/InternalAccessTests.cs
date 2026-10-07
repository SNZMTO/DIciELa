using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AssembliesType = UnityEditor.Compilation.AssembliesType;
using CompilationPipeline = UnityEditor.Compilation.CompilationPipeline;
using CompiledAssembly = UnityEditor.Compilation.Assembly;
using NUnit.Framework;

namespace Diciela.Tests.EditMode.Architecture
{
    /// <summary>
    /// Seule la simulation change la simulation (ARCHITECTURE, section 5) : ce qui modifie un système est internal, et
    /// seules les couches de la simulation ouvrent leurs internals, aux seules couches de la simulation. Ni la vue, ni
    /// les gestes, ni les tests ne reçoivent de porte dérobée. Ces tests lisent le code compilé, sans rien écrire : les
    /// ouvertures (InternalsVisibleTo) se lisent sur les assemblages eux-mêmes, quelle que soit leur écriture.
    /// </summary>
    public sealed class InternalAccessTests
    {
        private const string SimulationPrefix = "Diciela.Simulation";

        [Test]
        public void WhatChangesTheSimulationStaysInternal()
        {
            var faults = new List<string>();
            foreach ((Type owner, string name) in SimulationMutators.All)
            {
                // Toutes les formes d'une fonction : une seconde forme publique ouvrirait la même porte.
                int found = 0;
                foreach (MethodInfo method in owner.GetMethods(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.Name != name)
                        continue;
                    found++;
                    if (!method.IsAssembly)
                        faults.Add(owner.Name + "." + name + " doit rester internal : au-dessus, on envoie des ordres.");
                }
                if (found == 0)
                    faults.Add(owner.Name + "." + name + " est introuvable.");
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        [Test]
        public void OnlyTheSimulationOpensItsInternalsAndOnlyToItself()
        {
            var declarations = new List<string>();
            var faults = new List<string>();
            // Les assemblages qu'Unity compile pour l'éditeur, ceux du jeu compris : AppDomain en garderait de déchargés.
            foreach (CompiledAssembly compiled in CompilationPipeline.GetAssemblies(AssembliesType.Editor))
            {
                string name = compiled.name;
                if (!name.StartsWith("Diciela."))
                    continue;
                Assembly assembly = Assembly.Load(name);
                foreach (var opening in assembly.GetCustomAttributes<InternalsVisibleToAttribute>())
                {
                    // Le nom de l'assemblage ouvert, sans sa clé publique s'il en a une.
                    string target = opening.AssemblyName.Split(',')[0].Trim();
                    declarations.Add(target);
                    if (!IsSimulation(name) || !IsSimulation(target))
                        faults.Add(name + " ouvre ses internals à " + target + ".");
                }
            }

            Assert.IsNotEmpty(declarations,
                "Aucune déclaration InternalsVisibleTo trouvée : le socle et le terrain en ont une.");
            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static bool IsSimulation(string assembly)
        {
            return assembly == SimulationPrefix || assembly.StartsWith(SimulationPrefix + ".");
        }
    }
}
