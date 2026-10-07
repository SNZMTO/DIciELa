using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Tracer la Place (GDD, section 11 ; décisions 204 et 205), par les ordres du joueur, sur le monde plat de 40 m aux
    /// mailles de 50 cm : peindre une forme ajoute à la Place les mailles libres dont le milieu est en elle ; sa
    /// superficie a un plafond ; elle reste d'un seul tenant, d'une maille à sa voisine par un côté ; la gomme rend le
    /// sol, jamais sous une place de charrette. Un ordre refusé ne change rien, et la Place dit pourquoi.
    /// </summary>
    public sealed class PlazaZoneTests
    {
        private static readonly Vector2 Middle = new Vector2(20f, 20f);
        private TestTown _town;
        private List<PlazaResult> _refused;

        [SetUp]
        public void CreateTown()
        {
            _town = new TestTown();
            _refused = new List<PlazaResult>();
        }

        [TearDown]
        public void DestroyTown()
        {
            _town.Dispose();
        }

        [Test]
        public void ADiscCoversItsAreaAndPeopleWalkOnIt()
        {
            World world = World();
            Paint(world, new GroundDisc(Middle, 7f));

            Assert.IsEmpty(_refused);
            Assert.AreEqual(153.9f, world.Plaza.AreaSquareMeters, 3f, "π × 7², à la maille près.");
            Vector2Int middle = world.Grid.CellOf(Middle);
            Assert.IsTrue(world.Plaza.Contains(middle));
            Assert.IsFalse(world.Plaza.Contains(world.Grid.CellOf(Middle + new Vector2(7.5f, 0f))), "Rien au-delà.");
            Assert.AreEqual(PlazaZone.Occupant, world.Grid.OccupantAt(middle), "Le maillage la tient…");
            Assert.IsTrue(world.Grid.IsWalkable(middle), "… et l'on y marche.");
        }

        [Test]
        public void ItsAreaHasACeiling()
        {
            // Une Place de 100 m² au plus : un disque de 6 m de rayon (113 m²) ne se trace pas ; un de 5 m (79 m²), oui ;
            // un carré de 4 m qui le touche (16 m²) aussi ; un second, non.
            TestData.UsePlaza(_town.Catalog, _town.Keep(TestData.Plaza(100f, 2)));
            World world = World();
            Paint(world, new GroundDisc(Middle, 6f));
            Assert.AreEqual(0, world.Plaza.CellCount, "Trop grande, elle n'est pas tracée du tout.");
            Paint(world, new GroundDisc(Middle, 5f));
            Paint(world, new GroundBox(new RotatedRect(new Vector2(20f, 27f), new Vector2(4f, 4f), 0f)));
            Paint(world, new GroundBox(new RotatedRect(new Vector2(20f, 31f), new Vector2(4f, 4f), 0f)));

            CollectionAssert.AreEqual(new[] { PlazaResult.TooLarge, PlazaResult.TooLarge }, _refused);
            Assert.AreEqual(94.5f, world.Plaza.AreaSquareMeters, 2f, "Le disque de 5 m et un carré de 4 m.");
            Assert.LessOrEqual(world.Plaza.AreaSquareMeters, 100f);
        }

        [Test]
        public void ItStaysInOnePiece()
        {
            World world = World();
            Paint(world, new GroundDisc(new Vector2(12f, 20f), 3f));
            Paint(world, new GroundDisc(new Vector2(28f, 20f), 3f));
            Assert.AreEqual(new[] { PlazaResult.NotOnePiece }, _refused.ToArray(), "Un second morceau, à part : non.");

            // Un trait les relie : le second disque touche alors la Place.
            Paint(world, new GroundStroke(new[] { new Vector2(12f, 20f), new Vector2(28f, 20f) }, 1f));
            Paint(world, new GroundDisc(new Vector2(28f, 20f), 3f));
            Assert.AreEqual(1, _refused.Count, "Reliés, ils font un seul morceau.");

            // La gomme qui couperait le trait en deux est refusée ; celle qui retire tout un bout ne l'est pas.
            world.Submit(new ErasePlazaCommand(new GroundStroke(new[] { new Vector2(20f, 15f), new Vector2(20f, 25f) }, 1.5f)));
            world.Step();
            Assert.AreEqual(PlazaResult.NotOnePiece, _refused[1], "Couper la Place en deux : non.");
            world.Submit(new ErasePlazaCommand(new GroundDisc(new Vector2(27f, 20f), 4.5f)));
            world.Step();
            Assert.AreEqual(2, _refused.Count, "Retirer un bout : oui.");
            Assert.IsFalse(world.Plaza.Contains(world.Grid.CellOf(new Vector2(28f, 20f))));
            Assert.AreEqual(0, world.Grid.OccupantAt(world.Grid.CellOf(new Vector2(28f, 20f))), "Le sol est rendu.");
        }

        [Test]
        public void TheEraserDoesNotPassUnderACartSpot()
        {
            World world = World();
            Paint(world, new GroundDisc(Middle, 7f));
            TestTown.Send(world, new AddCartSpotCommand(Middle, 0f));
            world.Submit(new ErasePlazaCommand(new GroundDisc(Middle + new Vector2(1f, 0f), 2f)));
            world.Step();

            CollectionAssert.AreEqual(new[] { PlazaResult.MarkInTheWay }, _refused, "On retire d'abord la place.");
            Assert.IsTrue(world.Plaza.Contains(world.Grid.CellOf(Middle)), "Rien n'est gommé.");
        }

        private World World()
        {
            World world = _town.World();
            world.Plaza.PlazaRefused += _refused.Add;
            return world;
        }

        private static void Paint(World world, GroundShape shape)
        {
            TestTown.Send(world, new PaintPlazaCommand(shape));
        }
    }
}
