using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Le journal : il écrit dans la console les événements clés du monde, pour qu'un Play court prouve que la
    /// simulation travaille (ARCHITECTURE, section 12). Il sert au développeur ; le joueur ne le voit jamais.
    /// </summary>
    public sealed class SimulationJournal
    {
        private const string Prefix = "DICIELA : ";

        private readonly World _world;

        public SimulationJournal(World world)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));

            _world = world;
            _world.Clock.DayStarted += OnDayStarted;
            _world.Clock.PhaseChanged += OnPhaseChanged;
            _world.Structures.StructurePlaced += OnStructurePlaced;
            _world.Structures.PlacementRefused += OnPlacementRefused;
            Relief relief = _world.Map.Relief;
            Debug.Log(Prefix + "monde créé, carte de " + relief.SizeX.ToString("0") + " × " + relief.SizeZ.ToString("0") +
                      " m (relief de " + relief.SamplesX + " × " + relief.SamplesZ + " points), " +
                      Describe(_world.Clock) + ".");
        }

        /// <summary>Se désabonne du monde et écrit où il en était.</summary>
        public void Stop()
        {
            _world.Clock.DayStarted -= OnDayStarted;
            _world.Clock.PhaseChanged -= OnPhaseChanged;
            _world.Structures.StructurePlaced -= OnStructurePlaced;
            _world.Structures.PlacementRefused -= OnPlacementRefused;
            Debug.Log(Prefix + "monde arrêté après " + _world.Clock.StepCount + " pas, " + Describe(_world.Clock) + ".");
        }

        private static void OnDayStarted(int day)
        {
            Debug.Log(Prefix + "jour " + day + ".");
        }

        private static void OnPhaseChanged(DayPhase phase)
        {
            Debug.Log(Prefix + PhaseName(phase) + ".");
        }

        private static void OnStructurePlaced(Structure structure)
        {
            Debug.Log(Prefix + "construction n° " + structure.Id + " (" + structure.Definition.Id + ") posée en " +
                      structure.Center + ", à " + structure.RotationDegrees + "°.");
        }

        private static void OnPlacementRefused(StructureDefinition definition, Vector2 center, float rotation,
            PlacementResult reason)
        {
            Debug.Log(Prefix + "pose refusée (" + definition.Id + " en " + center + ", à " + rotation + "°) : " + reason +
                      ".");
        }

        private static string Describe(Clock clock)
        {
            double hour = clock.HourOfDay;
            int hours = (int)hour;
            int minutes = (int)((hour - hours) * 60.0);
            return "jour " + clock.Day + ", " + hours.ToString("00") + " h " + minutes.ToString("00") +
                   " depuis le matin, " + PhaseName(clock.Phase);
        }

        private static string PhaseName(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Work:
                    return "travail";
                case DayPhase.Evening:
                    return "soirée";
                default:
                    return "nuit";
            }
        }
    }
}
