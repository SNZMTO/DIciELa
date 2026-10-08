using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la roue (GDD, section 16 ; décisions 221 et 226). Elle s'ouvre là où l'on en est : un outil
    /// en main qui a des réglages, sur sa roue (IToolRing : celle d'un outil de sol, PaintRing, ou celle de Construire,
    /// BuildRing), dont la case Outils la retourne sur les outils ; sinon, sur les outils, chacun à sa place
    /// (WheelTool), la Place seulement une fois le campement posé (BuildOffer). Choisir un outil le prend en main avec
    /// ses derniers réglages, en lâchant les autres, ou le lâche s'il l'était : Construire reprend la dernière
    /// construction prise. La bulle du milieu dit le nom et l'usage de ce qu'on vise ; la molette règle la case visée ;
    /// les chiffres prennent les cases de la roue de l'outil en main.
    /// </summary>
    public sealed class ToolWheelPresenter
    {
        private readonly IToolWheelView _view;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly BuildOffer _offer;
        private readonly PlacementTool _placement;
        private readonly PaintTool _paths;
        private readonly PaintTool _plaza;
        private readonly ToolSwitch[] _switches;
        private readonly BuildRing _buildRing;
        private readonly PaintRing _pathsRing;
        private readonly PaintRing _plazaRing;
        private readonly List<ToolChoice> _toolsShown = new List<ToolChoice>();
        private List<WheelSlot> _slots = new List<WheelSlot>();
        private IToolRing _ring;
        private int _aimedSlot = -1;
        private int _aimedOption = -1;

        public ToolWheelPresenter(IToolWheelView view, TextTable texts, IToolKeys keys, Catalog catalog, World world,
            PlacementTool placement, ClearingTool clearing, MoveTool move, PaintTool paths, PaintTool plaza)
        {
            if (view == null || ReferenceEquals(texts, null) || keys == null || catalog == null ||
                ReferenceEquals(world, null) || ReferenceEquals(placement, null) || ReferenceEquals(clearing, null) ||
                ReferenceEquals(move, null) || ReferenceEquals(paths, null) || ReferenceEquals(plaza, null))
                throw new ArgumentException("La roue a besoin de sa vue, des textes, des touches, du catalogue, du " +
                                            "monde et des outils de pose, « Retirer », Déplacer, Chemins et de la " +
                                            "Place.");
            _view = view;
            _texts = texts;
            _keys = keys;
            _placement = placement;
            _paths = paths;
            _plaza = plaza;
            _offer = new BuildOffer(catalog, world);
            _buildRing = new BuildRing(catalog, _offer, placement, texts, keys, WheelTool.Of(ToolChoice.Build).Pastel);
            _pathsRing = new PaintRing(texts, keys, paths, WheelTool.Of(ToolChoice.Paths).Pastel);
            _plazaRing = new PaintRing(texts, keys, plaza, WheelTool.Of(ToolChoice.Plaza).Pastel);
            // Au rang du choix de chaque outil (ToolChoice).
            _switches = new[]
            {
                new ToolSwitch(() => placement.IsActive, _buildRing.Take, placement.PutDown),
                new ToolSwitch(() => clearing.IsActive, clearing.Take, clearing.PutDown),
                new ToolSwitch(() => move.IsActive, move.Take, move.PutDown),
                new ToolSwitch(() => paths.IsActive, paths.Take, paths.PutDown),
                new ToolSwitch(() => plaza.IsActive, plaza.Take, plaza.PutDown)
            };
            _view.ShowWheelKey(keys.WheelKey);
            _view.ShowLabel("", "");
        }

        /// <summary>L'infobulle de l'icône des outils : son nom, et la touche qui ouvre la roue sous le pointeur.</summary>
        public (string Title, string Detail) IconTooltip()
        {
            return (_texts.Get(TextKeys.ToolsTooltip),
                _texts.Get(TextKeys.ToolsTooltipDetail).Replace(TextKeys.KeyParameter, _keys.WheelKey));
        }

        /// <summary>La roue s'ouvre : sur les outils, ou là où l'on en est (la roue de l'outil en main).</summary>
        public void Open(bool onTools)
        {
            _ring = onTools ? null : HeldRing();
            ShowRing();
            Aim(-1, -1);
        }

        /// <summary>Prend cette case (option -1) ou cette option : vrai si la roue reste ouverte, retournée sur les outils.</summary>
        public bool Pick(int slot, int option)
        {
            if (slot < 0 || slot >= _slots.Count)
                return false;
            if (_ring == null)
            {
                Choose(_toolsShown[slot]);
                return false;
            }
            if (_ring.Pick(_slots[slot].Place, option))
                return false;
            Open(true);
            return true;
        }

        /// <summary>Le joueur vise cette case et cette option (-1 : aucune) : la bulle les nomme.</summary>
        public void Aim(int slot, int option)
        {
            _aimedSlot = slot;
            _aimedOption = option;
            if (slot < 0 || slot >= _slots.Count)
                _view.ShowLabel("", "");
            else if (option >= 0 && option < _slots[slot].Options.Count)
                _view.ShowLabel(_slots[slot].Options[option].Name, _slots[slot].Options[option].Detail);
            else
                _view.ShowLabel(_slots[slot].Name, _slots[slot].Detail);
        }

        /// <summary>La molette sur la roue ouverte : elle règle la case visée (la taille, les branches de l'étoile).</summary>
        public void Scroll(int notches)
        {
            if (_ring == null || _aimedSlot < 0 || _aimedSlot >= _slots.Count)
                return;
            if (!_ring.Scroll(_slots[_aimedSlot].Place, _aimedOption, notches))
                return;
            ShowRing();
            Aim(_aimedSlot, _aimedOption);
        }

        /// <summary>
        /// La touche d'un outil, ou son choix dans la roue : le prend en main en lâchant les autres, ou le lâche s'il
        /// l'était. La Place n'y répond qu'une fois le campement posé.
        /// </summary>
        public void Choose(ToolChoice choice)
        {
            if (choice == ToolChoice.Plaza && !_offer.OffersPlaza)
                return;
            ToolSwitch chosen = _switches[(int)choice];
            if (chosen.IsOn())
            {
                chosen.Off();
                return;
            }
            for (int i = 0; i < _switches.Length; i++)
            {
                if (i != (int)choice && _switches[i].IsOn())
                    _switches[i].Off();
            }
            chosen.On();
        }

        /// <summary>
        /// Le chiffre d'une case de la roue de l'outil en main (0 : la première) : vrai s'il demande la roue des
        /// outils.
        /// </summary>
        public bool TakeSlot(int index)
        {
            IToolRing ring = HeldRing();
            return ring != null && !ring.Key(index);
        }

        private void ShowRing()
        {
            _slots = _ring != null ? _ring.Slots() : ToolSlots();
            _view.ShowRing(_slots);
        }

        private List<WheelSlot> ToolSlots()
        {
            _toolsShown.Clear();
            var slots = new List<WheelSlot>();
            foreach (WheelTool tool in WheelTool.All)
            {
                if (tool.Choice == ToolChoice.Plaza && !_offer.OffersPlaza)
                    continue;
                _toolsShown.Add(tool.Choice);
                slots.Add(new WheelSlot(tool.Place, HudFace.Styled(tool.Icon), tool.Pastel, _keys.KeyOf(tool.Choice),
                    _texts.Get(tool.NameKey), _texts.Get(tool.DetailKey), _switches[(int)tool.Choice].IsOn()));
            }
            return slots;
        }

        // La roue des réglages de l'outil en main ; null si aucun n'en a (Retirer, Déplacer, les mains vides).
        private IToolRing HeldRing()
        {
            if (_plaza.IsActive)
                return _plazaRing;
            if (_paths.IsActive)
                return _pathsRing;
            return _placement.IsActive ? _buildRing : null;
        }
    }
}
