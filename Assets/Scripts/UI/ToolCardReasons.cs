using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que la carte de l'outil en main dit sous ses puces (décisions 221 et 226). Un outil de sol : qu'un clic
    /// retirerait la place de charrette visée, pourquoi le geste est refusé, et, jusqu'à ce que la visée change,
    /// pourquoi le monde a refusé en le jouant un ordre que l'aperçu laissait passer ; « rien ne change » ne dit rien.
    /// Une construction en main, ou Déplacer : pourquoi la construction ne se pose pas là où on la mène. Elle écoute la
    /// Place et les sols ; son présentateur lui dit quand l'outil en main ou sa réponse change.
    /// </summary>
    internal sealed class ToolCardReasons : IDisposable
    {
        private readonly IToolCardView _view;
        private readonly TextTable _texts;
        private readonly Func<PaintTool> _paint;
        private readonly PlacementTool _placement;
        private readonly MoveTool _move;
        private readonly PlazaZone _plaza;
        private readonly Floors _floors;

        public ToolCardReasons(IToolCardView view, TextTable texts, Func<PaintTool> paint, PlacementTool placement,
            MoveTool move, World world)
        {
            _view = view;
            _texts = texts;
            _paint = paint;
            _placement = placement;
            _move = move;
            _plaza = world.Plaza;
            _floors = world.Floors;
            _plaza.PlazaRefused += OnRefused;
            _floors.FloorRefused += OnFloorRefused;
        }

        public void Dispose()
        {
            _plaza.PlazaRefused -= OnRefused;
            _floors.FloorRefused -= OnFloorRefused;
            _view.ShowReason("", false);
        }

        /// <summary>L'outil en main, ou sa réponse, a changé : la carte dit ce qu'il y a à dire.</summary>
        public void Show()
        {
            PaintTool paint = _paint();
            if (paint != null && paint.HoveredMark != null)
                _view.ShowReason(_texts.Get(TextKeys.PlazaRemoveMark), false);
            else if (paint != null && !paint.Answer.IsOk && !paint.Answer.ChangesNothing)
                _view.ShowReason(_texts.Get(paint.Answer.ReasonKey), true);
            else if (paint == null && _placement.IsActive && _placement.Result != PlacementResult.Ok)
                _view.ShowReason(_texts.Get(TextKeys.PlacementRefusal(_placement.Result.ToString())), true);
            else if (paint == null && _move.IsActive && _move.Moving != null && _move.Result != PlacementResult.Ok)
                _view.ShowReason(_texts.Get(TextKeys.PlacementRefusal(_move.Result.ToString())), true);
            else
                _view.ShowReason("", false);
        }

        private void OnRefused(PlazaResult reason)
        {
            Say(PaintAnswer.Of(reason));
        }

        private void OnFloorRefused(FloorResult reason)
        {
            Say(PaintAnswer.Of(reason));
        }

        private void Say(PaintAnswer refusal)
        {
            if (_paint() != null && !refusal.ChangesNothing)
                _view.ShowReason(_texts.Get(refusal.ReasonKey), true);
        }
    }
}
