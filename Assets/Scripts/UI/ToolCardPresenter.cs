using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la carte de l'outil en main (GDD, section 16 ; décisions 221 et 226) : cachée les mains
    /// vides ; sinon l'icône, la couleur et le nom de l'outil, ceux de sa case dans la roue (WheelTool), le mode d'un
    /// outil de sol ou la construction en main, sa photo et son nom, ses nombres et ses réglages en puces
    /// (ToolCardContent), et ce que disent ses raisons (ToolCardReasons). Il écoute les outils, la Place et les arbres
    /// marqués : la carte ne photographie jamais.
    /// </summary>
    public sealed class ToolCardPresenter : IDisposable
    {
        private readonly IToolCardView _view;
        private readonly TextTable _texts;
        private readonly PlacementTool _placement;
        private readonly ClearingTool _clearing;
        private readonly MoveTool _move;
        private readonly PaintTool _paths;
        private readonly PaintTool _plaza;
        private readonly PlazaZone _zone;
        private readonly Nature _nature;
        private readonly ToolCardReasons _reasons;
        private readonly List<ToolCardChip> _chips = new List<ToolCardChip>();

        public ToolCardPresenter(IToolCardView view, TextTable texts, PlacementTool placement, ClearingTool clearing,
            MoveTool move, PaintTool paths, PaintTool plaza, World world)
        {
            if (view == null || ReferenceEquals(texts, null) || ReferenceEquals(placement, null) ||
                ReferenceEquals(clearing, null) || ReferenceEquals(move, null) || ReferenceEquals(paths, null) ||
                ReferenceEquals(plaza, null) || ReferenceEquals(world, null))
                throw new ArgumentException("La carte de l'outil en main a besoin de sa vue, des textes, des outils " +
                                            "de pose, « Retirer », Déplacer, Chemins et de la Place, et du monde.");
            _view = view;
            _texts = texts;
            _placement = placement;
            _clearing = clearing;
            _move = move;
            _paths = paths;
            _plaza = plaza;
            _zone = world.Plaza;
            _nature = world.Nature;
            _reasons = new ToolCardReasons(view, texts, HeldPaint, placement, move, world);
            _placement.HeldChanged += OnHeldChanged;
            _placement.ResultChanged += _reasons.Show;
            _clearing.HeldChanged += OnHeldChanged;
            _move.HeldChanged += OnHeldChanged;
            _move.ResultChanged += _reasons.Show;
            _paths.HeldChanged += OnHeldChanged;
            _paths.ResultChanged += _reasons.Show;
            _plaza.HeldChanged += OnHeldChanged;
            _plaza.ResultChanged += _reasons.Show;
            _zone.PlazaChanged += ShowHeld;
            _zone.MarkPlaced += OnMarksChanged;
            _zone.MarkRemoved += OnMarksChanged;
            _nature.ItemMarked += OnNatureChanged;
            _nature.ItemUnmarked += OnNatureChanged;
            _nature.ItemRemoved += OnNatureChanged;
            OnHeldChanged();
        }

        public void Dispose()
        {
            _placement.HeldChanged -= OnHeldChanged;
            _placement.ResultChanged -= _reasons.Show;
            _clearing.HeldChanged -= OnHeldChanged;
            _move.HeldChanged -= OnHeldChanged;
            _move.ResultChanged -= _reasons.Show;
            _paths.HeldChanged -= OnHeldChanged;
            _paths.ResultChanged -= _reasons.Show;
            _plaza.HeldChanged -= OnHeldChanged;
            _plaza.ResultChanged -= _reasons.Show;
            _zone.PlazaChanged -= ShowHeld;
            _zone.MarkPlaced -= OnMarksChanged;
            _zone.MarkRemoved -= OnMarksChanged;
            _nature.ItemMarked -= OnNatureChanged;
            _nature.ItemUnmarked -= OnNatureChanged;
            _nature.ItemRemoved -= OnNatureChanged;
            _reasons.Dispose();
            _view.ShowNothing();
        }

        // L'outil de sol en main ; null si aucun ne l'est.
        private PaintTool HeldPaint()
        {
            return _plaza.IsActive ? _plaza : _paths.IsActive ? _paths : null;
        }

        // Un outil pris, lâché, ou qui change : la carte le dit en entier, raisons comprises.
        private void OnHeldChanged()
        {
            ShowHeld();
            _reasons.Show();
        }

        private void OnMarksChanged(PlazaMark mark)
        {
            ShowHeld();
        }

        private void OnNatureChanged(NatureItem item)
        {
            if (_clearing.IsActive)
                ShowHeld();
        }

        // Ce qui est en main, et ses puces ; rien en main, la carte se cache.
        private void ShowHeld()
        {
            PaintTool paint = HeldPaint();
            if (paint != null)
            {
                ShowTitle(paint == _plaza ? ToolChoice.Plaza : ToolChoice.Paths,
                    HudFace.Styled(PaintIconClasses.Mode(paint.Mode)),
                    _texts.Get(TextKeys.PaintMode(paint.Mode.ToString())));
                ToolCardContent.Paint(paint, _zone, _texts, _chips);
            }
            else if (_placement.IsActive)
            {
                StructureDefinition structure = _placement.Current;
                ShowTitle(ToolChoice.Build, HudFace.Photo(structure.Picture), _texts.Get(structure.NameKey));
                ToolCardContent.Build(structure, _texts, _chips);
            }
            else if (_clearing.IsActive)
            {
                ShowTitle(ToolChoice.Clear, default, "");
                ToolCardContent.Clearing(_nature, _texts, _chips);
            }
            else if (_move.IsActive)
            {
                ShowTitle(ToolChoice.Move, default, "");
                ToolCardContent.Move(_move.Moving, _texts, _chips);
            }
            else
            {
                _view.ShowNothing();
                return;
            }
            _view.ShowChips(_chips);
        }

        private void ShowTitle(ToolChoice choice, HudFace modeIcon, string mode)
        {
            WheelTool tool = WheelTool.Of(choice);
            _view.ShowTool(HudFace.Styled(tool.Icon), tool.Pastel, _texts.Get(tool.NameKey), modeIcon, mode);
        }
    }
}
