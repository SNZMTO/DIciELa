using System;
using Diciela.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Diciela.UI
{
    /// <summary>
    /// Le curseur des outils (GDD, section 16 ; décision 221), dessiné par le jeu : la silhouette de l'outil en main, à
    /// la couleur de son outil, sa pointe sur le pointeur ; ses pastilles en bas à droite (ce qu'il pose, sa forme, sa
    /// symétrie) ; une croix rouge quand le geste serait refusé. Il ne se montre qu'un outil en main, le pointeur sur
    /// la ville : sur l'interface (la roue ouverte comprise), hors de l'écran, ou tenu par un geste qui le cache
    /// (tourner à la souris), c'est la flèche du système. Il suit le pointeur que les gestes lui donnent, à chaque
    /// image, par translate, qui ne refait pas la mise en page ; il cache la flèche tant qu'il se montre, et la rend
    /// dès qu'il se cache. Il fait « pop » quand l'outil change, et laisse une éclaboussure à la couleur de l'outil
    /// quand il agit (décision 225). Le pointeur le traverse. C'est la vue (le bloc « curseur » de Hud.uxml) ; son
    /// présentateur décide de son allure.
    /// </summary>
    public sealed class ToolCursor : IToolCursorView, IToolCursor
    {
        private const string IconClass = "curseur-icone";
        private const string TintClass = "teinte";
        private const string BadgeIconClass = "curseur-pastille-icone";
        private const string PopClass = "curseur-pop";
        private const string SplashStartClass = "eclaboussure-depart";

        private readonly VisualElement _cursor;
        private readonly VisualElement _icon;
        private readonly VisualElement _ink;
        private readonly VisualElement _form;
        private readonly VisualElement _symmetry;
        private readonly VisualElement _refusal;
        private readonly VisualElement _splash;
        private IInterfaceArea _hud;
        private ToolCursorPresenter _presenter;
        private bool _hasLook;
        private Vector2 _tip;
        private Vector2 _at = new Vector2(float.NaN, float.NaN);
        private bool _shown;
        private Vector2 _pointer;
        private string _silhouette = "";
        private string _pastel = "";
        private string _splashPastel = "";

        public ToolCursor(UIDocument document)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            _cursor = root?.Q("curseur");
            _icon = root?.Q("curseur-icone");
            _ink = root?.Q("curseur-sol");
            _form = root?.Q("curseur-forme");
            _symmetry = root?.Q("curseur-symetrie");
            _refusal = root?.Q("curseur-refus");
            _splash = root?.Q("curseur-eclaboussure");
            if (_cursor == null || _icon == null || _ink == null || _form == null || _symmetry == null ||
                _refusal == null || _splash == null || _ink.childCount == 0 || _form.childCount == 0 || _symmetry.childCount == 0)
                throw new InvalidOperationException("Le curseur des outils ne trouve pas ses éléments (UXML).");
        }

        /// <summary>Le montre quand un outil de ce monde est en main, le pointeur hors de cette interface.</summary>
        public void Show(PlacementTool placement, ClearingTool clearing, MoveTool move, PaintTool paths, PaintTool plaza,
            IInterfaceArea hud)
        {
            Hide();
            _hud = hud;
            _presenter = new ToolCursorPresenter(this, placement, clearing, move, paths, plaza);
        }

        /// <summary>Le cache, rend la flèche et oublie le monde.</summary>
        public void Hide()
        {
            _presenter?.Dispose();
            _presenter = null;
            _hud = null;
            ShowArrow();
        }

        public void ShowArrow()
        {
            _hasLook = false;
            _silhouette = "";
            _pastel = "";
            SetShown(false);
        }

        public void ShowLook(ToolCursorLook look)
        {
            _hasLook = true;
            _tip = look.Tip;
            _at = new Vector2(float.NaN, float.NaN);
            WheelPicture.Paint(_icon, IconClass, look.Silhouette, null);
            _icon.AddToClassList(TintClass);
            _icon.AddToClassList(look.Pastel);
            Badge(_ink, look.InkIcon, look.InkPicture);
            Badge(_form, look.FormIcon, null);
            Badge(_symmetry, look.SymmetryIcon, null);
            if (look.Silhouette == _silhouette && look.Pastel == _pastel)
                return;
            _silhouette = look.Silhouette;
            _pastel = look.Pastel;
            // Le pop grossit le curseur autour de sa pointe, qui reste sur le pointeur.
            _cursor.style.transformOrigin = new TransformOrigin(_tip.x, _tip.y);
            HudMotion.Enter(_cursor, PopClass);
        }

        public void Splash()
        {
            if (!_shown)
                return;
            if (_splashPastel != _pastel)
            {
                if (_splashPastel.Length > 0)
                    _splash.RemoveFromClassList(_splashPastel);
                if (_pastel.Length > 0)
                    _splash.AddToClassList(_pastel);
                _splashPastel = _pastel;
            }
            _splash.style.translate = new Translate(_pointer.x, _pointer.y);
            HudMotion.Enter(_splash, SplashStartClass);
        }

        public void ShowRefused(bool refused)
        {
            _refusal.EnableInClassList(StyleClasses.Hidden, !refused);
        }

        /// <summary>Le pointeur est là, sur l'écran : le curseur s'y pose, s'il a à se montrer.</summary>
        public void FollowTo(Vector2 screenPoint)
        {
            bool shown = _hasLook && _hud != null && Cursor.lockState == CursorLockMode.None &&
                         screenPoint.x >= 0f && screenPoint.y >= 0f && screenPoint.x < Screen.width &&
                         screenPoint.y < Screen.height && !_hud.Contains(screenPoint);
            if (shown)
            {
                _pointer = HudArea.ToPanel(_cursor.panel, screenPoint);
                Vector2 at = _pointer - _tip;
                if (at != _at)
                {
                    _at = at;
                    _cursor.style.translate = new Translate(at.x, at.y);
                }
            }
            SetShown(shown);
        }

        // Se montre ou se cache ; la flèche fait l'inverse, sauf quand un geste tient le curseur (CursorGrip).
        private void SetShown(bool shown)
        {
            if (shown == _shown)
                return;
            _shown = shown;
            _cursor.EnableInClassList(StyleClasses.Hidden, !shown);
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.visible = !shown;
        }

        private static void Badge(VisualElement badge, string icon, Texture2D picture)
        {
            bool shown = icon.Length > 0 || picture != null;
            badge.EnableInClassList(StyleClasses.Hidden, !shown);
            if (shown)
                WheelPicture.Paint(badge[0], BadgeIconClass, icon, picture);
        }
    }
}
