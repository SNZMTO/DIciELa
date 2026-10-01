using System;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// L'outil « choisir » : toucher une construction, en visant son modèle (ARCHITECTURE, section 11). Ce qu'on touche,
    /// ou rien, est dit à qui l'écoute : l'interface y ouvre la fiche d'un lieu, ou la ferme (décisions 141 et 172 : la
    /// Chimère s'envoie dormir depuis la fiche de son logis). Les gestes et les tests l'appellent par la même méthode.
    /// </summary>
    public sealed class ChooseTool
    {
        private readonly Camera _camera;
        private readonly StructureViews _views;

        public ChooseTool(Camera camera, StructureViews views)
        {
            if (camera == null || views == null)
                throw new ArgumentException("L'outil « choisir » a besoin d'une caméra et des constructions vues.");

            _camera = camera;
            _views = views;
        }

        /// <summary>Levé à chaque toucher : la construction touchée, ou null pour rien.</summary>
        public event Action<Structure> Touched;

        /// <summary>Touche ce qu'on voit sous ce point de l'écran ; vrai si c'est une construction.</summary>
        public bool TouchAt(Vector2 screenPoint)
        {
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            _views.TryPick(ray, out Structure touched);
            Touched?.Invoke(touched);
            return touched != null;
        }
    }
}
