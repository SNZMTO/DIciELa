using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le banc de la ville au travail : les réglages du banc de la Chimère (un pas de 0,1 s, 300 pas par heure de jeu,
    /// deux mailles par mètre, 1 m libre devant une porte), ou une journée plus courte ; deux statistiques de deux
    /// sous-statistiques (Bâtisse : construction, terrassement ; Échange : transport, troc) ; le Bois et la Pierre ;
    /// porter (transport) et bâtir (construction) ; le poste « à tout faire » (porter, puis bâtir) ; un tas de départ de
    /// 2 × 2 m qui tient 50 marchandises et ouvre ce poste, posé avec 30 Bois et 20 Pierre ; la tente de 3 × 3 m ; une
    /// cabane de 4 × 4 m qui coûte 10 Bois et 5 Pierre et 2 h de travail. La Chimère commence à 10 partout et porte 5
    /// marchandises. Ses mondes sont plats, 40 × 40 m ; les fiches qu'il crée sont détruites avec lui.
    /// </summary>
    internal sealed class TestTown : IDisposable
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly TestChimera _chimera = new TestChimera();

        public TestTown(float dayLengthSeconds = 720f)
        {
            Settings = Keep(TestData.Settings(0.1f, dayLengthSeconds, 2f / 3f, 1f / 12f,
                cellsPerMeter: TestChimera.CellsPerMeter));
            Wood = Keep(TestData.Good("bois"));
            Stone = Keep(TestData.Good("pierre"));
            SubStatisticDefinition construction = Keep(TestData.SubStatistic("construction"));
            SubStatisticDefinition transport = Keep(TestData.SubStatistic("transport"));
            var building = Keep(TestData.Statistic("batisse", construction,
                Keep(TestData.SubStatistic("terrassement"))));
            var trade = Keep(TestData.Statistic("echange", transport, Keep(TestData.SubStatistic("troc"))));
            Carry = Keep(TestData.TaskKind("porter", TaskWork.Carry, transport));
            Build = Keep(TestData.TaskKind("batir", TaskWork.Build, construction));
            OddJobs = Keep(TestData.WorkSlot("a_tout_faire", Carry, Build));
            Pile = Keep(TestData.Pile("tas", 2f, 50, OddJobs));
            Tent = Keep(TestData.Home());
            Cabin = Keep(TestData.Structure("cabane", 4f, 4f));
            TestData.Cost(Cabin, 2f, new GoodAmount(Wood, 10), new GoodAmount(Stone, 5));
            Opening = Keep(TestData.Opening(Pile, new GoodAmount(Wood, 30), new GoodAmount(Stone, 20)));
            Catalog = Keep(TestData.TownCatalog(Opening, new[] { Wood, Stone }, new[] { building, trade }));
        }

        public SimulationSettings Settings { get; }
        public GoodDefinition Wood { get; }
        public GoodDefinition Stone { get; }
        public TaskKindDefinition Carry { get; }
        public TaskKindDefinition Build { get; }
        public WorkSlotDefinition OddJobs { get; }
        public StructureDefinition Pile { get; }
        public StructureDefinition Tent { get; }
        public StructureDefinition Cabin { get; }
        public OpeningDefinition Opening { get; }
        public Catalog Catalog { get; }

        /// <summary>Un monde plat de 40 m où la Chimère vient de naître au centre, sans rien de posé.</summary>
        public World World(Catalog catalog = null)
        {
            var world = new World(Settings, TestData.FlatMap(40f), catalog ?? Catalog);
            world.Submit(new CreateChimeraCommand(_chimera.Kind, _chimera.Appearance));
            world.Step();
            return world;
        }

        /// <summary>
        /// Un monde où la tente est plantée en (20 ; 14), porte au nord (le tas se pose donc de lui-même), et où la
        /// Chimère est mise au tas, par les ordres du joueur.
        /// </summary>
        public World WorkingWorld()
        {
            World world = World();
            Place(world, Tent, 20f, 14f);
            Structure pile = PileOf(world);
            world.Submit(new AssignVillagerCommand(world.Villagers.Chimera.Id, pile.Id));
            world.Step();
            return world;
        }

        /// <summary>Pose cette construction ici, à l'angle 0, par l'ordre « poser », et la rend.</summary>
        public static Structure Place(World world, StructureDefinition definition, float x, float z, float angle = 0f)
        {
            world.Submit(new PlaceStructureCommand(definition, new Placement(new Vector2(x, z), angle)));
            world.Step();
            Structure placed = null;
            foreach (Structure structure in world.Structures.All)
            {
                if (ReferenceEquals(structure.Definition, definition))
                    placed = structure;
            }
            return placed;
        }

        /// <summary>Le tas de départ de ce monde ; null s'il n'est pas posé.</summary>
        public Structure PileOf(World world)
        {
            foreach (Structure structure in world.Structures.All)
            {
                if (ReferenceEquals(structure.Definition, Pile))
                    return structure;
            }
            return null;
        }

        /// <summary>Les marchandises de la ville, où qu'elles soient : entrepôts, charges, chantiers.</summary>
        public static int EverywhereOf(World world, GoodDefinition good)
        {
            int count = world.Warehouses.CityStockOf(good);
            foreach (Villager villager in world.Villagers.All)
                count += villager.Load.CountOf(good);
            foreach (ConstructionSite site in world.ConstructionSites.Queue)
            {
                for (int line = 0; line < site.LineCount; line++)
                {
                    if (ReferenceEquals(site.GoodAt(line), good))
                        count += site.DeliveredAt(line);
                }
            }
            return count;
        }

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
            _chimera.Dispose();
        }
    }
}
