using System;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.View;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Game
{
    /// <summary>
    /// Ce qu'on voit du monde (ARCHITECTURE, section 11) : ses constructions, ses habitants, ses arbres et ses rochers, ses
    /// charrettes, ses sols ; montrés ensemble à l'ouverture, suivis ensemble à chaque image, cachés ensemble à la sortie.
    /// Les trois premières vues sont dans la scène du jeu ; celles des charrettes et des sols naissent ici, sous la scène
    /// du démarrage.
    /// </summary>
    internal sealed class WorldViews
    {
        private readonly StructureViews _structures;
        private readonly VillagerViews _villagers;
        private readonly NatureViews _nature;
        private readonly Transform _parent;
        private CartViews _carts;
        private FloorViews _floors;

        public WorldViews(StructureViews structures, VillagerViews villagers, NatureViews nature, Transform parent)
        {
            _structures = structures != null ? structures : throw new ArgumentNullException(nameof(structures));
            _villagers = villagers != null ? villagers : throw new ArgumentNullException(nameof(villagers));
            _nature = nature != null ? nature : throw new ArgumentNullException(nameof(nature));
            _parent = parent != null ? parent : throw new ArgumentNullException(nameof(parent));
        }

        /// <summary>
        /// Montre ce monde, dont les arbres et les sols sont peints sur le Terrain de cette carte ; les fantômes des sols
        /// du matériau et de la couleur de ces réglages de la pose.
        /// </summary>
        public void Show(World world, MapScene map, PlacementSettings placement)
        {
            _structures.Show(world.Structures, world.ConstructionSites, world.Map.Relief);
            _villagers.Show(world.Villagers, world.Map.Relief);
            _nature.Show(world.Nature, map.Terrain, map.NatureInstances, world.Map.Relief);
            var holder = new GameObject("Charrettes");
            holder.transform.SetParent(_parent, false);
            _carts = holder.AddComponent<CartViews>();
            _carts.Show(world.Carts, world.Map.Relief);
            var floors = new GameObject("Sols");
            floors.transform.SetParent(_parent, false);
            _floors = floors.AddComponent<FloorViews>();
            _floors.Show(world, map.Terrain, map.OriginalGround, placement);
        }

        /// <summary>Donne aux habitants et aux charrettes la fraction du pas en cours, et les secondes de l'image.</summary>
        public void Follow(float stepFraction, float deltaSeconds)
        {
            _villagers.Follow(stepFraction, deltaSeconds);
            if (_carts != null)
                _carts.Follow(stepFraction, deltaSeconds);
        }

        /// <summary>
        /// En prévision, le monde se voit tel qu'il sera une fois tous ses chantiers finis : constructions bâties, sols
        /// peints, arbres marqués et sols effacés partis (décision 228) ; sinon, tel qu'il est.
        /// </summary>
        public void ShowForesight(bool on)
        {
            _structures.ShowForesight(on);
            _nature.ShowForesight(on);
            if (_floors != null)
                _floors.ShowForesight(on);
        }

        /// <summary>Cache tout ce qu'on montrait ; chaque vue accepte de n'avoir rien à cacher.</summary>
        public void Hide()
        {
            _structures.Hide();
            _villagers.Hide();
            _nature.Hide();
            if (_carts != null)
                Object.Destroy(_carts.gameObject);
            _carts = null;
            if (_floors != null)
                Object.Destroy(_floors.gameObject);
            _floors = null;
        }
    }
}
