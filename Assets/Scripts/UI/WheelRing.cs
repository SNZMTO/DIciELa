using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'anneau de la roue (décisions 168, 176 et 221) : le fond plein écran qui prend le pointeur, ouvert, et le centre
    /// où la roue s'ouvre, ramené pour tenir dans l'écran, éventails compris (WheelMeasures) ; ses cases et l'éventail de
    /// la case visée, ce qu'on y vise et ce qu'on y prend, sont ses cibles (WheelTargets). Elle s'ouvre en ressort, ses
    /// cases l'une après l'autre, et se retourne comme une pièce quand elle change d'étage (décision 225). Un appui sur
    /// le fond la congédie ; la molette, sur lui, se dit. Fermée, elle reste en page, invisible.
    /// </summary>
    internal sealed class WheelRing : IDisposable
    {
        private const string ClosedClass = "roue-fermee";
        private const string EnterClass = "roue-entree";
        private const string FlipClass = "roue-retournee";

        private readonly VisualElement _wheel;
        private readonly VisualElement _center;
        private readonly WheelMeasures _measures;
        private readonly WheelTargets _targets;
        private readonly EventCallback<PointerDownEvent> _onBackground;
        private readonly EventCallback<WheelEvent> _onScroll;
        private Vector2 _openedAt;
        private Vector2 _centerPoint;
        private bool _held;

        public WheelRing(ToolWheelElements elements)
        {
            _wheel = elements.Wheel;
            _center = elements.Center;
            _measures = new WheelMeasures(_wheel);
            _targets = new WheelTargets(elements, _measures);
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
        public event Action<int, int> AimChanged
        {
            add => _targets.AimChanged += value;
            remove => _targets.AimChanged -= value;
        }

        /// <summary>Levé quand on prend une case (option -1) ou une option : au clic, ou au relâchement de la touche.</summary>
        public event Action<int, int> Picked
        {
            add => _targets.Picked += value;
            remove => _targets.Picked -= value;
        }

        /// <summary>Levé quand on tourne la molette sur la roue ouverte : +1 vers le haut, -1 vers le bas.</summary>
        public event Action<int> Scrolled;

        /// <summary>Levé quand on appuie sur le fond, à côté des cases.</summary>
        public event Action Dismissed;

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Vrai si l'anneau s'est ouvert par la touche tenue : on y vise par la direction du pointeur ; faux, on y clique
        /// (aussi après la case Outils, qui le garde ouvert).
        /// </summary>
        public bool IsHeld
        {
            get => _held;
            set
            {
                _held = value;
                _targets.ByHover = IsOpen && !value;
            }
        }

        /// <summary>Montre ces cases ; ouvert, il se retourne quand elles changent d'étage.</summary>
        public void Show(IReadOnlyList<WheelSlot> slots)
        {
            if (_targets.Show(slots) && IsOpen)
                HudMotion.Enter(_center, FlipClass);
        }

        /// <summary>Ouvre l'anneau autour de ce point du panneau, ramené pour qu'il tienne dans l'écran.</summary>
        public void Open(Vector2 at, bool held)
        {
            Rect room = _wheel.worldBound;
            _openedAt = at;
            _centerPoint = _measures.CenterFor(at, room, _targets.Slots);
            _center.style.left = _centerPoint.x - room.xMin;
            _center.style.top = _centerPoint.y - room.yMin;
            IsOpen = true;
            IsHeld = held;
            _wheel.RemoveFromClassList(ClosedClass);
            SetPickable(true);
            _targets.Aim(-1, -1);
            HudMotion.Enter(_center, EnterClass);
            _targets.Enter();
        }

        public void Close()
        {
            IsOpen = false;
            IsHeld = false;
            _wheel.AddToClassList(ClosedClass);
            SetPickable(false);
            _targets.Aim(-1, -1);
        }

        /// <summary>Ferme l'anneau et le débranche de ses éléments.</summary>
        public void Dispose()
        {
            Close();
            _wheel.UnregisterCallback(_onBackground);
            _wheel.UnregisterCallback(_onScroll);
        }

        /// <summary>Touche tenue, le pointeur en ce point du panneau : la case ou l'option visée suit.</summary>
        public void AimAt(Vector2 pointer)
        {
            _targets.AimAt(_openedAt, _centerPoint, pointer);
        }

        /// <summary>Touche tenue, au relâchement : prend ce qu'on vise, comme un clic ; faux si l'on ne vise rien.</summary>
        public bool PickAimed()
        {
            return _targets.PickAimed();
        }

        /// <summary>L'icône prise saute vers le pointeur, touche tenue ; au clic, elle s'efface sur place.</summary>
        public void Fly(int slot, int option)
        {
            _targets.Fly(slot, option, IsHeld);
        }

        private void SetPickable(bool pickable)
        {
            _wheel.pickingMode = pickable ? PickingMode.Position : PickingMode.Ignore;
            _targets.SetPickable(pickable);
        }
    }
}
