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
    /// textes, met dans la main de l'outil de pose la construction choisie, et dit pourquoi une pose est refusée. Le
    /// panneau s'ouvre à la demande (la roue des outils, sa touche) et se ferme quand on prend une carte, pour dégager la
    /// vue pendant la pose, ou par Échap (décision 168). Il ne propose que ce qui se pose encore : une construction unique
    /// posée s'en va. La tente d'abord (GDD, section 6) : tant que la Chimère n'a pas de logis, il ne propose que son
    /// logis, si le catalogue en a un ; c'est une consigne du panneau, pas une règle des ordres (décision 95). Il écoute
    /// l'outil et les poses : le panneau ne photographie jamais.
    /// </summary>
    public sealed class BuildBarPresenter : IDisposable
    {
        private readonly IBuildBarView _view;
        private readonly Catalog _catalog;
        private readonly TextTable _texts;
        private readonly PlacementTool _placement;
        private readonly World _world;
        private readonly bool _catalogHasAHome;
        private readonly List<BuildBarFamily> _families = new List<BuildBarFamily>();
        private readonly List<BuildBarCard> _cards = new List<BuildBarCard>();
        private readonly List<StructureDefinition> _offered = new List<StructureDefinition>();
        private int _openFamily;

        public BuildBarPresenter(IBuildBarView view, Catalog catalog, TextTable texts, PlacementTool placement,
            World world)
        {
            if (view == null || catalog == null || ReferenceEquals(texts, null) || ReferenceEquals(placement, null) ||
                ReferenceEquals(world, null))
                throw new ArgumentException("Le panneau Construire a besoin de sa vue, du catalogue, des textes, " +
                                            "de l'outil de pose et du monde.");

            _view = view;
            _catalog = catalog;
            _texts = texts;
            _placement = placement;
            _world = world;
            _catalogHasAHome = HasAHome(catalog);
            _view.FamilyChosen += OnFamilyChosen;
            _view.StructureChosen += OnStructureChosen;
            _placement.HeldChanged += OnHeldChanged;
            _placement.ResultChanged += OnResultChanged;
            _world.Structures.StructurePlaced += OnStructurePlaced;
            OnFamilyChosen(0);
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
            _placement.ResultChanged -= OnResultChanged;
            _world.Structures.StructurePlaced -= OnStructurePlaced;
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

        /// <summary>Échap ferme le panneau ouvert ; faux s'il était fermé.</summary>
        public bool Escape()
        {
            bool wasOpen = IsOpen;
            Close();
            return wasOpen;
        }

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
            OnHeldChanged();
        }

        private bool IsOffered(StructureDefinition structure)
        {
            if (!_world.PlacementRules.IsAvailable(structure))
                return false;
            return structure.IsChimeraHome || !_catalogHasAHome || _world.Structures.ChimeraHome != null;
        }

        /// <summary>Prendre une carte met sa construction en main et ferme le panneau.</summary>
        private void OnStructureChosen(int index)
        {
            if (index < 0 || index >= _offered.Count)
                return;
            _placement.Begin(_offered[index]);
            Close();
        }

        private void OnHeldChanged()
        {
            _view.SelectStructure(_offered.IndexOf(_placement.Current));
        }

        private void OnResultChanged()
        {
            bool refused = _placement.IsActive && _placement.Result != PlacementResult.Ok;
            _view.ShowReason(refused ? _texts.Get(TextKeys.PlacementRefusal(_placement.Result.ToString())) : "");
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
