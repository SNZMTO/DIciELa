using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// L'anneau de la roue des outils (décision 168) : les outils autour d'un centre, à la place que leur donne la feuille
    /// de style, et le fond plein écran qui prend le pointeur tant que l'anneau est ouvert. Il s'ouvre en un point du
    /// panneau, ramené pour tenir dans l'écran ; touche tenue, on y vise par la direction du pointeur, au clic par le
    /// survol ; un appui sur le fond le congédie. Fermé, il reste en page, invisible et sans prise : il se mesure avant de
    /// s'ouvrir.
    /// </summary>
    internal sealed class WheelRing
    {
        private const string ClosedClass = "roue-fermee";
        private const string AimedClass = "vise";

        private readonly VisualElement _wheel;
        private readonly VisualElement _center;
        private readonly VisualElement _bubble;
        private readonly FeltButton[] _tools;
        private readonly List<Vector2> _toolCenters = new List<Vector2>();
        private Vector2 _centerPoint;

        public WheelRing(VisualElement wheel, VisualElement center, VisualElement bubble, FeltButton[] tools)
        {
            _wheel = wheel;
            _center = center;
            _bubble = bubble;
            _tools = tools;
            _wheel.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.target == _wheel)
                    Dismissed?.Invoke();
            });
            for (int i = 0; i < _tools.Length; i++)
            {
                int tool = i;
                _tools[i].RegisterCallback<PointerEnterEvent>(_ => AimByHover(tool));
                _tools[i].RegisterCallback<PointerLeaveEvent>(_ => AimByHover(-1));
            }
        }

        /// <summary>Levé quand l'outil visé change : son rang, ou -1.</summary>
        public event Action<int> AimChanged;

        /// <summary>Levé quand on appuie sur le fond, à côté des outils.</summary>
        public event Action Dismissed;

        public bool IsOpen { get; private set; }

        /// <summary>Vrai si l'anneau s'est ouvert par la touche tenue : on y vise par la direction du pointeur.</summary>
        public bool IsHeld { get; private set; }

        /// <summary>Ouvre l'anneau autour de ce point du panneau, ramené pour qu'il tienne dans l'écran.</summary>
        public void Open(Vector2 at, bool held)
        {
            Rect room = _wheel.worldBound;
            _centerPoint = WheelGeometry.Clamped(at, Extent(), room);
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

        // La boîte de l'anneau autour de son centre : ses outils, et la bulle à sa plus grande largeur. Une mesure qui
        // manque encore (avant la première mise en page) ne ramène rien.
        private Rect Extent()
        {
            Rect extent = _tools[0].layout;
            foreach (FeltButton tool in _tools)
                extent = Rect.MinMaxRect(Mathf.Min(extent.xMin, tool.layout.xMin), Mathf.Min(extent.yMin, tool.layout.yMin),
                    Mathf.Max(extent.xMax, tool.layout.xMax), Mathf.Max(extent.yMax, tool.layout.yMax));
            float bubble = _bubble.resolvedStyle.maxWidth.value;
            if (!float.IsNaN(bubble))
                extent = Rect.MinMaxRect(Mathf.Min(extent.xMin, -bubble / 2f), extent.yMin,
                    Mathf.Max(extent.xMax, bubble / 2f), extent.yMax);
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
