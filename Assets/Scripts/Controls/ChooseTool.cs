using System;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// L'outil « choisir » : toucher une construction, en visant son modèle (ARCHITECTURE, section 11). Ce qu'on touche,
    /// ou rien, est dit à qui l'écoute : l'interface y ouvre la fiche d'un lieu, ou la ferme (décision 141). Toucher le
    /// logis de la Chimère l'envoie aussi s'y coucher, ou la réveille si elle dort ou y va (GDD, section 6). Les gestes et
    /// les tests l'appellent par la même méthode.
    /// </summary>
    public sealed class ChooseTool
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly StructureViews _views;

        public ChooseTool(World world, Camera camera, StructureViews views)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (camera == null || views == null)
                throw new ArgumentException("L'outil « choisir » a besoin d'une caméra et des constructions vues.");

            _world = world;
            _camera = camera;
            _views = views;
        }

        /// <summary>Levé à chaque toucher : la construction touchée, ou null pour rien.</summary>
        public event Action<Structure> Touched;

        /// <summary>Touche ce qu'on voit sous ce point de l'écran ; vrai si un ordre est envoyé.</summary>
        public bool TouchAt(Vector2 screenPoint)
        {
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            _views.TryPick(ray, out Structure touched);

            Touched?.Invoke(touched);

            Villager chimera = _world.Villagers.Chimera;
            if (touched == null || chimera == null || !ReferenceEquals(touched, _world.Structures.ChimeraHome))
                return false;
            if (chimera.IsAsleep || chimera.IsGoingToBed)
                _world.Submit(new WakeChimeraCommand());
            else
                _world.Submit(new SendChimeraToBedCommand());
            return true;
        }
    }
}
