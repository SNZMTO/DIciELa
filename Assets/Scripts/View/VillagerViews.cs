using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les habitants, vus (ARCHITECTURE, section 11) : chacun habillé de son apparence (Dresser), à l'échelle de sa sorte
    /// (les animaux réduits pour passer les portes comme les humains ; décision 195). À l'ouverture, ceux
    /// qui sont déjà là ; ensuite, chaque arrivée, par l'événement de la simulation. À chaque image, le
    /// démarrage donne la fraction du pas en cours : chaque personnage se montre à cette fraction du chemin entre sa
    /// place au pas d'avant et celle du dernier pas (SAVOIR, 2.2), posé sur le sol, et se tourne vers où il va, sans
    /// à-coup. Le pack n'a pas d'animations : il glisse. Endormi, un habitant est chez lui : couché sur sa couche, s'il en
    /// a une (la Chimère au campement), sur le dos, la tête vers où le dit sa fiche ; sans couche, il y est entré : on ne
    /// le voit plus. Réveillé, il se relève devant sa porte, où la simulation l'a toujours tenu.
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
            public bool Lying;
            // La pose écrite en dernier : un habitant arrêté n'est pas réécrit à chaque image.
            public Vector2 ShownAt;
            public float ShownYaw;
            public bool Posed;
        }

        private readonly List<Shown> _shown = new List<Shown>();
        private Villagers _villagers;
        private Relief _relief;

        /// <summary>Montre les habitants de ce monde, couchés dans leur logis s'ils dorment, puis suit leurs arrivées.</summary>
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
            {
                if (!shown.Lying)
                    Place(shown, stepFraction, deltaSeconds);
            }
        }

        private void OnDestroy()
        {
            Hide();
        }

        private void OnVillagerArrived(Villager villager)
        {
            // Appelé pendant un pas de la simulation : une fiche mal réglée se dit, sans arrêter le monde.
            GameObject view = Dresser.Dress(villager.Appearance, transform);
            if (view == null)
            {
                Debug.LogError("DICIELA : l'habitant n° " + villager.Id + " ne se voit pas.", this);
                return;
            }

            view.name = villager.Kind.Id + " n° " + villager.Id;
            // Les animaux sont réduits pour passer les portes comme les humains (décision 195).
            view.transform.localScale = Vector3.one * villager.Kind.ModelScale;
            var shown = new Shown { Villager = villager, Transform = view.transform };
            _shown.Add(shown);
            ShowSleep(shown);
        }

        /// <summary>Il s'est endormi dans son logis, ou s'est réveillé.</summary>
        private void OnVillagerSleepChanged(Villager villager)
        {
            foreach (Shown shown in _shown)
            {
                if (ReferenceEquals(shown.Villager, villager))
                    ShowSleep(shown);
            }
        }

        // Endormi : couché sur la couche de son logis, s'il en a une, sinon caché ; éveillé : debout là où il est.
        private void ShowSleep(Shown shown)
        {
            Structure home = shown.Villager.IsAsleep ? HomeWithACouchOf(shown.Villager) : null;
            shown.Lying = home != null;
            shown.Posed = false;
            if (shown.Lying)
                LayDown(shown, home);
            else
                Place(shown, 1f, 0f);
            shown.Transform.gameObject.SetActive(!shown.Villager.IsAsleep || shown.Lying);
        }

        // Celui qui dort est chez lui, que la simulation dit ; chez lui, s'il a une couche.
        private Structure HomeWithACouchOf(Villager villager)
        {
            Structure home = _villagers.HomeOf(villager);
            return home != null && home.Definition.HasCouch ? home : null;
        }

        // Sur le dos, les pieds au repère de la couche, la tête vers où le dit la fiche, le visage vers le ciel.
        private void LayDown(Shown shown, Structure home)
        {
            StructureDefinition definition = home.Definition;
            Vector3 feet = ModelPose.PointOf(home.Placement, _relief, definition.CouchFeet);
            Vector3 head = ModelPose.Rotation(home.Placement) * Quaternion.Euler(0f, definition.CouchHeadingDegrees, 0f) *
                           Vector3.forward;
            shown.Transform.SetPositionAndRotation(feet, Quaternion.LookRotation(Vector3.up, head));
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
            if (shown.Posed && at == shown.ShownAt && shown.YawDegrees == shown.ShownYaw)
                return;
            shown.Transform.SetPositionAndRotation(_relief.GroundAt(at),
                Quaternion.Euler(0f, shown.YawDegrees, 0f));
            shown.ShownAt = at;
            shown.ShownYaw = shown.YawDegrees;
            shown.Posed = true;
        }
    }
}
