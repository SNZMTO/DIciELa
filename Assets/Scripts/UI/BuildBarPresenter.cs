using System;
using System.Collections.Generic;
using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.UI
{
    /// <summary>
    /// Le présentateur de la barre de construction (ARCHITECTURE, section 11) : il la remplit depuis le catalogue et la
    /// table des textes, met dans la main de l'outil de pose la construction choisie, et dit pourquoi une pose est
    /// refusée. Il écoute l'outil : la barre ne photographie jamais.
    /// </summary>
    public sealed class BuildBarPresenter : IDisposable
    {
        private readonly IBuildBarView _view;
        private readonly Catalog _catalog;
        private readonly TextTable _texts;
        private readonly PlacementTool _tool;
        private readonly List<string> _names = new List<string>();
        private int _openFamily;

        public BuildBarPresenter(IBuildBarView view, Catalog catalog, TextTable texts, PlacementTool tool)
        {
            if (view == null || catalog == null || ReferenceEquals(texts, null) || ReferenceEquals(tool, null))
                throw new ArgumentException("La barre de construction a besoin de sa vue, du catalogue, des textes et " +
                                            "de l'outil de pose.");

            _view = view;
            _catalog = catalog;
            _texts = texts;
            _tool = tool;
            _view.FamilyChosen += OnFamilyChosen;
            _view.StructureChosen += OnStructureChosen;
            _tool.CurrentChanged += OnCurrentChanged;
            _tool.ResultChanged += OnResultChanged;
            OnFamilyChosen(0);
            OnResultChanged();
        }

        public void Dispose()
        {
            _view.FamilyChosen -= OnFamilyChosen;
            _view.StructureChosen -= OnStructureChosen;
            _tool.CurrentChanged -= OnCurrentChanged;
            _tool.ResultChanged -= OnResultChanged;
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

            _names.Clear();
            foreach (StructureDefinition structure in families[index].Structures)
                _names.Add(_texts.Get(structure.NameKey));
            _view.ShowStructures(_names);
            OnCurrentChanged();
        }

        private void OnStructureChosen(int index)
        {
            if (_openFamily >= _catalog.Families.Count)
                return;
            IReadOnlyList<StructureDefinition> structures = _catalog.Families[_openFamily].Structures;
            if (index >= 0 && index < structures.Count)
                _tool.Begin(structures[index]);
        }

        private void OnCurrentChanged()
        {
            int selected = -1;
            if (_openFamily < _catalog.Families.Count)
            {
                IReadOnlyList<StructureDefinition> structures = _catalog.Families[_openFamily].Structures;
                for (int i = 0; i < structures.Count; i++)
                {
                    if (ReferenceEquals(structures[i], _tool.Current))
                        selected = i;
                }
            }
            _view.SelectStructure(selected);
        }

        private void OnResultChanged()
        {
            bool refused = _tool.IsActive && _tool.Result != PlacementResult.Ok;
            _view.ShowReason(refused ? _texts.Get(TextKeys.PlacementRefusal(_tool.Result.ToString())) : "");
        }
    }
}
