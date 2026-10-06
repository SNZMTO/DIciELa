using System;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Les ordres de l'outil de la Place (décision 206) : peindre ou gommer une forme, poser ou retirer une place de
    /// charrette. Chaque ordre joué lève un événement de la Place (elle a changé, une place est posée ou retirée, ou un
    /// refus) : il le dit à l'outil, qui redemande son aperçu. Un clic attend que le précédent soit joué ; un trait
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
        }

        /// <summary>Vrai entre l'envoi d'un ordre et son issue.</summary>
        public bool IsWaiting { get; private set; }

        public void Dispose()
        {
            _world.Place.PlaceChanged -= OnPlayed;
            _world.Place.MarkPlaced -= OnMarkPlayed;
            _world.Place.MarkRemoved -= OnMarkPlayed;
            _world.Place.PlaceRefused -= OnRefused;
        }

        /// <summary>Peint cette forme, ou la gomme.</summary>
        public void SendShape(GroundShape shape, bool erase)
        {
            Send(erase ? new ErasePlaceCommand(shape) : (Command)new PaintPlaceCommand(shape));
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
    }
}
