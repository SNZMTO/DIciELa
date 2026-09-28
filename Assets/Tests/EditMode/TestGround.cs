using System.Collections.Generic;
using System.Text;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Un terrain d'essai des trajets : un monde sur une carte plate, où chaque obstacle est un bloc (une construction
    /// d'une maille de côté, sans porte) posé par l'ordre « poser », le chemin du joueur. Le terrain garde à part ce
    /// qu'il a bloqué, pour que l'oracle ne lise pas le maillage testé. Il se dessine en texte, le nord en haut :
    /// « # » un bloc, « S » un départ, « G » une arrivée, « . » l'herbe libre.
    /// </summary>
    internal sealed class TestGround
    {
        private TestGround(World world, bool[,] blocked, List<Vector2Int> starts, List<Vector2Int> goals)
        {
            World = world;
            Blocked = blocked;
            Starts = starts;
            Goals = goals;
        }

        public World World { get; }

        /// <summary>Les mailles bloquées, en [colonne, rangée].</summary>
        public bool[,] Blocked { get; }

        public List<Vector2Int> Starts { get; }
        public List<Vector2Int> Goals { get; }

        /// <summary>Un terrain carré dessiné en texte, une chaîne par rangée, le nord en haut.</summary>
        public static TestGround Drawn(SimulationSettings settings, StructureDefinition block, params string[] rows)
        {
            int size = rows.Length;
            var blocked = new bool[size, size];
            var starts = new List<Vector2Int>();
            var goals = new List<Vector2Int>();
            for (int row = 0; row < size; row++)
            {
                Assert.AreEqual(size, rows[row].Length, "Un terrain dessiné est carré.");
                for (int x = 0; x < size; x++)
                {
                    var cell = new Vector2Int(x, size - 1 - row);
                    switch (rows[row][x])
                    {
                        case '#': blocked[x, cell.y] = true; break;
                        case 'S': starts.Add(cell); break;
                        case 'G': goals.Add(cell); break;
                        case '.': break;
                        default: Assert.Fail("Signe inconnu dans le dessin : " + rows[row][x]); break;
                    }
                }
            }
            return Build(settings, block, blocked, starts, goals);
        }

        /// <summary>
        /// Un terrain tiré au hasard : de 4 à 24 mailles de côté, bloqué de 0 à 45 %, avec un à trois départs et une à
        /// trois arrivées, pris ou non. La même graine donne les mêmes terrains, dans le même ordre.
        /// </summary>
        public static TestGround Random(SimulationSettings settings, StructureDefinition block, System.Random random)
        {
            int size = random.Next(4, 25);
            double density = random.NextDouble() * 0.45;
            var blocked = new bool[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                    blocked[x, y] = random.NextDouble() < density;
            }
            return Build(settings, block, blocked, RandomCells(random, size), RandomCells(random, size));
        }

        private static List<Vector2Int> RandomCells(System.Random random, int size)
        {
            int count = random.Next(1, 4);
            var cells = new List<Vector2Int>(count);
            for (int i = 0; i < count; i++)
                cells.Add(new Vector2Int(random.Next(size), random.Next(size)));
            return cells;
        }

        /// <summary>Un terrain carré de ces blocs, départs et arrivées ; chaque bloc est posé par un ordre.</summary>
        public static TestGround Build(SimulationSettings settings, StructureDefinition block, bool[,] blocked,
            List<Vector2Int> starts, List<Vector2Int> goals)
        {
            int size = blocked.GetLength(0);
            var world = new World(settings, TestData.FlatMap((float)size / settings.CellsPerMeter));
            Assert.AreEqual(size, world.Grid.Width, "Le terrain a une maille par case du dessin.");

            int count = 0;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (!blocked[x, y])
                        continue;
                    var placement = new Placement(world.Grid.CellCenter(new Vector2Int(x, y)), 0f);
                    world.Submit(new PlaceStructureCommand(block, placement));
                    count++;
                }
            }
            world.Step();
            Assert.AreEqual(count, world.Structures.All.Count, "Chaque bloc prend sa maille.");
            return new TestGround(world, blocked, starts, goals);
        }

        /// <summary>Le terrain redessiné en texte, avec un escalier de mailles en « o », pour lire un échec.</summary>
        public string Describe(IReadOnlyList<Vector2Int> path)
        {
            var onPath = new HashSet<Vector2Int>(path);
            int size = Blocked.GetLength(0);
            var text = new StringBuilder();
            for (int y = size - 1; y >= 0; y--)
            {
                for (int x = 0; x < size; x++)
                {
                    var cell = new Vector2Int(x, y);
                    char sign = Blocked[x, y] ? '#' : '.';
                    if (onPath.Contains(cell))
                        sign = 'o';
                    if (Starts.Contains(cell))
                        sign = 'S';
                    if (Goals.Contains(cell))
                        sign = 'G';
                    text.Append(sign);
                }
                text.Append('\n');
            }
            return text.ToString();
        }
    }
}
