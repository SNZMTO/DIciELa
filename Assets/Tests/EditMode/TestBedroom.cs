using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le banc du sommeil de la Chimère, sur celui de TestChimera (carte de 40 m, deux mailles par mètre, 1 m libre
    /// devant une porte) : elle est née en (20,25 ; 20,25). Son campement de 3 × 3 m, posé en (20 ; 26) porte au nord, couvre
    /// x de 18,5 à 21,5 et z de 24,5 à 27,5 ; le devant de sa porte va de 19,5 à 20,5 en x, de 27,5 à 28,5 en z. Son
    /// milieu (20 ; 28) tombe dans la maille (40 ; 56) : son lit est au milieu de cette maille, (20,25 ; 28,25).
    /// </summary>
    internal static class TestBedroom
    {
        public static readonly Vector2 Bed = new Vector2(20.25f, 28.25f);

        /// <summary>Le monde du banc, son campement installé.</summary>
        public static World WorldWithCamp(TestChimera test)
        {
            World world = test.World();
            Place(world, test.Keep(TestData.Home()), 20f, 26f);
            world.Step();
            Assert.IsNotNull(world.Structures.ChimeraHome, "Le campement est installé.");
            return world;
        }

        public static void Place(World world, StructureDefinition definition, float x, float z)
        {
            world.Submit(new PlaceStructureCommand(definition, new Placement(new Vector2(x, z), 0f)));
        }

        /// <summary>Ferme le devant de la porte du campement par trois blocs : à l'ouest, à l'est et au nord.</summary>
        public static void EncloseTheDoor(World world, TestChimera test)
        {
            Place(world, test.Block(1f, 2f), 19f, 28.5f);
            Place(world, test.Block(1f, 2f), 21f, 28.5f);
            Place(world, test.Block(1f, 1f), 20f, 29f);
        }

        /// <summary>L'envoie dormir, et joue jusqu'à ce qu'elle dorme.</summary>
        public static void SleepUntilAsleep(World world)
        {
            Villager chimera = world.Villagers.Chimera;
            world.Submit(new SendChimeraToBedCommand());
            TestGrids.StepUntil(world, () => chimera.IsAsleep, 600, "son sommeil");
        }
    }
}
