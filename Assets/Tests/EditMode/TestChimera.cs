using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le banc des tests de la Chimère : les réglages du jeu (un pas de 0,1 s, une journée de 720 s, soit 300 pas par
    /// heure de jeu), mais deux mailles par mètre ; sa sorte : 1,5 m/s (15 cm par pas), flânerie à 6 m au plus, pauses
    /// de 0,1 à 0,3 h (30 à 90 pas) ; son apparence : une tête et un corps d'une même couleur, sans pièce. Ses mondes sont plats, et elle
    /// y est créée par son ordre. Les fiches qu'il crée sont détruites avec lui.
    /// </summary>
    internal sealed class TestChimera : IDisposable
    {
        public const int CellsPerMeter = 2;
        public const float MetersPerStep = 0.15f;

        private readonly List<Object> _created = new List<Object>();

        public TestChimera()
        {
            Settings = Keep(TestData.Settings(0.1f, 720f, 2f / 3f, 1f / 12f, cellsPerMeter: CellsPerMeter));
            Kind = Keep(TestData.VillagerKind());
            PaletteDefinition palette = Keep(TestData.Palette("roux"));
            Appearance = new Appearance(Keep(TestData.Head("chat", palette)), palette, Keep(TestData.Body("chat", palette)),
                palette, Array.Empty<WornAttachment>());
        }

        public SimulationSettings Settings { get; }
        public VillagerKindDefinition Kind { get; }
        public Appearance Appearance { get; }

        /// <summary>
        /// Un monde plat de size mètres, où l'ordre de la créer vient d'être joué : elle y commence sa première pause.
        /// Sans point d'arrivée, elle naît au centre ; sur la carte de 40 m, en (20,25 ; 20,25).
        /// </summary>
        public World World(float size = 40f, Vector2? arrival = null)
        {
            var world = new World(Settings, TestData.FlatMap(size, arrival), TestData.EmptyCatalog);
            world.Submit(new CreateChimeraCommand(Kind, Appearance));
            world.Step();
            return world;
        }

        /// <summary>Joue jusqu'à ce qu'elle parte vers un point à 4 m au moins : le pas où elle vient de le tirer.</summary>
        public static Villager StartALongWalk(World world)
        {
            Villager chimera = world.Villagers.Chimera;
            TestGrids.StepUntil(world, () => chimera.IsWalking && chimera.Path.Length >= 4f &&
                                             chimera.Position == chimera.PreviousPosition, 20000, "un départ de 4 m");
            return chimera;
        }

        /// <summary>Une construction sans porte de width × depth mètres, détruite avec le banc.</summary>
        public StructureDefinition Block(float width, float depth)
        {
            return Keep(TestData.Structure("bloc", width, depth, hasDoor: false));
        }

        /// <summary>Garde une fiche créée pour le test, détruite avec le banc.</summary>
        public T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        public void Dispose()
        {
            foreach (Object created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }
    }
}
