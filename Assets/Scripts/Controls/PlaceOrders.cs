using System;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Les ordres d'un outil de sol (décisions 206 et 210) : peindre ou gommer une forme, la zone ou un sol, poser ou
    /// retirer une place de charrette. Chaque ordre joué lève un événement de la Place ou des sols (ils ont changé, une
    /// place est posée ou retirée, ou un refus) : il le dit à l'outil, qui redemande son aperçu. Un sol qui finit de se
    /// bâtir lève aussi le sien : l'aperçu se refait, et un clic attendu repart un peu plus tôt, sans dommage (un ordre
    /// qui ne change rien est refusé). Un clic attend que le précédent soit joué ; un trait
    /// relâché part toujours, l'ordre se vérifiant de lui-même. Celui qui le crée le libère (Dispose).
    /// </summary>
    internal sealed class PlaceOrders : IDisposable
    {
        private readonly World _world;
        private readonly Action _played;

        public PlaceOrders(World world, Action played)
        {
            _world = world;
            _played = played;
            _world.Place.PlaceChanged += OnPlayed;
            _world.Place.MarkPlaced += OnMarkPlayed;
            _world.Place.MarkRemoved += OnMarkPlayed;
            _world.Place.PlaceRefused += OnRefused;
            _world.Floors.FloorsChanged += OnFloorsPlayed;
            _world.Floors.FloorRefused += OnFloorRefused;
        }

        /// <summary>Vrai entre l'envoi d'un ordre et son issue.</summary>
        public bool IsWaiting { get; private set; }

        public void Dispose()
        {
            _world.Place.PlaceChanged -= OnPlayed;
            _world.Place.MarkPlaced -= OnMarkPlayed;
            _world.Place.MarkRemoved -= OnMarkPlayed;
            _world.Place.PlaceRefused -= OnRefused;
            _world.Floors.FloorsChanged -= OnFloorsPlayed;
            _world.Floors.FloorRefused -= OnFloorRefused;
        }

        /// <summary>Peint cette forme, ou la gomme : la zone, ou le sol de cette encre, à cette entrée.</summary>
        public void SendShape(GroundShape shape, bool erase, PaintInks inks, FloorScope scope)
        {
            if (inks.LaysTheZone)
                Send(erase ? new ErasePlaceCommand(shape) : (Command)new PaintPlaceCommand(shape));
            else if (erase)
                Send(new EraseFloorCommand(shape, scope));
            else if (inks.Floor != null)
                Send(new TraceFloorCommand(shape, inks.Floor, scope));
        }

        /// <summary>Pose une place de charrette à ce milieu, l'avant de sa charrette tourné vers ce cap.</summary>
        public void SendMark(Vector2 center, float headingDegrees)
        {
            Send(new AddCartSpotCommand(center, headingDegrees));
        }

        /// <summary>Retire cette place de charrette.</summary>
        public void SendRemoval(PlaceMark mark)
        {
            Send(new RemoveCartSpotCommand(mark.Id));
        }

        private void Send(Command order)
        {
            _world.Submit(order);
            IsWaiting = true;
        }

        private void OnPlayed()
        {
            IsWaiting = false;
            _played();
        }

        private void OnMarkPlayed(PlaceMark mark)
        {
            OnPlayed();
        }

        private void OnRefused(PlaceResult reason)
        {
            OnPlayed();
        }

        private void OnFloorsPlayed(RectInt cells)
        {
            OnPlayed();
        }

        private void OnFloorRefused(FloorResult reason)
        {
            OnPlayed();
        }
    }
}
