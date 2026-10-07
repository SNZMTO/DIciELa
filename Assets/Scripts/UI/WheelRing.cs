using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'anneau de la roue (décisions 168, 176 et 221) : ses cases autour d'un centre (WheelSlots), l'éventail de la case
    /// visée (WheelFan), à leurs mesures (WheelMeasures), et le fond plein écran qui prend le pointeur, ouvert. Il s'ouvre en
    /// un point du panneau, ramené pour tenir dans l'écran, éventails compris. Touche tenue, on vise une case par la
    /// direction où l'on bouge le pointeur depuis ce point, une option en poussant jusqu'à l'éventail ; au clic, par le
    /// survol. Un appui sur le fond le congédie ; la molette, sur lui, se dit. Fermé, il reste en page, invisible.
    /// </summary>
    internal sealed class WheelRing : IDisposable
    {
        private const string ClosedClass = "roue-fermee";

        private readonly VisualElement _wheel;
        private readonly VisualElement _center;
        private readonly WheelSlots _felts;
        private readonly WheelFan _fan;
        private readonly WheelMeasures _measures;
        private readonly List<Vector2> _slotCenters = new List<Vector2>();
        private readonly EventCallback<PointerDownEvent> _onBackground;
        private readonly EventCallback<WheelEvent> _onScroll;
        private IReadOnlyList<WheelSlot> _slots = Array.Empty<WheelSlot>();
        private Vector2 _openedAt;
        private Vector2 _centerPoint;
        private int _fanSlot = -1;

        public WheelRing(VisualElement wheel, VisualElement center, VisualElement slots, VisualElement fan)
        {
            _wheel = wheel;
            _center = center;
            _measures = new WheelMeasures(wheel);
            _felts = new WheelSlots(slots);
            _felts.Hovered += slot => AimByHover(slot, -1);
            _felts.Clicked += slot => Picked?.Invoke(slot, -1);
            _fan = new WheelFan(fan);
            _fan.Hovered += option => AimByHover(_fanSlot, option);
            _fan.Clicked += option => Picked?.Invoke(_fanSlot, option);
            _onBackground = e =>
            {
                if (e.target == _wheel)
                    Dismissed?.Invoke();
            };
            _onScroll = e =>
            {
                if (IsOpen && Mathf.Abs(e.delta.y) > 0f)
                    Scrolled?.Invoke(e.delta.y < 0f ? 1 : -1);
                e.StopPropagation();
            };
            _wheel.RegisterCallback(_onBackground);
            _wheel.RegisterCallback(_onScroll);
        }

        /// <summary>Levé quand la case visée ou l'option visée change : leurs rangs, -1 pour aucune.</summary>
        public event Action<int, int> AimChanged;

        /// <summary>Levé quand on clique une case (option -1) ou un jeton : leurs rangs.</summary>
        public event Action<int, int> Picked;

        /// <summary>Levé quand on tourne la molette sur la roue ouverte : +1 vers le haut, -1 vers le bas.</summary>
        public event Action<int> Scrolled;

        /// <summary>Levé quand on appuie sur le fond, à côté des cases.</summary>
        public event Action Dismissed;

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Vrai si l'anneau s'est ouvert par la touche tenue : on y vise par la direction du pointeur ; faux, on y clique
        /// (aussi après la case Outils, qui le garde ouvert).
        /// </summary>
        public bool IsHeld { get; set; }

        public int AimedSlot { get; private set; } = -1;
        public int AimedOption { get; private set; } = -1;

        /// <summary>Montre ces cases : refaites si leurs places changent, sinon mises à jour sur place, la visée gardée.</summary>
        public void Show(IReadOnlyList<WheelSlot> slots)
        {
            _slots = slots;
            if (_felts.Show(slots, _measures.SlotRadius))
            {
                _fanSlot = -1;
                _fan.Clear();
                Aim(-1, -1);
            }
            else if (_fanSlot >= 0)
                ShowFan(_fanSlot);
            _felts.SetPickable(IsOpen);
        }

        /// <summary>Ouvre l'anneau autour de ce point du panneau, ramené pour qu'il tienne dans l'écran.</summary>
        public void Open(Vector2 at, bool held)
        {
            Rect room = _wheel.worldBound;
            float margin = _measures.Margin;
            var inside = Rect.MinMaxRect(room.xMin + margin, room.yMin + margin, room.xMax - margin, room.yMax - margin);
            float reach = _measures.Reach(_slots);
            _openedAt = at;
            _centerPoint = WheelGeometry.Clamped(at, new Rect(-reach, -reach, reach * 2f, reach * 2f), inside);
            _center.style.left = _centerPoint.x - room.xMin;
            _center.style.top = _centerPoint.y - room.yMin;
            IsOpen = true;
            IsHeld = held;
            _wheel.RemoveFromClassList(ClosedClass);
            SetPickable(true);
            Aim(-1, -1);
        }

        public void Close()
        {
            IsOpen = false;
            IsHeld = false;
            _wheel.AddToClassList(ClosedClass);
            SetPickable(false);
            Aim(-1, -1);
        }

        /// <summary>Ferme l'anneau et le débranche de ses éléments.</summary>
        public void Dispose()
        {
            Close();
            _wheel.UnregisterCallback(_onBackground);
            _wheel.UnregisterCallback(_onScroll);
        }

        /// <summary>
        /// Touche tenue, le pointeur en ce point du panneau : l'option de l'éventail ouvert s'il y pousse, sinon la case
        /// dans sa direction depuis le point d'ouverture ; rien tant qu'il reste près de ce point.
        /// </summary>
        public void AimAt(Vector2 pointer)
        {
            if (_fanSlot >= 0)
            {
                int option = WheelGeometry.AimedOption(_centerPoint, _slots[_fanSlot].Place * 45f,
                    _slots[_fanSlot].Options.Count, _measures.FanStep, _measures.FanThreshold, pointer);
                if (option >= 0)
                {
                    Aim(_fanSlot, option);
                    return;
                }
            }
            _slotCenters.Clear();
            foreach (WheelSlot slot in _slots)
                _slotCenters.Add(_centerPoint + WheelGeometry.OnCircle(slot.Place * 45f, _measures.SlotRadius));
            Aim(WheelGeometry.Aimed(_openedAt, _centerPoint, _slotCenters, pointer), -1);
        }

        private void AimByHover(int slot, int option)
        {
            if (IsOpen && !IsHeld && slot >= 0)
                Aim(slot, option);
        }

        // Marque la case et l'option visées, ouvre l'éventail de la case si elle a des options, et le dit si ça change.
        private void Aim(int slot, int option)
        {
            if (slot >= _slots.Count)
                slot = -1;
            if (slot != _fanSlot)
            {
                if (slot >= 0 && _slots[slot].Options.Count > 0)
                    ShowFan(slot);
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

        private void ShowFan(int slot)
        {
            _fanSlot = slot;
            _fan.Show(_slots[slot].Options, _slots[slot].Place * 45f, _measures.FanRadius, _measures.FanStep,
                _slots[slot].Pastel);
            _fan.SetPickable(IsOpen);
        }

        private void SetPickable(bool pickable)
        {
            _wheel.pickingMode = pickable ? PickingMode.Position : PickingMode.Ignore;
            _felts.SetPickable(pickable);
            _fan.SetPickable(pickable);
        }
    }
}
