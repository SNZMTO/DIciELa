using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Le démarrage : il crée le monde à partir de la fiche de réglages et le fait avancer par pas fixes, au plus un
    /// nombre borné de pas par image (ARCHITECTURE, section 5). Le monde est recréé à chaque Play et libéré à la
    /// sortie : ce projet ne recharge ni le code ni la scène en entrant en Play.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        [Tooltip("La fiche des réglages de la simulation (Assets/Data/Settings).")]
        [SerializeField] private SimulationSettings _settings;

        private World _world;
        private FixedStepAccumulator _accumulator;
        private SimulationJournal _journal;

        private void OnEnable()
        {
            if (_settings == null)
            {
                Debug.LogError("DICIELA : le démarrage n'a pas de réglages de simulation ; le monde ne tourne pas.", this);
                enabled = false;
                return;
            }

            try
            {
                _world = new World(_settings);
                _accumulator = new FixedStepAccumulator(_settings);
            }
            catch (ArgumentException exception)
            {
                Debug.LogException(exception, this);
                _world = null;
                enabled = false;
                return;
            }

            _journal = new SimulationJournal(_world);
        }

        private void Update()
        {
            int steps = _accumulator.Accumulate(Time.deltaTime);
            for (int i = 0; i < steps; i++)
                _world.Step();
        }

        private void OnDisable()
        {
            if (_journal != null)
                _journal.Stop();

            _journal = null;
            _accumulator = null;
            _world = null;
        }
    }
}
