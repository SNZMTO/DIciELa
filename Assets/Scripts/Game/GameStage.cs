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
    /// neuve (GDD, section 6) : l'écran de création dans l'atelier, la Chimère à venir sur son socle, qu'on fait
    /// tourner ; puis, l'ordre de la créer parti, la ville (caméra, barre, gestes), la caméra tout près de la Chimère
    /// qui naît au point d'arrivée, puis reculée à sa distance de départ. Les constructions et les habitants sont vus dès
    /// l'ouverture. Toucher un lieu ouvre sa fiche ; Échap la ferme d'abord, puis lâche l'outil en main. Il délie et
    /// cache tout à la sortie.
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
        [SerializeField] private DayBoard _dayBoard;
        [SerializeField] private PlaceSheet _placeSheet;
        [SerializeField] private EscapeControls _escapeControls;
        [SerializeField] private CreationScreen _creationScreen;
        [SerializeField] private AppearancePreview _appearancePreview;
        [SerializeField] private ConfirmControls _confirmControls;
        [SerializeField] private Atelier _atelier;
        [SerializeField] private PreviewControls _previewControls;

        private PlacementTool _placementTool;
        private ChooseTool _chooseTool;

        /// <summary>Vrai si la scène a toutes ses vues et tous ses gestes.</summary>
        public bool IsComplete => _cameraRig != null && _cameraControls != null && _structureViews != null &&
                                  _villagerViews != null && _placementPreview != null && _placementControls != null &&
                                  _chooseControls != null && _buildBar != null && _dayBoard != null &&
                                  _placeSheet != null && _escapeControls != null &&
                                  _creationScreen != null &&
                                  _appearancePreview != null && _confirmControls != null && _atelier != null &&
                                  _previewControls != null;

        /// <summary>Montre ce monde et ouvre l'écran de création ; à la validation, ouvre la ville, puis appelle started.</summary>
        public void Open(World world, Catalog catalog, TextTable texts, PlacementSettings placement, Action started)
        {
            _structureViews.Show(world.Structures, world.Map.Relief);
            _villagerViews.Show(world.Villagers, world.Map.Relief);
            _atelier.Show();
            _appearancePreview.Stand(_atelier.Spot);
            _creationScreen.Show(catalog, texts, _appearancePreview, world, () =>
            {
                OpenTown(world, catalog, texts, placement);
                started();
            });
            _previewControls.Bind(_appearancePreview, _creationScreen);
            _confirmControls.Bind(_creationScreen.Confirm);
        }

        /// <summary>Donne aux habitants vus et à la journée la fraction du pas en cours.</summary>
        public void Follow(float stepFraction, float deltaSeconds)
        {
            _villagerViews.Follow(stepFraction, deltaSeconds);
            _dayBoard.Follow(stepFraction);
        }

        /// <summary>Délie et cache tout ce qui a pu être montré : chaque Hide accepte de n'avoir rien à cacher.</summary>
        public void Close()
        {
            _confirmControls.Bind(null);
            _previewControls.Bind(null, null);
            _placementControls.Bind(null, null);
            _chooseControls.Bind(null, null, null);
            _escapeControls.Bind();
            if (_chooseTool != null)
                _chooseTool.Touched -= _placeSheet.Touch;
            _chooseTool = null;
            _cameraControls.Bind(null);
            _creationScreen.Hide();
            _appearancePreview.Hide();
            _atelier.Hide();
            _buildBar.Hide();
            _dayBoard.Hide();
            _placeSheet.Hide();
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

        // La Chimère naît au point d'arrivée, tournée vers le nord : la caméra se tourne pour la voir de face, part tout
        // près d'elle et recule jusqu'à la distance de départ.
        private void OpenTown(World world, Catalog catalog, TextTable texts, PlacementSettings placement)
        {
            _confirmControls.Bind(null);
            _previewControls.Bind(null, null);
            _creationScreen.Hide();
            _appearancePreview.Hide();
            _atelier.Hide();
            _cameraRig.Show(world.Map);
            _cameraRig.Rotate(180f);
            _cameraRig.PullBack();
            _placementTool = new PlacementTool(world, _cameraRig.Camera, _placementPreview, placement);
            _buildBar.Show(catalog, texts, _placementTool, world);
            _dayBoard.Show(world.Clock, texts);
            _placeSheet.Show(world, texts);
            var hud = new HudArea(_buildBar.GetComponent<UnityEngine.UIElements.UIDocument>());
            _cameraControls.Bind(hud);
            _placementControls.Bind(_placementTool, hud);
            _chooseTool = new ChooseTool(world, _cameraRig.Camera, _structureViews);
            _chooseTool.Touched += _placeSheet.Touch;
            _chooseControls.Bind(_chooseTool, _placementTool, hud);
            _escapeControls.Bind(_placeSheet, _placementTool);
        }
    }
}
