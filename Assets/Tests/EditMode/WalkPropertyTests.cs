using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La marche de la Chimère sur des terrains tirés au hasard (SAVOIR, 4), où l'on pose des fantômes pendant qu'elle
    /// flâne : sur elle, en travers de sa ligne, ou n'importe où, par l'ordre « poser ». Ce qui doit toujours tenir :
    /// elle n'entre jamais d'elle-même dans une maille prise ; posée dessous, elle en sort ; elle ne reste jamais figée
    /// en marchant ; les mêmes ordres rejouent la même partie ; un pas n'alloue rien.
    /// </summary>
    public sealed class WalkPropertyTests
    {
        private const int StepsPerGround = 1500;

        // Sortir d'un fantôme : 20 m au plus à 15 cm par pas, avec de la marge.
        private const int MaxStepsCovered = 200;

        private TestChimera _test;
        private StructureDefinition[] _ghosts;
        private int[] _sink;
        private int _moves;
        private int _walkOuts;

        [SetUp]
        public void CreateData()
        {
            _test = new TestChimera();
            _ghosts = new[]
            {
                _test.Block(1f, 1f),
                _test.Block(3f, 0.5f),
                _test.Keep(TestData.Structure("cabane", 3f, 3f)),
                _test.Keep(TestData.Structure("long", 4f, 1.5f))
            };
        }

        [TearDown]
        public void DestroyData()
        {
            _test.Dispose();
        }

        [Test]
        public void OnTwoHundredGroundsSheNeverWalksIntoAGhostAndIsNeverStuck()
        {
            _walkOuts = 0;
            _moves = 0;
            for (int seed = 0; seed < 200; seed++)
                PlayAGround(seed, null);

            // Le test ne prouve quelque chose que si les cas arrivent : des fantômes posés sur elle, dont elle sort, et
            // beaucoup de marche (environ 8 poses sur elle par terrain, dont une partie refusée faute de place).
            Assert.Greater(_walkOuts, 200, "Trop peu de sorties d'un fantôme pour juger.");
            Assert.Greater(_moves, 20000, "Trop peu de marche pour juger.");
        }

        [Test]
        public void TheSameOrdersPlayTheSameWalkAmongGhosts()
        {
            var first = new List<Vector2>();
            var second = new List<Vector2>();
            PlayAGround(7, first);
            PlayAGround(7, second);

            Assert.AreEqual(StepsPerGround, first.Count);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void AStepAllocatesNothingWhileSheWalks()
        {
            // Une cabane posée près d'elle : ses trajets la contournent.
            World world = _test.World();
            world.Submit(new PlaceStructureCommand(_ghosts[2], new Placement(new Vector2(23f, 21f), 0f)));
            // De quoi faire grandir une fois pour toutes les listes et les tableaux des trajets et des îlots.
            for (int step = 0; step < 3000; step++)
                world.Step();
            Villager chimera = world.Villagers.Chimera;
            _moves = 0;

            // Le compteur voit bien une allocation : sans cela, la seconde assertion ne prouverait rien.
            Assert.That(() => { _sink = new int[4]; }, Is.AllocatingGCMemory());
            Assert.AreEqual(4, _sink.Length);
            Assert.That(() =>
            {
                for (int step = 0; step < 1000; step++)
                {
                    world.Step();
                    if (chimera.Position != chimera.PreviousPosition)
                        _moves++;
                }
            }, Is.Not.AllocatingGCMemory());
            Assert.Greater(_moves, 100, "Elle doit marcher pendant la mesure.");
        }

        /// <summary>
        /// Un terrain de 8 à 24 m de côté, la Chimère au centre ; environ une fois toutes les 60 pas, un fantôme posé sur
        /// elle, sur sa ligne, ou n'importe où. Chaque pas est jugé ; les positions vont dans trail, si on le donne.
        /// </summary>
        private void PlayAGround(int seed, List<Vector2> trail)
        {
            var random = new System.Random(seed);
            float size = random.Next(8, 25);
            World world = _test.World(size);
            Villager chimera = world.Villagers.Chimera;
            NavGrid grid = world.Grid;
            int wayOutRings = grid.Metrics.CellCount(20f);
            int covered = 0;
            int stillWalking = 0;

            for (int step = 0; step < StepsPerGround; step++)
            {
                if (random.Next(60) == 0)
                    world.Submit(new PlaceStructureCommand(_ghosts[random.Next(_ghosts.Length)],
                        new Placement(Aim(random, world, size), random.Next(24) * 15f)));
                world.Step();
                trail?.Add(chimera.Position);

                // Partie d'une maille libre en suivant son trajet, elle finit le pas sur une maille libre.
                bool startFree = grid.IsWalkable(grid.CellOf(chimera.PreviousPosition));
                bool endFree = grid.IsWalkable(grid.CellOf(chimera.Position));
                if (chimera.IsWalking && startFree && !endFree)
                    Assert.Fail("Elle entre dans un fantôme, " + Where(seed, step, chimera));

                // Posée sous un fantôme, elle en sort, sauf si aucune maille libre n'est à sa portée.
                if (covered > 0 && endFree)
                    _walkOuts++;
                if (chimera.Position != chimera.PreviousPosition)
                    _moves++;
                covered = endFree ? 0 : covered + 1;
                if (covered > MaxStepsCovered && grid.TryFindNearestWalkable(chimera.Position, wayOutRings, out _))
                    Assert.Fail("Elle ne sort pas du fantôme, " + Where(seed, step, chimera));

                // En marche, elle avance à chaque pas ; seul le pas du départ la laisse sur place.
                stillWalking = chimera.IsWalking && chimera.Position == chimera.PreviousPosition ? stillWalking + 1 : 0;
                if (stillWalking > 1)
                    Assert.Fail("Elle marche sans avancer, " + Where(seed, step, chimera));
            }
        }

        private static string Where(int seed, int step, Villager chimera)
        {
            return "terrain " + seed + ", pas " + step + ", en " + chimera.Position.ToString("F3");
        }

        /// <summary>Où poser : sur elle (un tiers des fois), sur sa ligne si elle marche (un tiers), ou n'importe où.</summary>
        private static Vector2 Aim(System.Random random, World world, float size)
        {
            Villager chimera = world.Villagers.Chimera;
            int choice = random.Next(3);
            if (choice == 0)
                return chimera.Position;
            if (choice == 1 && chimera.IsWalking)
            {
                IReadOnlyList<Vector2> corners = chimera.Path.Corners;
                Vector2 goal = corners[corners.Count - 1];
                return Vector2.Lerp(chimera.Position, goal, (float)random.NextDouble());
            }
            return new Vector2((float)random.NextDouble() * size, (float)random.NextDouble() * size);
        }
    }
}
