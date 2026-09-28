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
    /// c'est une consigne de la barre, pas une règle des ordres (décision 95). Il écoute l'outil et les poses : la barre
    /// ne photographie jamais.
    /// </summary>
    public sealed class BuildBarPresenter : IDisposable
    {
        private readonly IBuildBarView _view;
        private readonly Catalog _catalog;
        private readonly TextTable _texts;
        private readonly PlacementTool _tool;
        private readonly World _world;
        private readonly bool _catalogHasAHome;
        private readonly List<string> _names = new List<string>();
        private readonly List<StructureDefinition> _offered = new List<StructureDefinition>();
        private int _openFamily;

        public BuildBarPresenter(IBuildBarView view, Catalog catalog, TextTable texts, PlacementTool tool, World world)
        {
            if (view == null || catalog == null || ReferenceEquals(texts, null) || ReferenceEquals(tool, null) ||
                ReferenceEquals(world, null))
                throw new ArgumentException("La barre de construction a besoin de sa vue, du catalogue, des textes, " +
                                            "de l'outil de pose et du monde.");

            _view = view;
            _catalog = catalog;
            _texts = texts;
            _tool = tool;
            _world = world;
            _catalogHasAHome = HasAHome(catalog);
            _view.FamilyChosen += OnFamilyChosen;
            _view.StructureChosen += OnStructureChosen;
            _tool.CurrentChanged += OnCurrentChanged;
            _tool.ResultChanged += OnResultChanged;
            _world.Structures.StructurePlaced += OnStructurePlaced;
            OnFamilyChosen(0);
            OnResultChanged();
        }

        public void Dispose()
        {
            _view.FamilyChosen -= OnFamilyChosen;
            _view.StructureChosen -= OnStructureChosen;
            _tool.CurrentChanged -= OnCurrentChanged;
            _tool.ResultChanged -= OnResultChanged;
            _world.Structures.StructurePlaced -= OnStructurePlaced;
        }

        private void OnFamilyChosen(int index)
        {
            IReadOnlyList<StructureFamilyDefinition> families = _catalog.Families;
            if (index < 0 || index >= families.Count)
                return;

            _openFamily = index;
            _names.Clear();
            foreach (StructureFamilyDefinition family in families)
                _names.Add(_texts.Get(family.NameKey));
            _view.ShowFamilies(_names, index);
            ShowOpenFamily();
        }

        /// <summary>Une pose change ce qui se propose : une construction unique s'en va, et le logis ouvre le reste.</summary>
        private void OnStructurePlaced(Structure structure)
        {
            ShowOpenFamily();
        }

        /// <summary>Montre ce que la famille ouverte propose, et la construction en main.</summary>
        private void ShowOpenFamily()
        {
            _offered.Clear();
            _names.Clear();
            if (_openFamily < _catalog.Families.Count)
            {
                foreach (StructureDefinition structure in _catalog.Families[_openFamily].Structures)
                {
                    if (!IsOffered(structure))
                        continue;
                    _offered.Add(structure);
                    _names.Add(_texts.Get(structure.NameKey));
                }
            }
            _view.ShowStructures(_names);
            OnCurrentChanged();
        }

        private bool IsOffered(StructureDefinition structure)
        {
            if (!_world.PlacementRules.IsAvailable(structure))
                return false;
            return structure.IsChimeraHome || !_catalogHasAHome || _world.Structures.ChimeraHome != null;
        }

        private void OnStructureChosen(int index)
        {
            if (index >= 0 && index < _offered.Count)
                _tool.Begin(_offered[index]);
        }

        private void OnCurrentChanged()
        {
            _view.SelectStructure(_offered.IndexOf(_tool.Current));
        }

        private void OnResultChanged()
        {
            bool refused = _tool.IsActive && _tool.Result != PlacementResult.Ok;
            _view.ShowReason(refused ? _texts.Get(TextKeys.PlacementRefusal(_tool.Result.ToString())) : "");
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
