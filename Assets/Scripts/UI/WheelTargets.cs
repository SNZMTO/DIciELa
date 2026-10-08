using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les cibles de la roue (décisions 176 et 221) : ses cases à leur place sur le cercle (WheelSlots) et l'éventail de
    /// la case visée (WheelFan), ce qu'on y vise et ce qu'on y prend. Touche tenue, on vise une case par la direction où
    /// l'on bouge le pointeur depuis le point d'ouverture, une option en poussant jusqu'à l'éventail ; au clic, par le
    /// survol. L'icône prise saute vers le pointeur (WheelFlight) ; un éventail neuf se déplie depuis sa case.
    /// </summary>
    internal sealed class WheelTargets
    {
        private readonly WheelSlots _felts;
        private readonly WheelFan _fan;
        private readonly WheelFlight _flight;
        private readonly WheelMeasures _measures;
        private readonly List<Vector2> _slotCenters = new List<Vector2>();
        private IReadOnlyList<WheelSlot> _slots = Array.Empty<WheelSlot>();
        private Vector2 _pointer;
        private int _fanSlot = -1;
        private bool _pickable;

        public WheelTargets(ToolWheelElements elements, WheelMeasures measures)
        {
            _measures = measures;
            _felts = new WheelSlots(elements.Slots);
            _felts.Hovered += slot => Hover(slot, -1);
            _felts.Clicked += slot => Picked?.Invoke(slot, -1);
            _fan = new WheelFan(elements.Fan);
            _fan.Hovered += option => Hover(_fanSlot, option);
            _fan.Clicked += option => Picked?.Invoke(_fanSlot, option);
            _flight = new WheelFlight(elements.Flight);
        }

        /// <summary>Levé quand la case visée ou l'option visée change : leurs rangs, -1 pour aucune.</summary>
        public event Action<int, int> AimChanged;

        /// <summary>Levé quand on prend une case (option -1) ou une option : au clic, ou au relâchement de la touche.</summary>
        public event Action<int, int> Picked;

        public IReadOnlyList<WheelSlot> Slots => _slots;
        public int AimedSlot { get; private set; } = -1;
        public int AimedOption { get; private set; } = -1;

        /// <summary>Vrai si l'on vise au survol : la roue ouverte au clic, pas touche tenue.</summary>
        public bool ByHover { get; set; }

        /// <summary>Montre ces cases : refaites si leurs places changent (vrai), sinon mises à jour, la visée gardée.</summary>
        public bool Show(IReadOnlyList<WheelSlot> slots)
        {
            _slots = slots;
            bool rebuilt = _felts.Show(slots, _measures.SlotRadius);
            if (rebuilt)
            {
                _fanSlot = -1;
                _fan.Clear();
                Aim(-1, -1);
            }
            else if (_fanSlot >= 0)
                ShowFan(_fanSlot, false);
            _felts.SetPickable(_pickable);
            return rebuilt;
        }

        /// <summary>Les cases paraissent l'une après l'autre (décision 225).</summary>
        public void Enter()
        {
            _felts.Enter();
        }

        /// <summary>
        /// Touche tenue, le pointeur en ce point du panneau : l'option de l'éventail ouvert s'il y pousse, sinon la case
        /// dans sa direction depuis le point d'ouverture ; rien tant qu'il reste près de ce point.
        /// </summary>
        public void AimAt(Vector2 openedAt, Vector2 center, Vector2 pointer)
        {
            _pointer = pointer;
            if (_fanSlot >= 0)
            {
                int option = WheelGeometry.AimedOption(center, _slots[_fanSlot].Place * 45f,
                    _slots[_fanSlot].Options.Count, _measures.FanStep, _measures.FanThreshold, pointer);
                if (option >= 0)
                {
                    Aim(_fanSlot, option);
                    return;
                }
            }
            _slotCenters.Clear();
            foreach (WheelSlot slot in _slots)
                _slotCenters.Add(center + WheelGeometry.OnCircle(slot.Place * 45f, _measures.SlotRadius));
            Aim(WheelGeometry.Aimed(openedAt, center, _slotCenters, pointer), -1);
        }

        /// <summary>Touche tenue, au relâchement : prend ce qu'on vise, comme un clic ; faux si l'on ne vise rien.</summary>
        public bool PickAimed()
        {
            if (AimedSlot < 0)
                return false;
            Picked?.Invoke(AimedSlot, AimedOption);
            return true;
        }

        /// <summary>L'icône de cette case ou de cette option saute vers le pointeur, ou s'efface sur place.</summary>
        public void Fly(int slot, int option, bool towardPointer)
        {
            VisualElement face = option >= 0 && slot == _fanSlot ? _fan.FaceAt(option) : _felts.FaceAt(slot);
            if (face != null)
                _flight.Launch(face, towardPointer ? _pointer : face.worldBound.center);
        }

        // Marque la case et l'option visées, ouvre l'éventail de la case si elle a des options, et le dit si ça change.
        public void Aim(int slot, int option)
        {
            if (slot >= _slots.Count)
                slot = -1;
            if (slot != _fanSlot)
            {
                if (slot >= 0 && _slots[slot].Options.Count > 0)
                    ShowFan(slot, true);
                else
                {
                    _fanSlot = -1;
                    _fan.Clear();
                }
            }
            _fan.Aim(slot == _fanSlot ? option : -1);
            _felts.Aim(slot);
            if (slot == AimedSlot && option == AimedOption)
                return;
            AimedSlot = slot;
            AimedOption = option;
            AimChanged?.Invoke(slot, option);
        }

        public void SetPickable(bool pickable)
        {
            _pickable = pickable;
            _felts.SetPickable(pickable);
            _fan.SetPickable(pickable);
        }

        private void Hover(int slot, int option)
        {
            if (ByHover && slot >= 0)
                Aim(slot, option);
        }

        private void ShowFan(int slot, bool unfold)
        {
            _fanSlot = slot;
            _fan.Show(_slots[slot].Options, _slots[slot].Place * 45f, _measures.FanRadius, _measures.FanStep,
                _slots[slot].Pastel, unfold ? _measures.SlotRadius : 0f);
            _fan.SetPickable(_pickable);
        }
    }
}
