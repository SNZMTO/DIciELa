using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.UI;
using Diciela.View;

namespace Diciela.Game
{
    /// <summary>
    /// La Place en ville (décisions 206 et 210) : sa barre, les gestes des outils de sol, et sa zone vue en mode
    /// construction (GDD, sections 11 et 16 : « comme les fantômes ») : le panneau Construire ouvert, ou en main une
    /// construction, l'outil de la Place, Chemins ou Déplacer. Né à l'ouverture de la ville, libéré à sa sortie.
    /// </summary>
    internal sealed class TownPlace : IDisposable
    {
        private readonly TownTools _tools;
        private readonly IBuildMenu _menu;
        private readonly PlaceZoneView _zone;
        private readonly PlaceControls _controls;
        private readonly PlaceBar _bar;

        public TownPlace(World world, TextTable texts, TownTools tools, IBuildMenu menu, PlaceZoneView zone,
            PlaceControls controls, PlaceBar bar, IToolKeys keys, IInterfaceArea hud)
        {
            _tools = tools;
            _menu = menu;
            _zone = zone;
            _controls = controls;
            _bar = bar;
            _zone.Show(world);
            _bar.Show(texts, tools.Place, tools.Paths, world, keys);
            _controls.Bind(hud, tools.Place, tools.Paths);
            _menu.OpenChanged += ShowZone;
            _tools.Placement.HeldChanged += ShowZone;
            _tools.Place.HeldChanged += ShowZone;
            _tools.Paths.HeldChanged += ShowZone;
            _tools.Move.HeldChanged += ShowZone;
            ShowZone();
        }

        public void Dispose()
        {
            _menu.OpenChanged -= ShowZone;
            _tools.Placement.HeldChanged -= ShowZone;
            _tools.Place.HeldChanged -= ShowZone;
            _tools.Paths.HeldChanged -= ShowZone;
            _tools.Move.HeldChanged -= ShowZone;
            _controls.Bind(null);
            _bar.Hide();
            _zone.Hide();
        }

        private void ShowZone()
        {
            _zone.SetShown(_menu.IsOpen || _tools.Placement.IsActive || _tools.Place.IsActive || _tools.Paths.IsActive ||
                           _tools.Move.IsActive);
        }
    }
}
