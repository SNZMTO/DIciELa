using Diciela.Data;
using Diciela.Simulation;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les sols vus (GDD, sections 9 et 16 ; décisions 209, 213, 216 et 227) : un sol qui sert (bâti, ou qu'on efface)
    /// se peint dans le sol de la carte, de son calque, arrondi, effiloché et cerné de son liant (FloorPainting) ; un
    /// sol en chantier se voit en fantôme, à la texture de son calque, et un sol qu'on efface en voile de la couleur
    /// des fantômes (FloorGhosts), comme le fantôme d'une construction. La vue écoute les sols et se refait à la fin de
    /// l'image où ils ont changé, une seule fois.
    /// </summary>
    public sealed class FloorViews : MonoBehaviour
    {
        private World _world;
        private FloorPainting _painting;
        private FloorGhosts _ghosts;
        private bool _ghostsStale;

        /// <summary>
        /// Suit les sols de ce monde : peints sur ce Terrain (la copie en mémoire de la carte, dont la peinture d'origine
        /// est sur original), leurs fantômes des matériaux et de la couleur des fantômes de ces réglages.
        /// </summary>
        public void Show(World world, Terrain terrain, TerrainData original, PlacementSettings settings)
        {
            Hide();
            if (settings == null || settings.TranslucentMaterial == null || settings.FloorGhostMaterial == null)
                throw new System.InvalidOperationException("Les sols vus n'ont pas leur fiche de réglages de la pose.");
            _world = world;
            _painting = new FloorPainting(terrain, original, world.Floors, world.Grid, settings.GroundRoundingMeters,
                settings.FloorEdgeSoftnessMeters);
            _ghosts ??= new FloorGhosts(transform, settings);
            _ghosts.PlaceOn(terrain);
            _world.Floors.FloorsChanged += OnFloorsChanged;
            // Ce qui sert déjà se peint, ce qui est en chantier se voit en fantôme : la vue part de l'état du monde.
            _painting.MarkServing();
            _ghostsStale = true;
        }

        /// <summary>Cesse de suivre les sols ; leur peinture reste sur la copie de la carte, qui part avec elle.</summary>
        public void Hide()
        {
            if (_world != null)
                _world.Floors.FloorsChanged -= OnFloorsChanged;
            _world = null;
            _painting = null;
            _ghostsStale = false;
            _ghosts?.Hide();
        }

        private void OnDestroy()
        {
            Hide();
            _ghosts?.Destroy();
            _ghosts = null;
        }

        private void OnFloorsChanged(RectInt cells)
        {
            _painting.Mark(cells);
            _ghostsStale = true;
        }

        private void LateUpdate()
        {
            if (_world == null)
                return;
            _painting.Flush();
            if (!_ghostsStale)
                return;
            _ghostsStale = false;
            _ghosts.Rebuild(_world);
        }
    }
}
