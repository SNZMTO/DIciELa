using System;
using System.Collections;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Diciela.Game
{
    /// <summary>
    /// Le démarrage : il charge la carte de sa session et en fait la scène active (sa lumière s'applique), la lit, crée
    /// le monde et le fait avancer par pas fixes, au plus un nombre borné de pas par image (ARCHITECTURE, sections 5 et
    /// 10). Le monde est recréé à chaque Play et libéré à la sortie : ce projet ne recharge ni le code ni la scène.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la simulation (Assets/Data/Settings).")]
        [SerializeField] private SimulationSettings _settings;

        [Tooltip("La fiche de session : quelle carte charger.")]
        [SerializeField] private SessionSettings _session;

        [Tooltip("La caméra de gestion, qui montre la carte quand le monde naît.")]
        [SerializeField] private CameraRig _cameraRig;

        private World _world;
        private FixedStepAccumulator _accumulator;
        private SimulationJournal _journal;
        private string _mapPath;
        private AsyncOperation _mapLoading;
        private Scene _mapScene;
        private bool _quitting;

        private void OnEnable()
        {
            if (_settings == null || _session == null || _cameraRig == null)
            {
                Debug.LogError("DICIELA : le démarrage n'a pas ses fiches (réglages, session) ou sa caméra ; le monde " +
                               "ne tourne pas.", this);
                enabled = false;
                return;
            }

            _quitting = false;
            Application.quitting += OnQuitting;
            StartCoroutine(LoadMapThenCreateWorld());
        }

        private IEnumerator LoadMapThenCreateWorld()
        {
            _mapPath = _session.MapScenePath;
            _mapLoading = SceneLoader.LoadAdditive(_mapPath);
            if (_mapLoading == null)
            {
                Debug.LogError("DICIELA : la carte « " + _mapPath + " » ne se charge pas.", this);
                enabled = false;
                yield break;
            }
            yield return _mapLoading;
            _mapLoading = null;

            _mapScene = SceneManager.GetSceneByPath(_mapPath);
            SceneManager.SetActiveScene(_mapScene);
            if (!TryCreateWorld(MapRoot.FindIn(_mapScene)))
                enabled = false;
        }

        private bool TryCreateWorld(MapRoot mapRoot)
        {
            if (mapRoot == null)
            {
                Debug.LogError("DICIELA : la carte « " + _mapScene.path + " » n'a pas de racine (MapRoot).", this);
                return false;
            }

            try
            {
                MapDescription map = MapReader.Read(mapRoot.Terrain.terrainData, mapRoot.transform.position);
                _world = new World(_settings, map);
                _accumulator = new FixedStepAccumulator(_settings);
            }
            catch (ArgumentException exception)
            {
                Debug.LogException(exception, this);
                _world = null;
                return false;
            }

            _journal = new SimulationJournal(_world);
            _cameraRig.Show(_world.Map.Relief);
            return true;
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

            if (_journal != null)
                _journal.Stop();
            _journal = null;
            _accumulator = null;
            _world = null;

            // En quittant le Play, Unity décharge tout lui-même ; sinon, la carte part avec son démarrage.
            if (!_quitting)
                UnloadMap();
            _mapLoading = null;
            _mapScene = default;
        }

        /// <summary>Décharge la carte ; si elle se charge encore, elle partira à la fin de son chargement.</summary>
        private void UnloadMap()
        {
            if (_mapLoading != null)
            {
                string mapPath = _mapPath;
                _mapLoading.completed += _ => SceneManager.UnloadSceneAsync(mapPath);
            }
            else if (_mapScene.IsValid() && _mapScene.isLoaded)
                SceneManager.UnloadSceneAsync(_mapScene);
        }

        private void OnQuitting()
        {
            _quitting = true;
        }
    }
}
