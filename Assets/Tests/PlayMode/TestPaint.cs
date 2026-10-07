using Diciela.Controls;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Ce que partagent les tests des outils de sol (PaintPointsTests, PaintSymmetryTests, PaintMagnetTests), sur le petit
    /// monde des tests, ses points donnés à tant de mètres à l'est et au nord de son milieu : le point du sol, sa maille,
    /// son point à l'écran, l'état du sol là ; un clic du joueur là ; la zone de la Place peinte par son ordre ; un
    /// rectangle droit.
    /// </summary>
    internal static class TestPaint
    {
        public static Vector2 GroundAt(float east, float north)
        {
            return new Vector2(TestWorld.Center.x + east, TestWorld.Center.z + north);
        }

        public static Vector2 OnScreenAt(this TestWorld test, float east, float north)
        {
            return test.OnScreen(TestWorld.Center + new Vector3(east, 0f, north));
        }

        public static Vector2Int CellAt(this TestWorld test, float east, float north)
        {
            return test.World.Grid.CellOf(GroundAt(east, north));
        }

        public static FloorState FloorAt(this TestWorld test, float east, float north)
        {
            return test.World.Floors.StateAt(test.CellAt(east, north));
        }

        /// <summary>Un clic du joueur à ce point du sol, l'aimant des places de charrette en marche ou coupé (Ctrl).</summary>
        public static void ClickAt(this TestWorld test, PaintTool tool, float east, float north, bool magnet = true)
        {
            tool.AimAt(test.OnScreenAt(east, north), magnet);
            tool.Click();
        }

        /// <summary>La zone de la Place peinte par son ordre, joué tout de suite.</summary>
        public static void PaintPlaza(this TestWorld test, GroundShape shape)
        {
            test.World.Submit(new PaintPlazaCommand(shape));
            test.World.Step();
        }

        /// <summary>Un rectangle droit de cette taille, centré à ce point.</summary>
        public static GroundBox BoxAt(float east, float north, float width, float depth)
        {
            return new GroundBox(new RotatedRect(GroundAt(east, north), new Vector2(width, depth), 0f));
        }
    }
}
