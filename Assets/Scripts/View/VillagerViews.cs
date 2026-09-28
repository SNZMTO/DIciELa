using System;
using System.Collections.Generic;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les habitants, vus (ARCHITECTURE, section 11) : chacun par le personnage du pack que dit sa sorte, tel quel. À
    /// l'ouverture, ceux qui sont déjà là ; ensuite, chaque arrivée, par l'événement de la simulation. À chaque image, le
    /// démarrage donne la fraction du pas en cours : chaque personnage se montre à cette fraction du chemin entre sa
    /// place au pas d'avant et celle du dernier pas (SAVOIR, 2.2), posé sur le sol, et se tourne vers où il va, sans
    /// à-coup. Le pack n'a pas d'animations : il glisse. Endormi, il est entré dans son logis : on ne le voit plus
    /// jusqu'à ce qu'il se réveille.
    /// </summary>
    public sealed class VillagerViews : MonoBehaviour
    {
        // En deçà de ce déplacement en un pas, en mètres, un habitant ne change pas de direction : il est arrêté.
        private const float TurnThresholdMeters = 1e-4f;

        private sealed class Shown
        {
            public Villager Villager;
            public Transform Transform;
            public float YawDegrees;
        }

        private readonly List<Shown> _shown = new List<Shown>();
        private Villagers _villagers;
        private Relief _relief;

        /// <summary>Montre les habitants de ce monde, puis suit leurs arrivées.</summary>
        public void Show(Villagers villagers, Relief relief)
        {
            if (ReferenceEquals(villagers, null))
                throw new ArgumentNullException(nameof(villagers));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Hide();
            _villagers = villagers;
            _relief = relief;
            foreach (Villager villager in villagers.All)
                OnVillagerArrived(villager);
            villagers.VillagerArrived += OnVillagerArrived;
            villagers.VillagerFellAsleep += OnVillagerSleepChanged;
            villagers.VillagerWokeUp += OnVillagerSleepChanged;
        }

        /// <summary>Cesse de suivre le monde et retire ce qu'on montrait.</summary>
        public void Hide()
        {
            if (_villagers != null)
            {
                _villagers.VillagerArrived -= OnVillagerArrived;
                _villagers.VillagerFellAsleep -= OnVillagerSleepChanged;
                _villagers.VillagerWokeUp -= OnVillagerSleepChanged;
            }
            _villagers = null;
            _relief = null;
            foreach (Shown shown in _shown)
                Destroy(shown.Transform.gameObject);
            _shown.Clear();
        }

        /// <summary>
        /// Place chaque personnage à cette fraction du pas en cours (0 : où il était au pas d'avant, 1 : au dernier pas),
        /// tourné d'autant que le permettent ces secondes.
        /// </summary>
        public void Follow(float stepFraction, float deltaSeconds)
        {
            foreach (Shown shown in _shown)
                Place(shown, stepFraction, deltaSeconds);
        }

        private void OnDestroy()
        {
            Hide();
        }

        private void OnVillagerArrived(Villager villager)
        {
            // Appelé pendant un pas de la simulation : une fiche mal réglée se dit, sans arrêter le monde.
            GameObject model = villager.Kind.Model;
            if (model == null)
            {
                Debug.LogError("DICIELA : la sorte d'habitant « " + villager.Kind.Id + " » n'a pas de personnage ; " +
                               "l'habitant n° " + villager.Id + " ne se voit pas.", this);
                return;
            }

            GameObject view = Instantiate(model, transform, false);
            view.name = villager.Kind.Id + " n° " + villager.Id;
            var shown = new Shown { Villager = villager, Transform = view.transform };
            _shown.Add(shown);
            Place(shown, 1f, 0f);
            view.SetActive(!villager.IsAsleep);
        }

        /// <summary>Il s'est endormi dans son logis, ou s'est réveillé : on le cache, ou on le montre de nouveau.</summary>
        private void OnVillagerSleepChanged(Villager villager)
        {
            foreach (Shown shown in _shown)
            {
                if (ReferenceEquals(shown.Villager, villager))
                    shown.Transform.gameObject.SetActive(!villager.IsAsleep);
            }
        }

        private void Place(Shown shown, float stepFraction, float deltaSeconds)
        {
            Villager villager = shown.Villager;
            Vector2 moved = villager.Position - villager.PreviousPosition;
            if (moved.sqrMagnitude > TurnThresholdMeters * TurnThresholdMeters)
            {
                // L'angle se compte depuis le nord (+z), vers l'est : celui d'une rotation d'Unity autour de la verticale.
                float heading = Mathf.Atan2(moved.x, moved.y) * Mathf.Rad2Deg;
                shown.YawDegrees = Mathf.MoveTowardsAngle(shown.YawDegrees, heading,
                    villager.Kind.TurnDegreesPerSecond * deltaSeconds);
            }

            Vector2 at = Vector2.LerpUnclamped(villager.PreviousPosition, villager.Position, stepFraction);
            shown.Transform.SetPositionAndRotation(new Vector3(at.x, _relief.HeightAt(at.x, at.y), at.y),
                Quaternion.Euler(0f, shown.YawDegrees, 0f));
        }
    }
}
