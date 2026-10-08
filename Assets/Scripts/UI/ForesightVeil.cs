using System;
using Diciela.View;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Le voile de rêverie de la prévision (GDD, section 16 ; décisions 228 et 230) : une image sur tout l'écran, sous
    /// le reste du HUD, d'une lavande pâle, presque transparente au milieu et dense vers les bords (Hud.uss, l'image
    /// générée Reverie.png) ; il paraît en fondu tant que la prévision dure, pour qu'on sache toujours qu'on regarde la
    /// ville à venir. Il ne capte pas le pointeur. Branché à l'ouverture de la ville, caché à sa sortie.
    /// </summary>
    public sealed class ForesightVeil
    {
        private const string VeilName = "reverie";
        private const string ShownClass = "reverie-visible";

        private readonly VisualElement _veil;
        private Foresight _foresight;

        /// <summary>Le voile de ce document ; il doit le porter (Hud.uxml).</summary>
        public ForesightVeil(UIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            _veil = document.rootVisualElement.Q(VeilName);
            if (_veil == null)
                throw new InvalidOperationException("Le voile de la prévision ne trouve pas son élément (UXML).");
            _veil.pickingMode = PickingMode.Ignore;
        }

        /// <summary>Suit cette prévision : le voile paraît tant qu'elle dure.</summary>
        public void Show(Foresight foresight)
        {
            Hide();
            _foresight = foresight ?? throw new ArgumentNullException(nameof(foresight));
            _foresight.Changed += OnChanged;
            OnChanged();
        }

        /// <summary>Cesse de suivre la prévision ; le voile s'efface. Accepte de n'avoir rien à cacher.</summary>
        public void Hide()
        {
            if (_foresight == null)
                return;
            _foresight.Changed -= OnChanged;
            _foresight = null;
            _veil.RemoveFromClassList(ShownClass);
        }

        private void OnChanged()
        {
            _veil.EnableInClassList(ShownClass, _foresight.IsOn);
        }
    }
}
