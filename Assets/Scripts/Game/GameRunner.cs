using System;
using System.Collections;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.View;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Le démarrage, racine de composition (ARCHITECTURE, sections 5 et 10) : il charge la carte de sa session, crée le
    /// monde, le montre (caméra, constructions) et lui relie les gestes ; puis il le fait avancer par pas fixes, au plus
    /// un nombre borné de pas par image. Le monde est recréé à chaque Play et libéré à la sortie : ce projet ne recharge
    /// ni le code ni la scène.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la simulation (Assets/Data/Settings).")]
        [SerializeField] private SimulationSettings _settings;

        [Tooltip("La fiche de session : quelle carte charger.")]
        [SerializeField] private SessionSettings _session;

        [Tooltip("La fiche des réglages de la pose (Assets/Data/Settings).")]
        [SerializeField] private PlacementSettings _placementSettings;

        [Tooltip("La caméra de gestion, qui montre la carte quand le monde naît.")]
        [SerializeField] private CameraRig _cameraRig;

        [Tooltip("Les constructions posées, vues.")]
        [SerializeField] private StructureViews _structureViews;

        [Tooltip("L'aperçu de la pose.")]
        [SerializeField] private PlacementPreview _placementPreview;

        [Tooltip("Les gestes de la pose.")]
        [SerializeField] private PlacementControls _placementControls;

        private World _world;
        private FixedStepAccumulator _accumulator;
        private SimulationJournal _journal;
        private PlacementTool _placementTool;
        private MapScene _map;
        private bool _quitting;

        private void OnEnable()
        {
            if (_settings == null || _session == null || _placementSettings == null || _cameraRig == null ||
                _structureViews == null || _placementPreview == null || _placementControls == null)
            {
                Debug.LogError("DICIELA : le démarrage n'a pas ses fiches, sa caméra ou ses vues ; le monde ne tourne pas.",
                    this);
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
            if (_map.Description == null || !TryCreateWorld())
            {
                enabled = false;
                yield break;
            }
            Compose();
        }

        private bool TryCreateWorld()
        {
            try
            {
                _world = new World(_settings, _map.Description);
                _accumulator = new FixedStepAccumulator(_settings);
                return true;
            }
            catch (ArgumentException exception)
            {
                Debug.LogException(exception, this);
                _world = null;
                return false;
            }
        }

        /// <summary>Montre le monde et lui relie les gestes.</summary>
        private void Compose()
        {
            _journal = new SimulationJournal(_world);
            _cameraRig.Show(_world.Map.Relief);
            _structureViews.Show(_world.Structures, _world.Map.Relief);
            _placementTool = new PlacementTool(_world, _cameraRig.Camera, _placementPreview, _placementSettings);
            _placementControls.Bind(_placementTool);
        }

        private void Update()
        {
            if (_world == null)
                return;

            int steps = _accumulator.Accumulate(Time.deltaTime);
            for (int i = 0; i < steps; i++)
                _world.Step();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Application.quitting -= OnQuitting;

            if (_placementTool != null)
            {
                _placementControls.Bind(null);
                _placementTool.Cancel();
            }
            _placementTool = null;
            if (_structureViews != null)
                _structureViews.Hide();
            if (_journal != null)
                _journal.Stop();
            _journal = null;
            _accumulator = null;
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
