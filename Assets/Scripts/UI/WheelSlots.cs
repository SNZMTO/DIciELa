using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Les cases de l'anneau de la roue (décision 221) : un feutre par case, à sa place sur le cercle, son icône ou sa
    /// vignette, sa touche, son pastel, enfoncé si elle est prise, grossi si on la vise, et qui se dandine quand on se
    /// met à la viser ; à l'ouverture, elles paraissent l'une après l'autre (décision 225). On y vise au survol, on y
    /// clique. Il refait ses feutres quand les places changent, sinon les met à jour sur place.
    /// </summary>
    internal sealed class WheelSlots
    {
        private const string AimedClass = "vise";
        private const string EnterClass = "roue-entree";
        // L'écart entre deux cases qui paraissent : huit cases tiennent dans les 200 ms d'un mouvement.
        private const long StaggerMs = 10;
        private static readonly string[] SlotClasses = { "feutre", "pan-10", "roue-outil" };

        private readonly VisualElement _box;
        private readonly List<FeltButton> _felts = new List<FeltButton>();
        private readonly List<int> _places = new List<int>();
        private int _aimed = -1;

        public WheelSlots(VisualElement box)
        {
            _box = box;
        }

        /// <summary>Levé quand le pointeur entre sur une case : son rang.</summary>
        public event Action<int> Hovered;

        /// <summary>Levé quand on clique une case : son rang.</summary>
        public event Action<int> Clicked;

        /// <summary>Montre ces cases sur ce cercle ; vrai si leurs places ont changé (les feutres sont refaits).</summary>
        public bool Show(IReadOnlyList<WheelSlot> slots, float radius)
        {
            bool samePlaces = slots.Count == _places.Count;
            for (int i = 0; samePlaces && i < slots.Count; i++)
                samePlaces = slots[i].Place == _places[i];
            if (!samePlaces)
                Rebuild(slots, radius);
            for (int i = 0; i < slots.Count; i++)
                Paint(_felts[i], slots[i]);
            return !samePlaces;
        }

        /// <summary>Marque la case visée (-1 : aucune) ; une case qu'on se met à viser se dandine.</summary>
        public void Aim(int slot)
        {
            for (int i = 0; i < _felts.Count; i++)
                _felts[i].EnableInClassList(AimedClass, i == slot);
            if (slot != _aimed && slot >= 0 && slot < _felts.Count)
                HudMotion.Wobble(_felts[slot]);
            _aimed = slot;
        }

        /// <summary>Les cases paraissent l'une après l'autre.</summary>
        public void Enter()
        {
            for (int i = 0; i < _felts.Count; i++)
                HudMotion.Enter(_felts[i], EnterClass, i * StaggerMs);
        }

        /// <summary>Le visage de cette case : son icône ou sa vignette ; null hors des cases.</summary>
        public VisualElement FaceAt(int slot)
        {
            return slot >= 0 && slot < _felts.Count ? _felts[slot][0] : null;
        }

        public void SetPickable(bool pickable)
        {
            foreach (FeltButton felt in _felts)
                felt.pickingMode = pickable ? PickingMode.Position : PickingMode.Ignore;
        }

        private void Rebuild(IReadOnlyList<WheelSlot> slots, float radius)
        {
            _box.Clear();
            _felts.Clear();
            _places.Clear();
            _aimed = -1;
            for (int i = 0; i < slots.Count; i++)
            {
                int index = i;
                FeltButton felt = Styled.With(new FeltButton(), SlotClasses);
                felt.Add(new VisualElement { pickingMode = PickingMode.Ignore });
                Facet key = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, "creux", "pan-4", "touche");
                key.Add(Styled.Text("", "touche-texte"));
                felt.Add(key);
                Vector2 at = WheelGeometry.OnCircle(slots[i].Place * 45f, radius);
                felt.style.left = at.x;
                felt.style.top = at.y;
                felt.RegisterCallback<PointerEnterEvent>(_ => Hovered?.Invoke(index));
                felt.Clicked += () => Clicked?.Invoke(index);
                felt.userData = "";
                _box.Add(felt);
                _felts.Add(felt);
                _places.Add(slots[i].Place);
            }
        }

        // Le visage d'une case : son pastel, enfoncée si elle est prise, son icône ou sa vignette, sa touche.
        private static void Paint(FeltButton felt, WheelSlot slot)
        {
            var pastel = (string)felt.userData;
            if (pastel != slot.Pastel)
            {
                if (pastel.Length > 0)
                    felt.RemoveFromClassList(pastel);
                if (slot.Pastel.Length > 0)
                    felt.AddToClassList(slot.Pastel);
                felt.userData = slot.Pastel;
            }
            felt.EnableInClassList(StyleClasses.Open, slot.IsOn);
            WheelPicture.Paint(felt[0], "roue-icone", slot.Icon, slot.Picture);
            ((Label)felt[1][0]).text = slot.Key;
            felt[1].EnableInClassList(StyleClasses.Hidden, slot.Key.Length == 0);
        }
    }
}
