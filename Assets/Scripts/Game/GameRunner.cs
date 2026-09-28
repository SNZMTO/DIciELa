using System;
using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.UI;
using Diciela.View;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Le démarrage, racine de composition (ARCHITECTURE, sections 5, 10 et 11) : il charge la carte de sa session, crée
    /// le monde et la table des textes, montre le monde (caméra, constructions, barre) et lui relie les gestes ; puis il
    /// le fait avancer par pas fixes, au plus un nombre borné de pas par image. Tout est recréé à chaque Play et libéré
    /// à la sortie : ce projet ne recharge ni le code ni la scène.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        [Header("Fiches")]
        [SerializeField] private SimulationSettings _settings;
        [SerializeField] private SessionSettings _session;
        [SerializeField] private PlacementSettings _placementSettings;
        [SerializeField] private InterfaceSettings _interfaceSettings;
        [SerializeField] private Catalog _catalog;

        [Header("Vues et gestes de la scène")]
        [SerializeField] private CameraRig _cameraRig;
        [SerializeField] private CameraControls _cameraControls;
        [SerializeField] private StructureViews _structureViews;
        [SerializeField] private PlacementPreview _placementPreview;
        [SerializeField] private PlacementControls _placementControls;
        [SerializeField] private BuildBar _buildBar;

        private World _world;
        private TextTable _texts;
        private FixedStepAccumulator _accumulator;
        private SimulationJournal _journal;
        private PlacementTool _placementTool;
        private MapScene _map;
        private double _lastRealtime;
        private bool _quitting;

        /// <summary>
        /// Le monde en cours, que lisent les outils de l'éditeur (la planche des trajets) ; null tant qu'il n'est pas né,
        /// et après le Play.
        /// </summary>
        public World World => _world;

        private void OnEnable()
        {
            if (_settings == null || _session == null || _placementSettings == null || _interfaceSettings == null ||
                _catalog == null || _cameraRig == null || _cameraControls == null || _structureViews == null ||
                _placementPreview == null || _placementControls == null || _buildBar == null)
            {
                Debug.LogError("DICIELA : le démarrage n'a pas toutes ses fiches, ses vues et ses gestes (lancer " +
                               "« DICIELA/Scènes/Monter les scènes ») ; le monde ne tourne pas.", this);
                enabled = false;
                return;
            }

            _quitting = false;
            Application.quitting += OnQuitting;
            _map = new MapScene(_session.MapScenePath);
            StartCoroutine(LoadMapThenCreateWorld());
        }

        private IEnumerator LoadMapThenCreateWorld()
        {
            yield return _map.Load(this);
            if (_map.Description == null || !TryCreateWorld() || !TryCompose())
                enabled = false;
        }

        /// <summary>Le monde et la table des textes ; une fiche mal réglée s'écrit dans la console.</summary>
        private bool TryCreateWorld()
        {
            try
            {
                _world = new World(_settings, _map.Description);
                _accumulator = new FixedStepAccumulator(_settings);
                _lastRealtime = Time.realtimeSinceStartupAsDouble;
                _texts = TextTable.Parse(_interfaceSettings.Texts != null ? _interfaceSettings.Texts.text : null,
                    _interfaceSettings.Language);
                return true;
            }
            catch (ArgumentException exception)
            {
                Debug.LogException(exception, this);
                _world = null;
                return false;
            }
        }

        /// <summary>
        /// Montre le monde et lui relie les gestes ; un échec (une interface qui ne trouve pas ses éléments) s'écrit dans
        /// la console, et le monde ne tourne pas à moitié monté.
        /// </summary>
        private bool TryCompose()
        {
            try
            {
                _journal = new SimulationJournal(_world);
                _cameraRig.Show(_world.Map.Relief);
                _structureViews.Show(_world.Structures, _world.Map.Relief);
                _placementTool = new PlacementTool(_world, _cameraRig.Camera, _placementPreview, _placementSettings);
                _buildBar.Show(_catalog, _texts, _placementTool);
                _cameraControls.Bind(_buildBar);
                _placementControls.Bind(_placementTool, _buildBar);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return false;
            }
        }

        private void Update()
        {
            if (_world == null)
                return;

            // Le temps réel, pas Time.deltaTime, qu'Unity plafonne : seule la fiche borne les pas d'une image (décision 65).
            double now = Time.realtimeSinceStartupAsDouble;
            int steps = _accumulator.Accumulate(now - _lastRealtime);
            _lastRealtime = now;
            for (int i = 0; i < steps; i++)
                _world.Step();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Application.quitting -= OnQuitting;

            // Tout ce qui a pu être montré, même par une composition interrompue : chaque Hide accepte de n'avoir
            // rien à cacher.
            if (_world != null)
            {
                _placementControls.Bind(null, null);
                _cameraControls.Bind(null);
                _buildBar.Hide();
                if (_placementTool != null)
                {
                    _placementTool.Cancel();
                    _placementTool.Dispose();
                }
                _cameraRig.Hide();
                _structureViews.Hide();
            }
            _placementTool = null;
            if (_journal != null)
                _journal.Stop();
            _journal = null;
            _accumulator = null;
            _texts = null;
            _world = null;

            // En quittant le Play, Unity décharge tout lui-même ; sinon, la carte part avec son démarrage.
            if (!_quitting && _map != null)
                _map.Unload();
            _map = null;
        }

        private void OnQuitting()
        {
            _quitting = true;
        }
    }
}
