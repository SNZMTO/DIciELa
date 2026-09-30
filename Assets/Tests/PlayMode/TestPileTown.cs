using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le petit monde des tests de l'interface du tas : plat, 40 × 40 m, la Chimère née au centre ; le Bois et la Pierre ;
    /// un tas de départ de 2 × 2 m qui tient 50 marchandises et ouvre le poste « à tout faire » (porter) ; le déroulé du
    /// départ, qui le pose avec 30 Bois et 20 Pierre quand la tente est plantée. Les fiches sont remplies par
    /// SerializedObject, le chemin de l'Inspector, et détruites avec lui.
    /// </summary>
    internal sealed class TestPileTown : IDisposable
    {
        private readonly List<Object> _created = new List<Object>();

        public TestPileTown()
        {
#if UNITY_EDITOR
            var settings = Keep(ScriptableObject.CreateInstance<SimulationSettings>());
            Wood = Good("bois", "marchandise.bois");
            Stone = Good("pierre", "marchandise.pierre");
            var transport = Keep(ScriptableObject.CreateInstance<SubStatisticDefinition>());
            var trade = Keep(ScriptableObject.CreateInstance<StatisticDefinition>());
            TestWorld.FillList(trade, "_subStatistics", transport);
            var carry = Keep(ScriptableObject.CreateInstance<TaskKindDefinition>());
            TestWorld.Fill(carry, "_subStatistic", transport);
            var slot = Keep(ScriptableObject.CreateInstance<WorkSlotDefinition>());
            TestWorld.Fill(slot, "_nameKey", "poste.a_tout_faire");
            TestWorld.FillList(slot, "_tasks", carry);
            Pile = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            TestWorld.Fill(Pile, "_id", "tas");
            TestWorld.Fill(Pile, "_nameKey", "construction.tas_de_depart");
            TestWorld.Fill(Pile, "_widthMeters", 2f);
            TestWorld.Fill(Pile, "_depthMeters", 2f);
            TestWorld.Fill(Pile, "_hasDoor", false);
            TestWorld.Fill(Pile, "_unique", true);
            TestWorld.Fill(Pile, "_builtWhenPlaced", true);
            TestWorld.Fill(Pile, "_storageCapacity", 50);
            TestWorld.FillList(Pile, "_workSlots", slot);
            var opening = Keep(ScriptableObject.CreateInstance<OpeningDefinition>());
            TestWorld.Fill(opening, "_startingPile", Pile);
            FillGoods(opening, "_startingGoods", new GoodAmount(Wood, 30), new GoodAmount(Stone, 20));
            Catalog = Keep(ScriptableObject.CreateInstance<Catalog>());
            TestWorld.Fill(Catalog, "_opening", opening);
            TestWorld.FillList(Catalog, "_goods", Wood, Stone);
            TestWorld.FillList(Catalog, "_statistics", trade);
            var kind = Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            TestWorld.Fill(kind, "_id", "chimere");
            Tent = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            TestWorld.Fill(Tent, "_widthMeters", 3f);
            TestWorld.Fill(Tent, "_depthMeters", 3f);
            TestWorld.Fill(Tent, "_unique", true);
            TestWorld.Fill(Tent, "_chimeraHome", true);
            TestWorld.Fill(Tent, "_builtWhenPlaced", true);

            World = new World(settings, new MapDescription(new Relief(40f, 40f, 2, 2, new float[4])), Catalog);
            World.Submit(new CreateChimeraCommand(kind, TestWorld.PlainAppearance(o => Keep(o))));
            World.Step();
#endif
        }

        public World World { get; }
        public Catalog Catalog { get; }
        public GoodDefinition Wood { get; }
        public GoodDefinition Stone { get; }
        public StructureDefinition Pile { get; }
        public StructureDefinition Tent { get; }

        /// <summary>Le tas de départ, une fois posé ; null avant.</summary>
        public Structure PlacedPile
        {
            get
            {
                foreach (Structure structure in World.Structures.All)
                {
                    if (ReferenceEquals(structure.Definition, Pile))
                        return structure;
                }
                return null;
            }
        }

        /// <summary>Plante la tente en (20 ; 14), porte au nord, par l'ordre « poser » : le tas se pose avec elle.</summary>
        public void PlantTheTent()
        {
            World.Submit(new PlaceStructureCommand(Tent, new Placement(new Vector2(20f, 14f), 0f)));
            World.Step();
        }

        /// <summary>Une construction de 4 × 4 m qui coûte ces marchandises, gardée pour être détruite avec le monde.</summary>
        public StructureDefinition Cabin(params GoodAmount[] cost)
        {
            var cabin = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            FillGoods(cabin, "_cost", cost);
#endif
            return cabin;
        }

        public T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        public void Dispose()
        {
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

#if UNITY_EDITOR
        private GoodDefinition Good(string id, string nameKey)
        {
            var good = Keep(ScriptableObject.CreateInstance<GoodDefinition>());
            TestWorld.Fill(good, "_id", id);
            TestWorld.Fill(good, "_nameKey", nameKey);
            return good;
        }

        private static void FillGoods(Object target, string field, params GoodAmount[] amounts)
        {
            var serialized = new UnityEditor.SerializedObject(target);
            UnityEditor.SerializedProperty list = serialized.FindProperty(field);
            list.arraySize = amounts.Length;
            for (int i = 0; i < amounts.Length; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_good").objectReferenceValue = amounts[i].Good;
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_amount").intValue = amounts[i].Amount;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }
}
