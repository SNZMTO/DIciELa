using System;
using Diciela.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Le cadre qu'on tire à la souris avec l'outil « Retirer » (décision 161) : du lin translucide bordé de bois, un
    /// remplaçant simple, dessiné dans l'écran du HUD ; il ne capte pas le pointeur. Le pointeur compte depuis le bas de
    /// l'écran, UI Toolkit depuis le haut : on retourne l'axe vertical, puis on passe dans le repère du panneau.
    /// </summary>
    public sealed class SelectionFrame : ISelectionFrame, IDisposable
    {
        private readonly VisualElement _screen;
        private readonly VisualElement _frame;

        public SelectionFrame(UIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            _screen = document.rootVisualElement.Q("screen") ??
                      throw new InvalidOperationException("Le HUD n'a pas son écran (« screen »).");

            _frame = new VisualElement { name = "cadre-selection", pickingMode = PickingMode.Ignore };
            _frame.AddToClassList("cadre-selection");
            _frame.style.display = DisplayStyle.None;
            _screen.Add(_frame);
        }

        public void ShowFrame(Vector2 corner, Vector2 opposite)
        {
            IPanel panel = _screen.panel;
            if (panel == null)
                return;
            Vector2 a = _screen.WorldToLocal(HudArea.ToPanel(panel, corner));
            Vector2 b = _screen.WorldToLocal(HudArea.ToPanel(panel, opposite));
            _frame.style.left = Mathf.Min(a.x, b.x);
            _frame.style.top = Mathf.Min(a.y, b.y);
            _frame.style.width = Mathf.Abs(a.x - b.x);
            _frame.style.height = Mathf.Abs(a.y - b.y);
            _frame.style.display = DisplayStyle.Flex;
        }

        public void HideFrame()
        {
            _frame.style.display = DisplayStyle.None;
        }

        public void Dispose()
        {
            _frame.RemoveFromHierarchy();
        }
    }
}
