using Diciela.Controls;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La roue de Construire et les idées (GDD, sections 13 et 16 ; décisions 190 et 226), sur le petit monde du
    /// campement, installé : deux familles, le Logement (le campement, la maison d'un lit) et les Métiers (l'atelier,
    /// dont le bâtisseur apporte l'idée). Une famille qui ne propose rien ne paraît pas ; elle paraît à sa place dès
    /// que le bâtisseur arrive avec son idée. L'outil de pose est celui de ce monde ; les autres outils, ceux du petit
    /// monde des tests, ne servent pas ici.
    /// </summary>
    public sealed class BuildRingIdeasTests
    {
        private const string Texts = "clé,fr\n" +
                                     "famille.logement,Logement\n" +
                                     "famille.metiers,Métiers\n" +
                                     "construction.atelier,Atelier\n";

        private TestWorld _test;
        private TestCampTown _town;
        private StructureDefinition _house;
        private PlacementTool _placement;
        private NotedToolWheelView _view;
        private ToolWheelPresenter _presenter;

        [SetUp]
        public void CreateWheel()
        {
            _test = new TestWorld();
            _town = new TestCampTown();
            _house = _town.HouseWithABed();
            var workshop = _town.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(workshop, "_nameKey", "construction.atelier");
            TestWorld.Fill(_town.Builder, "_workplace", workshop);
            TestWorld.FillList(_town.Catalog, "_families",
                _test.Family("famille.logement", Pastel.Rose, _town.Camp, _house),
                _test.Family("famille.metiers", Pastel.Peche, workshop));
#endif
            _town.InstallTheCamp();
            _placement = new PlacementTool(_town.World, _test.Camera, _test.Preview, _test.Settings);
            _view = new NotedToolWheelView();
            _presenter = new ToolWheelPresenter(_view, TextTable.Parse(Texts, "fr"), new FixedToolKeys(), _town.Catalog,
                _town.World, _placement, _test.Clearing, _test.Mover, _test.Paths, _test.Plaza);
        }

        [TearDown]
        public void DestroyWheel()
        {
            _placement.Dispose();
            _town.Dispose();
            _test.Dispose();
        }

        [Test]
        public void TheTradesAppearWhenTheBuilderBringsTheIdea()
        {
            _presenter.Choose(ToolChoice.Build);
            Assert.AreSame(_house, _placement.Current, "Le campement installé, Construire prend la maison.");
            _presenter.Open(false);
            Assert.IsNull(_view.At(2), "Personne n'a l'idée d'un métier : les Métiers attendent.");

            _town.World.Submit(new PlaceStructureCommand(_house, new Placement(new Vector2(30f, 30f), 0f)));
            _town.World.Step();

            _presenter.Open(false);
            Assert.AreEqual("Métiers", _view.At(2)?.Name,
                "Le bâtisseur est arrivé, avec son idée : les Métiers, à droite.");
            Assert.AreEqual("Atelier", _view.At(2).Options[0].Name);
        }
    }
}
