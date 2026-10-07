using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur du panneau Construire (ARCHITECTURE, section 11) : il le remplit depuis le catalogue et la table des
    /// textes, met dans la main de l'outil de pose la construction choisie, ou l'outil de la Place (sa carte, en tête de
    /// sa famille : décision 206), et dit pourquoi une pose est refusée. Le
    /// panneau s'ouvre à la demande (la roue des outils, sa touche) et se ferme quand on prend une carte, pour dégager la
    /// vue pendant la pose, ou par Échap (décision 168). Ce qu'il propose, BuildBarOffer le dit (ce qui se pose encore,
    /// le campement d'abord), ses onglets, BuildBarTabs (une famille qui ne propose rien ne paraît pas), et ses cartes,
    /// BuildBarCards (ce que chacune offre, ses lits, et ce qu'elle coûte). Panneau ouvert, les touches 1 à 9 prennent les
    /// cartes de la famille ouverte, comme un clic, et chaque carte porte sa touche (décision 184). Il écoute l'outil, les
    /// poses, les chantiers finis et les idées : le panneau ne photographie jamais. La raison d'un refus est l'une des
    /// voix de la bulle des raisons.
    /// </summary>
    public sealed class BuildBarPresenter : IDisposable
    {
        private readonly IBuildBarView _view;
        private readonly TextTable _texts;
        private readonly PlacementTool _placement;
        private readonly PaintTool _plaza;
        private readonly World _world;
        private readonly BuildBarTabs _tabs;
        private readonly BuildBarCards _cards;
        private readonly List<BuildBarFamily> _families = new List<BuildBarFamily>();

        public BuildBarPresenter(IBuildBarView view, Catalog catalog, TextTable texts, PlacementTool placement,
            PaintTool plaza, World world, IToolKeys keys)
        {
            if (view == null || catalog == null || ReferenceEquals(texts, null) || ReferenceEquals(placement, null) ||
                ReferenceEquals(plaza, null) || ReferenceEquals(world, null) || keys == null)
                throw new ArgumentException("Le panneau Construire a besoin de sa vue, du catalogue, des textes, " +
                                            "de l'outil de pose, de celui de la Place, du monde et des touches.");

            _view = view;
            _texts = texts;
            _placement = placement;
            _plaza = plaza;
            _world = world;
            var offer = new BuildBarOffer(catalog, world);
            _tabs = new BuildBarTabs(catalog, offer);
            _cards = new BuildBarCards(texts, catalog.Plaza, offer, keys);
            _view.FamilyChosen += OnFamilyChosen;
            _view.StructureChosen += OnStructureChosen;
            _placement.HeldChanged += OnHeldChanged;
            _plaza.HeldChanged += OnHeldChanged;
            _placement.ResultChanged += OnResultChanged;
            _world.Structures.StructurePlaced += OnStructuresChanged;
            _world.Structures.StructureBuilt += OnStructuresChanged;
            _world.Knowledge.Learned += OnLearned;
            ShowTabs();
            OnResultChanged();
            _view.ShowOpen(false);
        }

        /// <summary>Vrai tant que le panneau est ouvert.</summary>
        public bool IsOpen { get; private set; }

        public void Dispose()
        {
            _view.FamilyChosen -= OnFamilyChosen;
            _view.StructureChosen -= OnStructureChosen;
            _placement.HeldChanged -= OnHeldChanged;
            _plaza.HeldChanged -= OnHeldChanged;
            _placement.ResultChanged -= OnResultChanged;
            _world.Structures.StructurePlaced -= OnStructuresChanged;
            _world.Structures.StructureBuilt -= OnStructuresChanged;
            _world.Knowledge.Learned -= OnLearned;
        }

        public void Open()
        {
            if (IsOpen)
                return;
            IsOpen = true;
            _view.ShowOpen(true);
        }

        public void Close()
        {
            if (!IsOpen)
                return;
            IsOpen = false;
            _view.ShowOpen(false);
        }

        /// <summary>
        /// La touche d'une carte prend la carte de ce rang dans la famille ouverte, comme un clic ; panneau fermé, elle
        /// ne fait rien.
        /// </summary>
        public void TakeCard(int index)
        {
            if (IsOpen)
                OnStructureChosen(index);
        }

        /// <summary>Échap ferme le panneau ouvert ; faux s'il était fermé.</summary>
        public bool Escape()
        {
            bool wasOpen = IsOpen;
            Close();
            return wasOpen;
        }

        private void OnFamilyChosen(int index)
        {
            if (_tabs.Choose(index))
                ShowTabs();
        }

        /// <summary>
        /// Une pose ou un chantier fini change ce qui se propose : une construction unique posée s'en va, et le logis
        /// bâti ouvre le reste, qu'il se dresse dès la pose ou sorte de son chantier.
        /// </summary>
        private void OnStructuresChanged(Structure structure)
        {
            ShowTabs();
        }

        /// <summary>Une idée : la ville sait bâtir une construction de plus, et son onglet peut paraître.</summary>
        private void OnLearned(StructureDefinition structure)
        {
            ShowTabs();
        }

        /// <summary>Montre les onglets qui paraissent, celui qui est ouvert, et ce qu'il propose.</summary>
        private void ShowTabs()
        {
            _tabs.Refresh();
            _families.Clear();
            foreach (StructureFamilyDefinition family in _tabs.Shown)
                _families.Add(new BuildBarFamily(_texts.Get(family.NameKey), family.Icon, family.Pastel));
            _view.ShowFamilies(_families, _tabs.OpenIndex);
            ShowOpenFamily();
        }

        /// <summary>Montre ce que la famille ouverte propose, et ce qui est en main.</summary>
        private void ShowOpenFamily()
        {
            _cards.Fill(_tabs.Open);
            _view.ShowStructures(_cards.Cards);
            OnHeldChanged();
        }

        /// <summary>Prendre une carte met en main sa construction, ou l'outil de la Place, et ferme le panneau.</summary>
        private void OnStructureChosen(int index)
        {
            StructureDefinition structure = _cards.StructureAt(index);
            if (_cards.IsPlaza(index))
                _plaza.Take();
            else if (structure != null)
                _placement.Begin(structure);
            else
                return;
            Close();
        }

        private void OnHeldChanged()
        {
            _view.SelectStructure(_plaza.IsActive && _cards.HasPlaza ? 0 : _cards.IndexOf(_placement.Current));
        }

        private void OnResultChanged()
        {
            bool refused = _placement.IsActive && _placement.Result != PlacementResult.Ok;
            _view.ShowReason(refused ? _texts.Get(TextKeys.PlacementRefusal(_placement.Result.ToString())) : "");
        }
    }
}
