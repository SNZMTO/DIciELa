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
    /// tourner ; puis, l'ordre de la créer parti, la ville (caméra, HUD, outils, gestes), la caméra tout près de la
    /// Chimère qui naît au point d'arrivée, puis reculée à sa distance de départ. Le monde (constructions, habitants,
    /// arbres, charrettes) est vu dès l'ouverture (WorldViews) ; la Place, en mode construction (TownPlaza) ; l'outil
    /// en main, à sa carte et à son curseur. Toucher un lieu ouvre sa fiche ; Échap ferme la fiche, puis la roue des
    /// outils, puis défait le geste en cours, puis lâche l'outil en main. Il délie et cache tout à la sortie.
    /// </summary>
    public sealed class GameStage : MonoBehaviour
    {
        [SerializeField] private CameraRig _cameraRig;
        [SerializeField] private CameraControls _cameraControls;
        [SerializeField] private StructureViews _structureViews;
        [SerializeField] private VillagerViews _villagerViews;
        [SerializeField] private NatureViews _natureViews;
        [SerializeField] private PlacementPreview _placementPreview;
        [SerializeField] private PlacementControls _placementControls;
        [SerializeField] private ChooseControls _chooseControls;
        [SerializeField] private ClearingControls _clearingControls;
        [SerializeField] private DayBoard _dayBoard;
        [SerializeField] private LocationSheet _locationSheet;
        [SerializeField] private StockPennants _stockPennants;
        [SerializeField] private TipBoard _tipBoard;
        [SerializeField] private EscapeControls _escapeControls;
        [SerializeField] private CreationScreen _creationScreen;
        [SerializeField] private AppearancePreview _appearancePreview;
        [SerializeField] private ConfirmControls _confirmControls;
        [SerializeField] private Atelier _atelier;
        [SerializeField] private PreviewControls _previewControls;
        [SerializeField] private ToolWheel _toolWheel;
        [SerializeField] private ToolControls _toolControls;
        [SerializeField] private PaintControls _paintControls;
        [SerializeField] private PlazaZoneView _plazaZoneView;
        [SerializeField] private PaintToolPreview _paintToolPreview;

        // Le portrait de l'astuce : 48 px de l'interface, en triple résolution.
        private const int PortraitPixels = 144;

        private TownTools _tools;
        private TownPlaza _plaza;
        private ToolCard _toolCard;
        private ToolCursor _toolCursor;
        private SelectionFrame _selectionFrame;
        private Texture2D _portrait;
        private Tooltips _tooltips;
        private WorldViews _worldViews;

        /// <summary>Vrai si la scène a toutes ses vues et tous ses gestes.</summary>
        public bool IsComplete => _cameraRig != null && _cameraControls != null && _structureViews != null &&
                                  _villagerViews != null && _natureViews != null && _placementPreview != null &&
                                  _placementControls != null && _clearingControls != null &&
                                  _chooseControls != null && _dayBoard != null &&
                                  _locationSheet != null && _stockPennants != null && _tipBoard != null &&
                                  _escapeControls != null &&
                                  _creationScreen != null &&
                                  _appearancePreview != null && _confirmControls != null && _atelier != null &&
                                  _previewControls != null && _toolWheel != null && _toolControls != null &&
                                  _paintControls != null && _plazaZoneView != null && _paintToolPreview != null;

        /// <summary>
        /// Montre ce monde, dont les arbres sont peints sur le Terrain de cette carte, et ouvre l'écran de création ; à la
        /// validation, ouvre la ville, puis appelle started. Si la ville ne s'ouvre pas (une interface qui ne trouve pas
        /// ses éléments), l'erreur s'écrit dans la console et failed est appelé : rien ne tourne à moitié ouvert.
        /// </summary>
        internal void Open(World world, Catalog catalog, TextTable texts, PlacementSettings placement, MapScene map,
            Action started, Action failed)
        {
            _worldViews = new WorldViews(_structureViews, _villagerViews, _natureViews, transform);
            _worldViews.Show(world, map, placement);
            _atelier.Show();
            _appearancePreview.Stand(_atelier.Spot);
            _creationScreen.Show(catalog, texts, _appearancePreview, world, () =>
            {
                if (TryOpenTown(world, catalog, texts, placement))
                    started();
                else
                    failed();
            });
            _previewControls.Bind(_appearancePreview, _creationScreen);
            _confirmControls.Bind(_creationScreen.Confirm);
        }

        /// <summary>Donne aux habitants et aux charrettes vus, et à la journée, la fraction du pas en cours.</summary>
        public void Follow(float stepFraction, float deltaSeconds)
        {
            _worldViews?.Follow(stepFraction, deltaSeconds);
            _dayBoard.Follow(stepFraction);
        }

        /// <summary>Délie et cache tout ce qui a pu être montré : chaque Hide accepte de n'avoir rien à cacher.</summary>
        public void Close()
        {
            _confirmControls.Bind(null);
            _previewControls.Bind(null, null);
            _placementControls.Bind(null, null);
            _chooseControls.Bind(null, null, null);
            _clearingControls.Bind(null, null, null);
            _toolControls.Bind(null, null);
            _escapeControls.Bind();
            _plaza?.Dispose();
            _plaza = null;
            _toolCard?.Hide();
            _toolCard = null;
            _toolCursor?.Hide();
            _toolCursor = null;
            _selectionFrame?.Dispose();
            _selectionFrame = null;
            if (_tools != null)
                _tools.Choose.Touched -= _locationSheet.Touch;
            _cameraControls.Bind(null);
            _creationScreen.Hide();
            _appearancePreview.Hide();
            _atelier.Hide();
            _toolWheel.Hide();
            _dayBoard.Hide();
            _locationSheet.Hide();
            _stockPennants.Hide();
            _tipBoard.Hide();
            _tooltips?.Hide();
            _tooltips = null;
            if (_portrait != null)
                Destroy(_portrait);
            _portrait = null;
            _tools?.Dispose();
            _tools = null;
            _cameraRig.Hide();
            _worldViews?.Hide();
            _worldViews = null;
        }

        // La Chimère naît au point d'arrivée, tournée vers le nord : la caméra se tourne pour la voir de face, part tout
        // près d'elle et recule jusqu'à la distance de départ.
        private bool TryOpenTown(World world, Catalog catalog, TextTable texts, PlacementSettings placement)
        {
            try
            {
                OpenTown(world, catalog, texts, placement);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return false;
            }
        }

        private void OpenTown(World world, Catalog catalog, TextTable texts, PlacementSettings placement)
        {
            _confirmControls.Bind(null);
            _previewControls.Bind(null, null);
            _creationScreen.Hide();
            // Le portrait de l'astuce, pris à l'atelier avant qu'il se cache (décision 154).
            _portrait = _appearancePreview.TakePortrait(PortraitPixels);
            _appearancePreview.Hide();
            _atelier.Hide();
            _cameraRig.Show(world.Map);
            _cameraRig.Rotate(180f);
            _cameraRig.PullBack();
            _tools = new TownTools(world, catalog, _cameraRig.Camera, _placementPreview, _paintToolPreview, placement,
                _natureViews, _structureViews);
            var document = _toolWheel.GetComponent<UnityEngine.UIElements.UIDocument>();
            _tooltips = new Tooltips(document);
            _toolWheel.Show(texts, _toolControls, catalog, world, _tools.Placement, _tools.Clearing, _tools.Move,
                _tools.Paths, _tools.Plaza, _tooltips);
            _toolCard = new ToolCard(document);
            _toolCard.Show(texts, _tools.Placement, _tools.Clearing, _tools.Move, _tools.Paths, _tools.Plaza, world);
            _dayBoard.Show(world.Clock, texts, _tooltips);
            _stockPennants.Show(world, catalog, texts, _tooltips);
            _locationSheet.Show(world, texts);
            _tipBoard.Show(world, texts, _portrait);
            var hud = new HudArea(document);
            _cameraControls.Bind(hud);
            _placementControls.Bind(_tools.Placement, hud, _tools.Move);
            _tools.Choose.Touched += _locationSheet.Touch;
            _selectionFrame = new SelectionFrame(document);
            _clearingControls.Bind(_tools.Clearing, _selectionFrame, hud);
            _chooseControls.Bind(_tools.Choose, _tools.Hand, hud);
            _plaza = new TownPlaza(world, _tools, _plazaZoneView, _paintControls, hud);
            _toolCursor = new ToolCursor(document);
            _toolCursor.Show(_tools.Placement, _tools.Clearing, _tools.Move, _tools.Paths, _tools.Plaza, hud);
            _toolControls.Bind(_toolWheel, _toolCursor);
            _escapeControls.Bind(_locationSheet, _toolWheel, _tools.Hand);
        }
    }
}
