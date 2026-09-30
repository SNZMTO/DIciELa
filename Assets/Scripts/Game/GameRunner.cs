using System;
using System.Collections;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Le démarrage, racine de composition (ARCHITECTURE, sections 5, 10 et 11) : il charge la carte de sa session, crée
    /// le monde et lit la table des textes ; sa scène (GameStage) montre le monde et ouvre l'écran de création. Le temps
    /// part quand l'ordre « créer la Chimère » est parti (GDD, section 6) : le premier pas se joue tout de suite, et la
    /// Chimère prend la place de son aperçu dans la même image. Ensuite, il fait avancer le monde par pas fixes, au plus
    /// un nombre borné de pas par image, et donne aux habitants vus la fraction du pas en cours. Tout est recréé à chaque
    /// Play et libéré à la sortie : ce projet ne recharge ni le code ni la scène.
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
        [SerializeField] private GameStage _stage;

        private World _world;
        private TextTable _texts;
        private FixedStepAccumulator _accumulator;
        private SimulationJournal _journal;
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
                _catalog == null || _catalog.ChimeraKind == null || _stage == null || !_stage.IsComplete)
            {
                Debug.LogError("DICIELA : le démarrage n'a pas toutes ses fiches, ses vues et ses gestes (lancer " +
                               "« DICIELA/Scènes/Monter les scènes ») ; le monde ne tourne pas.", this);
                enabled = false;
                return;
            }

            _quitting = false;
            Application.quitting += OnQuitting;
            _map = new MapScene(_session.MapScenePath, _catalog.Nature);
            StartCoroutine(LoadMapThenCreateWorld());
        }

        private IEnumerator LoadMapThenCreateWorld()
        {
            yield return _map.Load(this);
            if (_map.Description == null || !TryCreateWorld() || !TryOpen())
                enabled = false;
        }

        /// <summary>Le monde et la table des textes ; une fiche mal réglée s'écrit dans la console.</summary>
        private bool TryCreateWorld()
        {
            try
            {
                _world = new World(_settings, _map.Description, _catalog);
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
        /// Montre le monde et ouvre l'écran de création ; un échec (une interface qui ne trouve pas ses éléments) s'écrit
        /// dans la console, et le monde ne tourne pas à moitié monté.
        /// </summary>
        private bool TryOpen()
        {
            try
            {
                _journal = new SimulationJournal(_world);
                _stage.Open(_world, _catalog, _texts, _placementSettings, _map, StartTime);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return false;
            }
        }

        /// <summary>L'ordre « créer la Chimère » est parti : le temps part, et son premier pas la fait naître.</summary>
        private void StartTime()
        {
            _accumulator = new FixedStepAccumulator(_settings);
            _lastRealtime = Time.realtimeSinceStartupAsDouble;
            _world.Step();
        }

        private void Update()
        {
            if (_world == null || _accumulator == null)
                return;

            // Le temps réel, pas Time.deltaTime, qu'Unity plafonne : seule la fiche borne les pas d'une image (décision 65).
            double now = Time.realtimeSinceStartupAsDouble;
            int steps = _accumulator.Accumulate(now - _lastRealtime);
            _lastRealtime = now;
            for (int i = 0; i < steps; i++)
                _world.Step();
            _stage.Follow(_accumulator.StepFraction, Time.deltaTime);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Application.quitting -= OnQuitting;

            // Tout ce qui a pu être montré, même par une ouverture interrompue.
            if (_world != null)
                _stage.Close();
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
