using System;
using Diciela.Controls;
using Diciela.Simulation;
using Diciela.UI;
using Diciela.View;

namespace Diciela.Game
{
    /// <summary>
    /// La Place en ville (décisions 206 et 210) : les gestes des outils de sol, et sa zone vue en mode construction
    /// (GDD, sections 11 et 16 : « comme les fantômes ») : en main une construction, l'outil de la Place, Chemins ou
    /// Déplacer ; la roue de Construire ne s'ouvre qu'une construction en main. Né à l'ouverture de la ville, libéré à
    /// sa sortie.
    /// </summary>
    internal sealed class TownPlaza : IDisposable
    {
        private readonly TownTools _tools;
        private readonly PlazaZoneView _zone;
        private readonly PaintControls _controls;

        public TownPlaza(World world, TownTools tools, PlazaZoneView zone, PaintControls controls, IInterfaceArea hud)
        {
            _tools = tools;
            _zone = zone;
            _controls = controls;
            _zone.Show(world);
            _controls.Bind(hud, tools.Plaza, tools.Paths);
            _tools.Placement.HeldChanged += ShowZone;
            _tools.Plaza.HeldChanged += ShowZone;
            _tools.Paths.HeldChanged += ShowZone;
            _tools.Move.HeldChanged += ShowZone;
            ShowZone();
        }

        public void Dispose()
        {
            _tools.Placement.HeldChanged -= ShowZone;
            _tools.Plaza.HeldChanged -= ShowZone;
            _tools.Paths.HeldChanged -= ShowZone;
            _tools.Move.HeldChanged -= ShowZone;
            _controls.Bind(null);
            _zone.Hide();
        }

        private void ShowZone()
        {
            _zone.SetShown(_tools.Placement.IsActive || _tools.Plaza.IsActive || _tools.Paths.IsActive ||
                           _tools.Move.IsActive);
        }
    }
}
