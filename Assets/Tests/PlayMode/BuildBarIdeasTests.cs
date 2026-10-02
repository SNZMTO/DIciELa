using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les onglets du panneau Construire et les idées (GDD, sections 13 et 16 ; décision 190), sur le petit monde du
    /// campement et une vue qui note : deux familles, le Logement (le campement, la maison d'un lit) et les Métiers
    /// (l'atelier, dont le bâtisseur apporte l'idée) ; un onglet qui ne propose rien ne paraît pas, et paraît dès que le
    /// bâtisseur arrive avec son idée.
    /// </summary>
    public sealed class BuildBarIdeasTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "famille.metiers,Métiers\n" +
                                     "construction.atelier,Atelier\n";

        private TestCampTown _town;
        private StructureDefinition _house;
        private NotedBuildBarView _view;
        private BuildBarPresenter _presenter;

        [SetUp]
        public void CreateBar()
        {
            _town = new TestCampTown();
            _house = _town.HouseWithABed();
            var workshop = _town.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            var housing = _town.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
            var trades = _town.Keep(ScriptableObject.CreateInstance<StructureFamilyDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(workshop, "_nameKey", "construction.atelier");
            TestWorld.Fill(_town.Builder, "_workplace", workshop);
            TestWorld.Fill(housing, "_nameKey", "famille.logement");
            TestWorld.FillList(housing, "_structures", _town.Camp, _house);
            TestWorld.Fill(trades, "_nameKey", "famille.metiers");
            TestWorld.FillList(trades, "_structures", workshop);
            TestWorld.FillList(_town.Catalog, "_families", housing, trades);
#endif
            _town.InstallTheCamp();
            // L'outil de pose du panneau : une caméra, un aperçu et ses réglages, que ce test ne regarde pas.
            var camera = _town.Keep(new GameObject("Caméra de test", typeof(Camera))).GetComponent<Camera>();
            var settings = _town.Keep(ScriptableObject.CreateInstance<PlacementSettings>());
            var preview = _town.Keep(new GameObject("Aperçu de test")).AddComponent<PlacementPreview>();
#if UNITY_EDITOR
            TestWorld.Fill(preview, "_settings", settings);
#endif
            _view = new NotedBuildBarView();
            _presenter = new BuildBarPresenter(_view, _town.Catalog, TextTable.Parse(Texts, "fr"),
                new PlacementTool(_town.World, camera, preview, settings), _town.World, new FixedToolKeys());
        }

        [TearDown]
        public void DestroyBar()
        {
            _presenter.Dispose();
            _town.Dispose();
        }

        [Test]
        public void TheTradesTabAppearsWhenTheBuilderBringsTheIdea()
        {
            Assert.AreEqual(new[] { "Logement" }, _view.Families, "Personne n'a l'idée d'un métier : son onglet attend.");

            _town.World.Submit(new PlaceStructureCommand(_house, new Placement(new Vector2(30f, 30f), 0f)));
            _town.World.Step();

            Assert.AreEqual(new[] { "Logement", "Métiers" }, _view.Families, "Le bâtisseur est arrivé, avec son idée.");
            _view.Open(1);
            Assert.AreEqual(new[] { "Atelier" }, _view.Structures);
        }
    }
}
