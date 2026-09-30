using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.Simulation.Society;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de l'astuce (GDD, section 16 ; décision 154) : c'est le jeu qui parle, et il dit quoi faire
    /// ensuite, au début d'une partie : planter la tente ; puis mettre la Chimère au tas ; puis poser une première
    /// construction à bâtir. Quand il n'a plus rien à dire, l'astuce se cache. Il se déduit de la ville, sans rien
    /// retenir, et l'écoute : l'arrivée de la Chimère, les poses, les chantiers finis, les postes.
    /// </summary>
    public sealed class TipPresenter : IDisposable
    {
        private readonly ITipView _view;
        private readonly World _world;
        private readonly TextTable _texts;

        public TipPresenter(ITipView view, World world, TextTable texts)
        {
            if (view == null || ReferenceEquals(world, null) || ReferenceEquals(texts, null))
                throw new ArgumentException("L'astuce a besoin de sa vue, du monde et des textes.");

            _view = view;
            _world = world;
            _texts = texts;
            _world.Villagers.VillagerArrived += OnVillagerArrived;
            _world.Structures.StructurePlaced += OnStructureChanged;
            _world.Structures.StructureBuilt += OnStructureChanged;
            _world.WorkSlots.VillagerAssigned += OnSlotChanged;
            _world.WorkSlots.VillagerUnassigned += OnSlotChanged;
            Refresh();
        }

        public void Dispose()
        {
            _world.Villagers.VillagerArrived -= OnVillagerArrived;
            _world.Structures.StructurePlaced -= OnStructureChanged;
            _world.Structures.StructureBuilt -= OnStructureChanged;
            _world.WorkSlots.VillagerAssigned -= OnSlotChanged;
            _world.WorkSlots.VillagerUnassigned -= OnSlotChanged;
        }

        private void OnVillagerArrived(Villager villager)
        {
            Refresh();
        }

        private void OnStructureChanged(Structure structure)
        {
            Refresh();
        }

        private void OnSlotChanged(Villager villager, WorkSlot slot)
        {
            Refresh();
        }

        private void Refresh()
        {
            _view.ShowTip(KeyOfTheNextThing() is string key ? _texts.Get(key) : "");
        }

        /// <summary>La clé de ce qu'il y a à faire ensuite ; null s'il n'y a plus rien à dire.</summary>
        private string KeyOfTheNextThing()
        {
            Villager chimera = _world.Villagers.Chimera;
            if (chimera == null)
                return null;
            if (_world.Structures.ChimeraHome == null)
                return TextKeys.TipTent;
            if (chimera.Slot == null)
                return _world.WorkSlots.All.Count > 0 ? TextKeys.TipPile : null;
            return HasBuiltAnything() ? null : TextKeys.TipCabin;
        }

        /// <summary>Vrai si le joueur a déjà posé une construction qui se bâtit (en chantier ou bâtie).</summary>
        private bool HasBuiltAnything()
        {
            foreach (Structure structure in _world.Structures.All)
            {
                if (!structure.Definition.BuiltWhenPlaced)
                    return true;
            }
            return false;
        }
    }
}
