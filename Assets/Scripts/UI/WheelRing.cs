using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'anneau de la roue des outils (décision 168) : son plateau et les outils autour d'un centre, à la place que leur
    /// donne la feuille de style, et le fond plein écran qui prend le pointeur tant que l'anneau est ouvert. Il s'ouvre en
    /// un point du panneau, ramené pour tenir dans l'écran, à la marge des blocs du HUD ; touche tenue, on y vise par la
    /// direction du pointeur, au clic par le survol, et on clique un outil ; un appui sur le fond le congédie. Fermé, il
    /// reste en page, invisible et sans prise : il se mesure avant de s'ouvrir. Il se branche sur ses éléments à sa
    /// naissance et s'en débranche quand on le libère (Dispose).
    /// </summary>
    internal sealed class WheelRing : IDisposable
    {
        private const string ClosedClass = "roue-fermee";
        private const string AimedClass = "vise";
        // L'écart que la roue garde avec le bord de l'écran, comme les blocs du HUD (Hud.uss).
        private static readonly CustomStyleProperty<float> EdgeMargin = new CustomStyleProperty<float>("--marge");

        private readonly VisualElement _wheel;
        private readonly VisualElement _center;
        private readonly VisualElement _plate;
        private readonly FeltButton[] _tools;
        private readonly List<Vector2> _toolCenters = new List<Vector2>();
        private readonly EventCallback<PointerDownEvent> _onBackground;
        private readonly EventCallback<PointerEnterEvent>[] _onEnter;
        private readonly EventCallback<PointerLeaveEvent>[] _onLeave;
        private readonly Action[] _onClicked;
        private Vector2 _centerPoint;

        public WheelRing(VisualElement wheel, VisualElement center, VisualElement plate, FeltButton[] tools)
        {
            _wheel = wheel;
            _center = center;
            _plate = plate;
            _tools = tools;
            _onBackground = e =>
            {
                if (e.target == _wheel)
                    Dismissed?.Invoke();
            };
            _wheel.RegisterCallback(_onBackground);
            _onEnter = new EventCallback<PointerEnterEvent>[_tools.Length];
            _onLeave = new EventCallback<PointerLeaveEvent>[_tools.Length];
            _onClicked = new Action[_tools.Length];
            for (int i = 0; i < _tools.Length; i++)
            {
                int tool = i;
                _onEnter[i] = _ => AimByHover(tool);
                _onLeave[i] = _ => AimByHover(-1);
                _onClicked[i] = () => Picked?.Invoke(tool);
                _tools[i].RegisterCallback(_onEnter[i]);
                _tools[i].RegisterCallback(_onLeave[i]);
                _tools[i].Clicked += _onClicked[i];
            }
        }

        /// <summary>Levé quand l'outil visé change : son rang, ou -1.</summary>
        public event Action<int> AimChanged;

        /// <summary>Levé quand on appuie sur le fond, à côté des outils.</summary>
        public event Action Dismissed;

        /// <summary>Levé quand on clique un outil : son rang.</summary>
        public event Action<int> Picked;

        public bool IsOpen { get; private set; }

        /// <summary>Vrai si l'anneau s'est ouvert par la touche tenue : on y vise par la direction du pointeur.</summary>
        public bool IsHeld { get; private set; }

        /// <summary>Ouvre l'anneau autour de ce point du panneau, ramené pour qu'il tienne dans l'écran.</summary>
        public void Open(Vector2 at, bool held)
        {
            Rect room = _wheel.worldBound;
            float margin = _wheel.customStyle.TryGetValue(EdgeMargin, out float value) ? value : 0f;
            var inside = Rect.MinMaxRect(room.xMin + margin, room.yMin + margin, room.xMax - margin, room.yMax - margin);
            _centerPoint = WheelGeometry.Clamped(at, Extent(), inside);
            _center.style.left = _centerPoint.x - room.xMin;
            _center.style.top = _centerPoint.y - room.yMin;
            IsOpen = true;
            IsHeld = held;
            _wheel.RemoveFromClassList(ClosedClass);
            SetPickable(true);
            Aim(-1);
        }

        public void Close()
        {
            IsOpen = false;
            IsHeld = false;
            _wheel.AddToClassList(ClosedClass);
            SetPickable(false);
            Aim(-1);
        }

        /// <summary>Ferme l'anneau et le débranche de ses éléments.</summary>
        public void Dispose()
        {
            Close();
            _wheel.UnregisterCallback(_onBackground);
            for (int i = 0; i < _tools.Length; i++)
            {
                _tools[i].UnregisterCallback(_onEnter[i]);
                _tools[i].UnregisterCallback(_onLeave[i]);
                _tools[i].Clicked -= _onClicked[i];
            }
        }

        /// <summary>Le rang de l'outil qu'on vise depuis le centre vers ce point du panneau ; -1 près du centre.</summary>
        public int ToolAt(Vector2 pointer)
        {
            _toolCenters.Clear();
            foreach (FeltButton tool in _tools)
                _toolCenters.Add(_centerPoint + tool.layout.center);
            return WheelGeometry.Aimed(_centerPoint, _toolCenters, pointer);
        }

        /// <summary>Marque l'outil visé (-1 : aucun) et le dit s'il change.</summary>
        public void Aim(int tool)
        {
            bool changed = false;
            for (int i = 0; i < _tools.Length; i++)
            {
                changed |= _tools[i].ClassListContains(AimedClass) != (i == tool);
                _tools[i].EnableInClassList(AimedClass, i == tool);
            }
            if (changed)
                AimChanged?.Invoke(tool);
        }

        private void AimByHover(int tool)
        {
            if (IsOpen && !IsHeld)
                Aim(tool);
        }

        // La boîte de l'anneau autour de son centre : son plateau et ses outils, qui l'agrandissent quand on les vise. Une
        // mesure qui manque encore (avant la première mise en page) ne ramène rien.
        private Rect Extent()
        {
            Rect extent = _plate.layout;
            foreach (FeltButton tool in _tools)
                extent = Rect.MinMaxRect(Mathf.Min(extent.xMin, tool.layout.xMin), Mathf.Min(extent.yMin, tool.layout.yMin),
                    Mathf.Max(extent.xMax, tool.layout.xMax), Mathf.Max(extent.yMax, tool.layout.yMax));
            return float.IsNaN(extent.width) || float.IsNaN(extent.height) ? Rect.zero : extent;
        }

        private void SetPickable(bool pickable)
        {
            PickingMode mode = pickable ? PickingMode.Position : PickingMode.Ignore;
            _wheel.pickingMode = mode;
            foreach (FeltButton tool in _tools)
                tool.pickingMode = mode;
        }
    }
}
