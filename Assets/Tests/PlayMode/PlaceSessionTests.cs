using System.Collections;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La Place de bout en bout, sur la scène de test et avec le vrai contenu (décisions 192, 193, 197, 198 et 203), par
    /// les gestes du joueur (PlaceSession) : prise aux Métiers et posée devant le campement, elle se bâtit avec le stock
    /// de départ, qui paie son coût ; un trajet la traverse tout droit, d'un bord à l'autre ; la Chimère y gare la
    /// charrette du campement, qu'on voit glisser jusqu'à sa place. Les vrais appareils sont coupés (GestureFixture) : la
    /// souris du développeur ne fait pas défiler la caméra par les bords pendant le test.
    /// </summary>
    public sealed class PlaceSessionTests : GestureFixture
    {
        [UnityTearDown]
        public IEnumerator LeaveTheTestScene()
        {
            yield return TestOpening.Leave(PlaceSession.MaxFrames);
        }

        [UnityTest]
        public IEnumerator ThePlaceComesFromTheTradesTabAndIsBuiltFromTheStartingStock()
        {
            var session = new PlaceSession();
            yield return session.Build();
            World world = session.World;
            Inventory stock = world.Warehouses.StockOf(session.Camp);
            foreach (GoodAmount goods in PlaceSession.CatalogOf().Opening.StartingGoods)
                Assert.AreEqual(goods.Amount - PlaceSession.CostOf(session.Place.Definition, goods.Good),
                    stock.CountOf(goods.Good), "Le stock de départ a payé la Place en " + goods.Good.Id + ".");

            // D'un mètre avant son bord ouest à un mètre après son bord est.
            NavGrid grid = world.Grid;
            float half = session.Place.Definition.WidthMeters * 0.5f + PlaceSession.GapMeters;
            Vector2Int from = grid.CellOf(session.At - new Vector2(half, 0f));
            Vector2Int to = grid.CellOf(session.At + new Vector2(half, 0f));
            var path = new Path();
            Assert.IsTrue(world.PathFinder.TryFindPath(new[] { from }, new[] { to }, path), "On traverse la Place.");
            Assert.AreEqual(to.x - from.x + 1, path.Cells.Count, "Le trajet la traverse tout droit.");
            foreach (Vector2Int cell in path.Cells)
                Assert.AreEqual(from.y, cell.y, "Le trajet reste sur sa rangée.");
        }

        [UnityTest]
        public IEnumerator TheCampCartComesToThePlaceAndIsSeenParkedThere()
        {
            var session = new PlaceSession();
            yield return session.Build();
            World world = session.World;
            Cart cart = null;
            foreach (Cart kept in world.Carts.All)
            {
                if (ReferenceEquals(kept.Home, session.Camp))
                    cart = kept;
            }
            Assert.IsNotNull(cart, "Le campement garde sa charrette.");

            for (int step = 0; step < PlaceSession.MaxSteps && cart.State != CartState.Parked; step++)
            {
                world.Step();
                if (step % PlaceSession.StepsPerFrame == 0)
                    yield return null;
            }
            Assert.AreEqual(CartState.Parked, cart.State, "La Chimère gare la charrette du campement sur la Place.");
            Transform shown = TestOpening.FindIn<CartViews>(ScenePaths.GameTest).transform.Find("charrette n° " + cart.Id);
            Assert.IsNotNull(shown, "La charrette a sa vue.");
            Vector2 spot = cart.Spot.Position;
            for (int frame = 0; frame < PlaceSession.MaxFrames && Vector2.Distance(Flat(shown.position), spot) > 0.01f;
                 frame++)
                yield return null;
            Assert.IsTrue(shown.gameObject.activeInHierarchy, "Garée, on la voit.");
            Assert.Less(Vector2.Distance(Flat(shown.position), spot), 0.01f, "Elle a glissé jusqu'à sa place.");
            Assert.AreEqual(cart.Definition.Models[cart.ModelIndex].name + "(Clone)", shown.GetChild(0).name,
                "On voit le modèle du pack que la simulation lui a tiré.");
        }

        private static Vector2 Flat(Vector3 point)
        {
            return new Vector2(point.x, point.z);
        }
    }
}
