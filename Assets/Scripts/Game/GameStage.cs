using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.UI;
using Diciela.View;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Ce que le démarrage montre d'un monde et relie aux gestes (ARCHITECTURE, section 11), dans l'ordre d'une partie
    /// neuve (GDD, section 6) : la ville vue (caméra, constructions, habitants) et l'écran de création, la caméra tout
    /// près du point d'arrivée, tournée vers la Chimère à venir ; puis, l'ordre de la créer parti, la barre et les gestes
    /// de la ville, la caméra reculée à sa distance de départ. Il délie et cache tout à la sortie.
    /// </summary>
    public sealed class GameStage : MonoBehaviour
    {
        [SerializeField] private CameraRig _cameraRig;
        [SerializeField] private CameraControls _cameraControls;
        [SerializeField] private StructureViews _structureViews;
        [SerializeField] private VillagerViews _villagerViews;
        [SerializeField] private PlacementPreview _placementPreview;
        [SerializeField] private PlacementControls _placementControls;
        [SerializeField] private ChooseControls _chooseControls;
        [SerializeField] private BuildBar _buildBar;
        [SerializeField] private CreationScreen _creationScreen;
        [SerializeField] private AppearancePreview _appearancePreview;
        [SerializeField] private ConfirmControls _confirmControls;

        private PlacementTool _placementTool;

        /// <summary>Vrai si la scène a toutes ses vues et tous ses gestes.</summary>
        public bool IsComplete => _cameraRig != null && _cameraControls != null && _structureViews != null &&
                                  _villagerViews != null && _placementPreview != null && _placementControls != null &&
                                  _chooseControls != null && _buildBar != null && _creationScreen != null &&
                                  _appearancePreview != null && _confirmControls != null;

        /// <summary>
        /// Montre ce monde et ouvre l'écran de création ; à la validation, ouvre la ville, puis appelle started. La
        /// Chimère regarde vers le nord : la caméra se tourne pour la voir de face.
        /// </summary>
        public void Open(World world, Catalog catalog, TextTable texts, PlacementSettings placement, Action started)
        {
            _cameraRig.Show(world.Map);
            _structureViews.Show(world.Structures, world.Map.Relief);
            _villagerViews.Show(world.Villagers, world.Map.Relief);
            _appearancePreview.Stand(world.Map.ArrivalPoint, world.Map.Relief);
            _creationScreen.Show(catalog, texts, _appearancePreview, world, () =>
            {
                OpenTown(world, catalog, texts, placement);
                started();
            });
            _cameraControls.Bind(_creationScreen);
            _confirmControls.Bind(_creationScreen.Confirm);
            _cameraRig.Rotate(180f);
            _cameraRig.ZoomTo(_cameraRig.NearestDistance);
        }

        /// <summary>Donne aux habitants vus la fraction du pas en cours.</summary>
        public void Follow(float stepFraction, float deltaSeconds)
        {
            _villagerViews.Follow(stepFraction, deltaSeconds);
        }

        /// <summary>Délie et cache tout ce qui a pu être montré : chaque Hide accepte de n'avoir rien à cacher.</summary>
        public void Close()
        {
            _confirmControls.Bind(null);
            _placementControls.Bind(null, null);
            _chooseControls.Bind(null, null, null);
            _cameraControls.Bind(null);
            _creationScreen.Hide();
            _appearancePreview.Hide();
            _buildBar.Hide();
            if (_placementTool != null)
            {
                _placementTool.Cancel();
                _placementTool.Dispose();
            }
            _placementTool = null;
            _cameraRig.Hide();
            _structureViews.Hide();
            _villagerViews.Hide();
        }

        private void OpenTown(World world, Catalog catalog, TextTable texts, PlacementSettings placement)
        {
            _confirmControls.Bind(null);
            _creationScreen.Hide();
            _appearancePreview.Hide();
            _placementTool = new PlacementTool(world, _cameraRig.Camera, _placementPreview, placement);
            _buildBar.Show(catalog, texts, _placementTool, world);
            _cameraControls.Bind(_buildBar);
            _placementControls.Bind(_placementTool, _buildBar);
            _chooseControls.Bind(new ChooseTool(world, _cameraRig.Camera, _structureViews), _placementTool, _buildBar);
            _cameraRig.ZoomTo(_cameraRig.StartDistance);
        }
    }
}
