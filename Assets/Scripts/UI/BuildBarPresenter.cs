using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la barre de construction (ARCHITECTURE, section 11) : il la remplit depuis le catalogue et la
    /// table des textes, met dans la main de l'outil de pose la construction choisie, et dit pourquoi une pose est
    /// refusée. Il ne propose que ce qui se pose encore : une construction unique posée s'en va. La tente d'abord (GDD,
    /// section 6) : tant que la Chimère n'a pas de logis, la barre ne propose que son logis, si le catalogue en a un ;
    /// c'est une consigne de la barre, pas une règle des ordres (décision 95). Le bouton de l'outil « Retirer » paraît
    /// avec le reste : un clic prend l'outil en main, un second le lâche, et il reste enfoncé tant que l'outil est en main
    /// (décision 167). Il écoute les outils et les poses : la barre ne photographie jamais.
    /// </summary>
    public sealed class BuildBarPresenter : IDisposable
    {
        private readonly IBuildBarView _view;
        private readonly Catalog _catalog;
        private readonly TextTable _texts;
        private readonly PlacementTool _placement;
        private readonly ClearingTool _clearing;
        private readonly World _world;
        private readonly bool _catalogHasAHome;
        private readonly List<BuildBarFamily> _families = new List<BuildBarFamily>();
        private readonly List<BuildBarCard> _cards = new List<BuildBarCard>();
        private readonly List<StructureDefinition> _offered = new List<StructureDefinition>();
        private int _openFamily;

        public BuildBarPresenter(IBuildBarView view, Catalog catalog, TextTable texts, PlacementTool placement,
            ClearingTool clearing, World world)
        {
            if (view == null || catalog == null || ReferenceEquals(texts, null) || ReferenceEquals(placement, null) ||
                ReferenceEquals(clearing, null) || ReferenceEquals(world, null))
                throw new ArgumentException("La barre de construction a besoin de sa vue, du catalogue, des textes, " +
                                            "des outils de pose et « Retirer », et du monde.");

            _view = view;
            _catalog = catalog;
            _texts = texts;
            _placement = placement;
            _clearing = clearing;
            _world = world;
            _catalogHasAHome = HasAHome(catalog);
            _view.FamilyChosen += OnFamilyChosen;
            _view.StructureChosen += OnStructureChosen;
            _view.ClearingChosen += OnClearingChosen;
            _placement.CurrentChanged += OnCurrentChanged;
            _placement.ResultChanged += OnResultChanged;
            _clearing.ActiveChanged += ShowClearing;
            _world.Structures.StructurePlaced += OnStructurePlaced;
            OnFamilyChosen(0);
            OnResultChanged();
            ShowClearing();
        }

        public void Dispose()
        {
            _view.FamilyChosen -= OnFamilyChosen;
            _view.StructureChosen -= OnStructureChosen;
            _view.ClearingChosen -= OnClearingChosen;
            _placement.CurrentChanged -= OnCurrentChanged;
            _placement.ResultChanged -= OnResultChanged;
            _clearing.ActiveChanged -= ShowClearing;
            _world.Structures.StructurePlaced -= OnStructurePlaced;
        }

        /// <summary>L'infobulle du bouton de l'outil « Retirer » : son nom, et comment s'en servir.</summary>
        public (string Title, string Detail) ClearingTooltip()
        {
            return (_texts.Get(TextKeys.ClearingTool), _texts.Get(TextKeys.ClearingToolDetail));
        }

        // La tente d'abord : tant que la Chimère n'a pas de logis, la barre ne propose que lui.
        private bool OffersEverything => !_catalogHasAHome || _world.Structures.ChimeraHome != null;

        private void OnFamilyChosen(int index)
        {
            IReadOnlyList<StructureFamilyDefinition> families = _catalog.Families;
            if (index < 0 || index >= families.Count)
                return;

            _openFamily = index;
            _families.Clear();
            foreach (StructureFamilyDefinition family in families)
                _families.Add(new BuildBarFamily(_texts.Get(family.NameKey), family.Icon, family.Pastel));
            _view.ShowFamilies(_families, index);
            ShowOpenFamily();
        }

        /// <summary>Une pose change ce qui se propose : une construction unique s'en va, et le logis ouvre le reste.</summary>
        private void OnStructurePlaced(Structure structure)
        {
            ShowOpenFamily();
            ShowClearing();
        }

        /// <summary>Montre ce que la famille ouverte propose, et la construction en main.</summary>
        private void ShowOpenFamily()
        {
            _offered.Clear();
            _cards.Clear();
            if (_openFamily < _catalog.Families.Count)
            {
                StructureFamilyDefinition family = _catalog.Families[_openFamily];
                foreach (StructureDefinition structure in family.Structures)
                {
                    if (!IsOffered(structure))
                        continue;
                    _offered.Add(structure);
                    _cards.Add(new BuildBarCard(_texts.Get(structure.NameKey), structure.Picture, family.Pastel,
                        structure.Cost));
                }
            }
            _view.ShowStructures(_cards);
            OnCurrentChanged();
        }

        private bool IsOffered(StructureDefinition structure)
        {
            if (!_world.PlacementRules.IsAvailable(structure))
                return false;
            return structure.IsChimeraHome || OffersEverything;
        }

        private void OnStructureChosen(int index)
        {
            if (index >= 0 && index < _offered.Count)
                _placement.Begin(_offered[index]);
        }

        private void OnCurrentChanged()
        {
            _view.SelectStructure(_offered.IndexOf(_placement.Current));
        }

        private void OnResultChanged()
        {
            bool refused = _placement.IsActive && _placement.Result != PlacementResult.Ok;
            _view.ShowReason(refused ? _texts.Get(TextKeys.PlacementRefusal(_placement.Result.ToString())) : "");
        }

        private void OnClearingChosen()
        {
            if (_clearing.IsActive)
                _clearing.Escape();
            else
                _clearing.Take();
        }

        private void ShowClearing()
        {
            _view.ShowClearing(OffersEverything, _clearing.IsActive);
        }

        private static bool HasAHome(Catalog catalog)
        {
            foreach (StructureFamilyDefinition family in catalog.Families)
            {
                foreach (StructureDefinition structure in family.Structures)
                {
                    if (structure != null && structure.IsChimeraHome)
                        return true;
                }
            }
            return false;
        }
    }
}
