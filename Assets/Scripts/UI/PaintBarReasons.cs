using System;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Ce que la barre des outils de sol dit dans la bulle des raisons (décisions 206 et 210) : qu'un clic retirerait la
    /// place de charrette visée, pourquoi le geste est refusé, et, jusqu'à ce que la visée change, pourquoi le monde a
    /// refusé en le jouant un ordre que l'aperçu laissait passer ; « rien ne change » ne dit rien. Elle écoute la Place et
    /// les sols ; son présentateur lui dit quand la réponse de l'outil en main change.
    /// </summary>
    internal sealed class PaintBarReasons : IDisposable
    {
        private readonly IPaintBarView _view;
        private readonly TextTable _texts;
        private readonly Func<PaintTool> _held;
        private readonly PlazaZone _plaza;
        private readonly Floors _floors;

        public PaintBarReasons(IPaintBarView view, TextTable texts, Func<PaintTool> held, World world)
        {
            _view = view;
            _texts = texts;
            _held = held;
            _plaza = world.Plaza;
            _floors = world.Floors;
            _plaza.PlazaRefused += OnRefused;
            _floors.FloorRefused += OnFloorRefused;
        }

        public void Dispose()
        {
            _plaza.PlazaRefused -= OnRefused;
            _floors.FloorRefused -= OnFloorRefused;
            _view.ShowReason("");
        }

        /// <summary>La réponse de l'outil en main a changé, ou l'outil : la bulle dit ce qu'il y a à dire.</summary>
        public void Show()
        {
            PaintTool held = _held();
            if (held == null)
                _view.ShowReason("");
            else if (held.HoveredMark != null)
                _view.ShowReason(_texts.Get(TextKeys.PlazaRemoveMark));
            else if (!held.Answer.IsOk && !held.Answer.ChangesNothing)
                _view.ShowReason(_texts.Get(held.Answer.ReasonKey));
            else
                _view.ShowReason("");
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
            if (_held() != null && !refusal.ChangesNothing)
                _view.ShowReason(_texts.Get(refusal.ReasonKey));
        }
    }
}
