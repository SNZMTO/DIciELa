using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la roue (GDD, section 16 ; décisions 168 et 221). Elle s'ouvre là où l'on en est : un outil de
    /// sol en main, sur sa roue (PaintRing), dont la case Outils la retourne sur les outils ; sinon, sur les outils, chacun
    /// à sa place (WheelTool), la Place seulement une fois le campement posé. Choisir un outil l'allume, en éteignant les
    /// autres, ou l'éteint s'il l'était : Construire ouvre ou ferme le panneau Construire, les autres prennent ou lâchent
    /// leur outil. La bulle du milieu dit le nom et l'usage de ce qu'on vise ; la molette règle la case visée ; les
    /// chiffres prennent les cases de l'outil de sol en main.
    /// </summary>
    public sealed class ToolWheelPresenter
    {
        private readonly IToolWheelView _view;
        private readonly TextTable _texts;
        private readonly IToolKeys _keys;
        private readonly IBuildMenu _build;
        private readonly PaintTool _paths;
        private readonly PaintTool _plaza;
        private readonly Switch[] _switches;
        private readonly PaintRing _paintRing;
        private readonly List<ToolChoice> _toolsShown = new List<ToolChoice>();
        private List<WheelSlot> _slots = new List<WheelSlot>();
        private PaintTool _ringTool;
        private int _aimedSlot = -1;
        private int _aimedOption = -1;

        /// <summary>Ce qu'un outil allume et éteint, et s'il est allumé.</summary>
        private readonly struct Switch
        {
            public readonly Func<bool> IsOn;
            public readonly Action On;
            public readonly Action Off;

            public Switch(Func<bool> isOn, Action on, Action off)
            {
                IsOn = isOn;
                On = on;
                Off = off;
            }
        }

        public ToolWheelPresenter(IToolWheelView view, TextTable texts, IToolKeys keys, ClearingTool clearing,
            IBuildMenu build, MoveTool move, PaintTool paths, PaintTool plaza)
        {
            if (view == null || ReferenceEquals(texts, null) || keys == null || ReferenceEquals(clearing, null) ||
                build == null || ReferenceEquals(move, null) || ReferenceEquals(paths, null) || ReferenceEquals(plaza, null))
                throw new ArgumentException("La roue a besoin de sa vue, des textes, des touches, du panneau Construire " +
                                            "et des outils « Retirer », « Déplacer », « Chemins » et de la Place.");
            _view = view;
            _texts = texts;
            _keys = keys;
            _build = build;
            _paths = paths;
            _plaza = plaza;
            _paintRing = new PaintRing(texts, keys);
            // Au rang du choix de chaque outil (ToolChoice).
            _switches = new[]
            {
                new Switch(() => build.IsOpen, build.Open, build.Close),
                new Switch(() => clearing.IsActive, clearing.Take, clearing.PutDown),
                new Switch(() => move.IsActive, move.Take, move.PutDown),
                new Switch(() => paths.IsActive, paths.Take, paths.PutDown),
                new Switch(() => plaza.IsActive, plaza.Take, plaza.PutDown)
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

        /// <summary>La roue s'ouvre : sur les outils, ou là où l'on en est (la roue de l'outil de sol en main).</summary>
        public void Open(bool onTools)
        {
            _ringTool = onTools ? null : HeldPaint();
            ShowRing();
            Aim(-1, -1);
        }

        /// <summary>Prend cette case (option -1) ou cette option : vrai si la roue reste ouverte, retournée sur les outils.</summary>
        public bool Pick(int slot, int option)
        {
            if (slot < 0 || slot >= _slots.Count)
                return false;
            if (_ringTool == null)
            {
                Choose(_toolsShown[slot]);
                return false;
            }
            if (_paintRing.Pick(_ringTool, _slots[slot].Place, option))
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
            if (_ringTool == null || _aimedSlot < 0 || _aimedSlot >= _slots.Count)
                return;
            if (!_paintRing.Scroll(_ringTool, _slots[_aimedSlot].Place, _aimedOption, notches))
                return;
            ShowRing();
            Aim(_aimedSlot, _aimedOption);
        }

        /// <summary>
        /// La touche d'un outil, ou son choix dans la roue : l'allume en éteignant les autres, ou l'éteint s'il l'était.
        /// La Place n'y répond qu'une fois le campement posé.
        /// </summary>
        public void Choose(ToolChoice choice)
        {
            if (choice == ToolChoice.Plaza && !_build.OffersPlaza)
                return;
            Switch chosen = _switches[(int)choice];
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
        /// Le chiffre d'une case de l'outil de sol en main (0 : la première) : vrai s'il demande la roue des outils.
        /// </summary>
        public bool TakeSlot(int index)
        {
            PaintTool tool = HeldPaint();
            return tool != null && !_paintRing.Key(tool, index);
        }

        private void ShowRing()
        {
            _slots = _ringTool != null
                ? _paintRing.Slots(_ringTool, WheelTool.Of(_ringTool == _plaza ? ToolChoice.Plaza : ToolChoice.Paths).Pastel)
                : ToolSlots();
            _view.ShowRing(_slots);
        }

        private List<WheelSlot> ToolSlots()
        {
            _toolsShown.Clear();
            var slots = new List<WheelSlot>();
            foreach (WheelTool tool in WheelTool.All)
            {
                if (tool.Choice == ToolChoice.Plaza && !_build.OffersPlaza)
                    continue;
                _toolsShown.Add(tool.Choice);
                slots.Add(new WheelSlot(tool.Place, tool.Icon, null, tool.Pastel, _keys.KeyOf(tool.Choice),
                    _texts.Get(tool.NameKey), _texts.Get(tool.DetailKey), _switches[(int)tool.Choice].IsOn()));
            }
            return slots;
        }

        private PaintTool HeldPaint()
        {
            return _plaza.IsActive ? _plaza : _paths.IsActive ? _paths : null;
        }
    }
}
