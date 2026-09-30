using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La fiche d'un lieu et son présentateur (GDD, section 16 ; décisions 141 et 152), sur une vue qui note : toucher le
    /// tas de départ ouvre sa fiche (son nom, son Bois, son poste libre, « Y mettre la Chimère ») ; le bouton y met la
    /// Chimère, puis l'en retire, par les ordres ; toucher autre chose, ou rien, la ferme, comme la croix ; la fiche suit
    /// le tas quand la Chimère y prend du Bois. Un monde plat de 40 m, la tente plantée en (20 ; 14).
    /// </summary>
    public sealed class PlaceSheetPresenterTests
    {
        private const string Texts = "clé,fr\n" +
                                     "construction.tas_de_depart,Tas de départ\n" +
                                     "marchandise.bois,Bois\n" +
                                     "poste.a_tout_faire,À tout faire\n" +
                                     "fiche.mettre_chimere,Y mettre la Chimère\n" +
                                     "fiche.retirer_chimere,Retirer la Chimère\n" +
                                     "fiche.la_chimere,La Chimère\n" +
                                     "fiche.poste_libre,Libre\n";

        private readonly List<Object> _created = new List<Object>();
        private World _world;
        private NotedPlaceSheetView _view;
        private PlaceSheetPresenter _presenter;
        private StructureDefinition _pile;
        private GoodDefinition _wood;

        private T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        [SetUp]
        public void CreateTown()
        {
#if UNITY_EDITOR
            var settings = Keep(ScriptableObject.CreateInstance<SimulationSettings>());
            _wood = Keep(ScriptableObject.CreateInstance<GoodDefinition>());
            TestWorld.Fill(_wood, "_id", "bois");
            TestWorld.Fill(_wood, "_nameKey", "marchandise.bois");
            var transport = Keep(ScriptableObject.CreateInstance<SubStatisticDefinition>());
            var trade = Keep(ScriptableObject.CreateInstance<StatisticDefinition>());
            TestWorld.FillList(trade, "_subStatistics", transport);
            var carry = Keep(ScriptableObject.CreateInstance<TaskKindDefinition>());
            TestWorld.Fill(carry, "_subStatistic", transport);
            var slot = Keep(ScriptableObject.CreateInstance<WorkSlotDefinition>());
            TestWorld.Fill(slot, "_nameKey", "poste.a_tout_faire");
            TestWorld.FillList(slot, "_tasks", carry);
            _pile = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            TestWorld.Fill(_pile, "_id", "tas");
            TestWorld.Fill(_pile, "_nameKey", "construction.tas_de_depart");
            TestWorld.Fill(_pile, "_widthMeters", 2f);
            TestWorld.Fill(_pile, "_depthMeters", 2f);
            TestWorld.Fill(_pile, "_hasDoor", false);
            TestWorld.Fill(_pile, "_unique", true);
            TestWorld.Fill(_pile, "_builtWhenPlaced", true);
            TestWorld.Fill(_pile, "_storageCapacity", 50);
            TestWorld.FillList(_pile, "_workSlots", slot);
            var opening = Keep(ScriptableObject.CreateInstance<OpeningDefinition>());
            TestWorld.Fill(opening, "_startingPile", _pile);
            FillGoods(opening, "_startingGoods", _wood, 30);
            var catalog = Keep(ScriptableObject.CreateInstance<Catalog>());
            TestWorld.Fill(catalog, "_opening", opening);
            TestWorld.FillList(catalog, "_statistics", trade);
            var kind = Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            TestWorld.Fill(kind, "_id", "chimere");
            var tent = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            TestWorld.Fill(tent, "_widthMeters", 3f);
            TestWorld.Fill(tent, "_depthMeters", 3f);
            TestWorld.Fill(tent, "_unique", true);
            TestWorld.Fill(tent, "_chimeraHome", true);
            TestWorld.Fill(tent, "_builtWhenPlaced", true);

            _world = new World(settings, new MapDescription(new Relief(40f, 40f, 2, 2, new float[4])), catalog);
            _world.Submit(new CreateChimeraCommand(kind, TestWorld.PlainAppearance(o => Keep(o))));
            _world.Submit(new PlaceStructureCommand(tent, new Placement(new Vector2(20f, 14f), 0f)));
            _world.Step();
#endif
            _view = new NotedPlaceSheetView();
            _presenter = new PlaceSheetPresenter(_view, _world, TextTable.Parse(Texts, "fr"));
        }

        [TearDown]
        public void DestroyTown()
        {
            _presenter.Dispose();
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        private Structure Pile
        {
            get
            {
                foreach (Structure structure in _world.Structures.All)
                {
                    if (ReferenceEquals(structure.Definition, _pile))
                        return structure;
                }
                return null;
            }
        }

        [Test]
        public void TouchingThePileOpensItsSheet()
        {
            Assert.IsFalse(_view.Shown, "Fermée tant qu'on n'a rien touché.");
            _presenter.Touch(Pile);

            Assert.IsTrue(_view.Shown);
            Assert.AreEqual("Tas de départ", _view.Title);
            Assert.AreEqual(1, _view.Lines.Count);
            Assert.AreEqual("Bois", _view.Lines[0].Name);
            Assert.AreEqual(30, _view.Lines[0].Count);
            Assert.AreEqual("À tout faire", _view.SlotName);
            Assert.AreEqual("Libre", _view.SlotHolder);
            Assert.AreEqual("Y mettre la Chimère", _view.Button);
        }

        [Test]
        public void TheButtonPutsTheChimeraThereThenTakesHerAway()
        {
            _presenter.Touch(Pile);
            _view.ClickButton();
            _world.Step();
            Assert.AreEqual("La Chimère", _view.SlotHolder);
            Assert.AreEqual("Retirer la Chimère", _view.Button);

            _view.ClickButton();
            _world.Step();
            Assert.AreEqual("Libre", _view.SlotHolder);
            Assert.AreEqual("Y mettre la Chimère", _view.Button);
            Assert.IsNull(_world.Villagers.Chimera.Slot);
        }

        [Test]
        public void TouchingSomethingElseOrNothingClosesIt()
        {
            _presenter.Touch(Pile);
            _presenter.Touch(null);
            Assert.IsFalse(_view.Shown, "Toucher le sol ferme la fiche.");

            _presenter.Touch(Pile);
            _presenter.Touch(_world.Structures.ChimeraHome);
            Assert.IsFalse(_view.Shown, "La tente n'est pas un lieu de travail.");

            _presenter.Touch(Pile);
            _view.ClickClose();
            Assert.IsFalse(_view.Shown, "La croix la ferme.");
            Assert.IsFalse(_presenter.Close(), "Déjà fermée : Échap passe à la suite.");
        }

        [Test]
        public void TheSheetFollowsThePile()
        {
            _presenter.Touch(Pile);
            _world.Submit(new AssignVillagerCommand(_world.Villagers.Chimera.Id, Pile.Id));
            var cabin = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            FillGoods(cabin, "_cost", _wood, 10);
#endif
            _world.Submit(new PlaceStructureCommand(cabin, new Placement(new Vector2(28f, 20f), 0f)));
            for (int step = 0; step < 3000 && _view.Lines[0].Count == 30; step++)
                _world.Step();

            Assert.AreEqual(25, _view.Lines[0].Count, "La Chimère a pris 5 Bois : la fiche le montre.");
        }

#if UNITY_EDITOR
        private static void FillGoods(Object target, string field, GoodDefinition good, int amount)
        {
            var serialized = new UnityEditor.SerializedObject(target);
            UnityEditor.SerializedProperty list = serialized.FindProperty(field);
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).FindPropertyRelative("_good").objectReferenceValue = good;
            list.GetArrayElementAtIndex(0).FindPropertyRelative("_amount").intValue = amount;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }
}
