using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Society;
using Diciela.View;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de l'écran de création (GDD, section 6 ; ARCHITECTURE, section 11) : il le remplit depuis le
    /// catalogue et la table des textes, une ligne pour le corps, une pour la couleur, puis une par emplacement, chacune
    /// avec « aucun » en tête de ses pièces ; les flèches font le tour de chaque ligne. Il part de l'apparence de départ
    /// du catalogue, et changer de corps lui rend sa couleur d'origine. Il habille l'aperçu à chaque choix ; à la
    /// validation, il envoie l'ordre « créer la Chimère » avec l'apparence choisie, une seule fois, puis le dit.
    /// </summary>
    public sealed class CreationPresenter : IDisposable
    {
        private const int BodyRow = 0;
        private const int PaletteRow = 1;
        private const int FirstSlotRow = 2;
        private const int None = -1;

        private readonly ICreationView _view;
        private readonly Catalog _catalog;
        private readonly TextTable _texts;
        private readonly AppearancePreview _preview;
        private readonly World _world;
        private readonly Action _created;
        private readonly List<List<AttachmentDefinition>> _offered = new List<List<AttachmentDefinition>>();
        private readonly int[] _chosen;
        private readonly List<AttachmentDefinition> _worn = new List<AttachmentDefinition>();
        private int _body;
        private int _palette;
        private bool _confirmed;

        public CreationPresenter(ICreationView view, Catalog catalog, TextTable texts, AppearancePreview preview,
            World world, Action created)
        {
            if (view == null || catalog == null || catalog.ChimeraKind == null || ReferenceEquals(texts, null) ||
                preview == null || ReferenceEquals(world, null))
                throw new ArgumentException("L'écran de création a besoin de sa vue, du catalogue et de la sorte de la " +
                                            "Chimère, des textes, de l'aperçu et du monde.");

            _view = view;
            _catalog = catalog;
            _texts = texts;
            _preview = preview;
            _world = world;
            _created = created;
            Appearance starting = Appearance.StartingIn(catalog);
            _palette = Math.Max(0, IndexOf(catalog.Palettes, starting.Palette));
            foreach (AttachmentSlotDefinition slot in catalog.AttachmentSlots)
            {
                var pieces = new List<AttachmentDefinition>();
                foreach (AttachmentDefinition piece in catalog.Attachments)
                {
                    if (piece != null && ReferenceEquals(piece.Slot, slot))
                        pieces.Add(piece);
                }
                _offered.Add(pieces);
            }
            _chosen = new int[_offered.Count];
            for (int i = 0; i < _chosen.Length; i++)
                _chosen[i] = None;

            var labels = new List<string> { texts.Get(TextKeys.CreationBody), texts.Get(TextKeys.CreationPalette) };
            foreach (AttachmentSlotDefinition slot in catalog.AttachmentSlots)
                labels.Add(texts.Get(slot.NameKey));
            _view.Stepped += OnStepped;
            _view.Confirmed += OnConfirmed;
            _view.ShowRows(texts.Get(TextKeys.CreationTitle), texts.Get(TextKeys.CreationConfirm),
                texts.Get(TextKeys.CreationPrevious), texts.Get(TextKeys.CreationNext), labels);
            Show();
        }

        /// <summary>L'apparence choisie en ce moment.</summary>
        public Appearance Current
        {
            get
            {
                _worn.Clear();
                for (int i = 0; i < _chosen.Length; i++)
                {
                    if (_chosen[i] != None)
                        _worn.Add(_offered[i][_chosen[i]]);
                }
                return new Appearance(_catalog.Bodies[_body], _catalog.Palettes[_palette], _worn);
            }
        }

        public void Dispose()
        {
            _view.Stepped -= OnStepped;
            _view.Confirmed -= OnConfirmed;
        }

        private void OnStepped(int row, int direction)
        {
            if (_confirmed)
                return;
            if (row == BodyRow)
            {
                _body = Wrap(_body + direction, _catalog.Bodies.Count);
                int own = IndexOf(_catalog.Palettes, _catalog.Bodies[_body].Palette);
                if (own >= 0)
                    _palette = own;
            }
            else if (row == PaletteRow)
                _palette = Wrap(_palette + direction, _catalog.Palettes.Count);
            else if (row >= FirstSlotRow && row - FirstSlotRow < _chosen.Length)
            {
                int slot = row - FirstSlotRow;
                // « Aucun » est un choix de la ligne, avant ses pièces.
                _chosen[slot] = Wrap(_chosen[slot] + 1 + direction, _offered[slot].Count + 1) - 1;
            }
            else
                return;
            Show();
        }

        private void OnConfirmed()
        {
            if (_confirmed)
                return;
            _confirmed = true;
            _world.Submit(new CreateChimeraCommand(_catalog.ChimeraKind, Current));
            _created?.Invoke();
        }

        private void Show()
        {
            _view.ShowChoice(BodyRow, _texts.Get(_catalog.Bodies[_body].NameKey));
            _view.ShowChoice(PaletteRow, _texts.Get(_catalog.Palettes[_palette].NameKey));
            for (int i = 0; i < _chosen.Length; i++)
            {
                _view.ShowChoice(FirstSlotRow + i, _chosen[i] == None
                    ? _texts.Get(TextKeys.CreationNone)
                    : _texts.Get(_offered[i][_chosen[i]].NameKey));
            }
            _preview.Show(Current);
        }

        private static int Wrap(int index, int count)
        {
            return (index % count + count) % count;
        }

        private static int IndexOf<T>(IReadOnlyList<T> list, T item) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], item))
                    return i;
            }
            return -1;
        }
    }
}
