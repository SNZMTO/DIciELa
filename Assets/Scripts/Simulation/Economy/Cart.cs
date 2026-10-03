using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Une charrette de l'étal (GDD, section 11 ; décision 194) : celle d'un entrepôt, le campement compris. Rangée chez
    /// lui, on ne la voit pas ; une place de charrette de la Place l'appelle, un charretier vient la prendre et la tire
    /// jusqu'à cette place, où elle se gare et sert d'étal. Sa position et son cap, au sol, valent tant qu'on la voit ;
    /// ceux du pas d'avant aussi, pour que la vue glisse de l'un à l'autre. Seules les charrettes de la ville la changent
    /// (Carts).
    /// </summary>
    public sealed class Cart
    {
        internal Cart(int id, Structure home, CartDefinition definition, int modelIndex)
        {
            Id = id;
            Home = home;
            Definition = definition;
            ModelIndex = modelIndex;
            Position = home.Placement.Center;
            PreviousPosition = Position;
        }

        public int Id { get; }

        /// <summary>L'entrepôt, ou le campement, où elle est rangée.</summary>
        public Structure Home { get; }

        public CartDefinition Definition { get; }

        /// <summary>Lequel des modèles de sa fiche on voit : tiré au hasard à sa naissance, toujours le même.</summary>
        public int ModelIndex { get; }

        public CartState State { get; private set; }

        /// <summary>La place de charrette qui l'a appelée ; null tant qu'aucune ne l'appelle.</summary>
        public CartSpot Spot { get; private set; }

        /// <summary>Son milieu, au sol, en mètres.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Son milieu au début du pas.</summary>
        public Vector2 PreviousPosition { get; private set; }

        /// <summary>Vers où pointe son avant, son timon, en degrés depuis le nord vers l'est.</summary>
        public float HeadingDegrees { get; private set; }

        /// <summary>Son cap au début du pas.</summary>
        public float PreviousHeadingDegrees { get; private set; }

        internal void BeginStep()
        {
            PreviousPosition = Position;
            PreviousHeadingDegrees = HeadingDegrees;
        }

        internal void Answer(CartSpot spot)
        {
            Spot = spot;
        }

        /// <summary>On l'attelle : elle sort, là, ainsi tournée, sans glisser depuis l'entrepôt.</summary>
        internal void Hitch(Vector2 position, float headingDegrees)
        {
            State = CartState.Pulled;
            Position = position;
            HeadingDegrees = headingDegrees;
            BeginStep();
        }

        internal void MoveTo(Vector2 position, float headingDegrees)
        {
            Position = position;
            HeadingDegrees = headingDegrees;
        }

        internal void Park()
        {
            State = CartState.Parked;
            MoveTo(Spot.Position, Spot.HeadingDegrees);
        }

        /// <summary>Elle retourne à son entrepôt, sans place qui l'appelle.</summary>
        internal void ReturnHome()
        {
            State = CartState.AtHome;
            Spot = null;
            MoveTo(Home.Placement.Center, 0f);
            BeginStep();
        }
    }
}
