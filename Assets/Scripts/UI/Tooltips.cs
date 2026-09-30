using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les infobulles du HUD (GDD, section 16 ; décision 153) : une seule bulle de lin au liseré de paille, qui se montre
    /// sous ou au-dessus de l'élément survolé, centrée sur lui, et dit ce que son présentateur lui fait dire au moment du
    /// survol (un titre, et une suite en plus doux). Elle ne capte pas le pointeur.
    /// </summary>
    public sealed class Tooltips
    {
        // L'écart entre l'élément survolé et la bulle, en pixels du panneau.
        private const float Gap = 10f;
        private const string HiddenClass = "bloc-cache";
        private const string BubbleName = "infobulle";
        private const string TitleName = "infobulle-titre";
        private const string DetailName = "infobulle-suite";

        private readonly VisualElement _bubble;
        private readonly Label _title;
        private readonly Label _detail;

        /// <summary>Les infobulles de ce document ; il doit porter la bulle (Hud.uxml).</summary>
        public Tooltips(UIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            VisualElement root = document.rootVisualElement;
            _bubble = root.Q(BubbleName);
            _title = root.Q<Label>(TitleName);
            _detail = root.Q<Label>(DetailName);
            if (_bubble == null || _title == null || _detail == null)
                throw new InvalidOperationException("Les infobulles ne trouvent pas leur bulle (UXML).");
            _bubble.pickingMode = PickingMode.Ignore;
            Hide();
        }

        /// <summary>
        /// Donne une infobulle à cet élément : au survol, la bulle dit ce que rend <paramref name="text"/> (un titre, et
        /// une suite, vide si rien), sous lui, ou au-dessus s'il est en bas de l'écran.
        /// </summary>
        public void Attach(VisualElement target, Func<(string Title, string Detail)> text, bool above = false)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            target.RegisterCallback<PointerEnterEvent>(_ => Show(target, text(), above));
            target.RegisterCallback<PointerLeaveEvent>(_ => Hide());
        }

        public void Hide()
        {
            _bubble.AddToClassList(HiddenClass);
        }

        private void Show(VisualElement target, (string Title, string Detail) text, bool above)
        {
            _title.text = text.Title;
            _detail.text = text.Detail;
            _detail.EnableInClassList(HiddenClass, string.IsNullOrEmpty(text.Detail));
            Rect bound = target.worldBound;
            VisualElement parent = _bubble.parent;
            Vector2 anchor = parent.WorldToLocal(new Vector2(bound.center.x, above ? bound.yMin - Gap : bound.yMax + Gap));
            _bubble.style.left = anchor.x;
            _bubble.style.top = anchor.y;
            _bubble.style.translate = new Translate(Length.Percent(-50f), Length.Percent(above ? -100f : 0f));
            _bubble.RemoveFromClassList(HiddenClass);
        }
    }
}
