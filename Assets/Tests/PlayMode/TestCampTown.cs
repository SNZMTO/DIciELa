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
    /// Le petit monde des tests de l'interface du campement : plat, 40 × 40 m (ou la carte donnée), la Chimère née au
    /// centre (sauf si le test la crée lui-même) ; le Bois et la Pierre ; un campement de 3 × 3 m qui tient 100
    /// marchandises et ouvre le poste « à tout faire » (porter, puis défricher) ; le déroulé du départ, qui lui donne
    /// 30 Bois et 20 Pierre quand on l'installe. Les fiches sont remplies par SerializedObject, le chemin de
    /// l'Inspector, et détruites avec lui.
    /// </summary>
    internal sealed class TestCampTown : IDisposable
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly VillagerKindDefinition _kind;

        public TestCampTown(bool withChimera = true, MapDescription map = null)
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
            var landscape = Keep(ScriptableObject.CreateInstance<SubStatisticDefinition>());
            var land = Keep(ScriptableObject.CreateInstance<StatisticDefinition>());
            TestWorld.FillList(land, "_subStatistics", landscape);
            var clear = Keep(ScriptableObject.CreateInstance<TaskKindDefinition>());
            TestWorld.Fill(clear, "_work", (int)TaskWork.Clear);
            TestWorld.Fill(clear, "_subStatistic", landscape);
            var slot = Keep(ScriptableObject.CreateInstance<WorkSlotDefinition>());
            TestWorld.Fill(slot, "_nameKey", "poste.a_tout_faire");
            TestWorld.FillList(slot, "_tasks", carry, clear);
            var opening = Keep(ScriptableObject.CreateInstance<OpeningDefinition>());
            FillGoods(opening, "_startingGoods", new GoodAmount(Wood, 30), new GoodAmount(Stone, 20));
            Catalog = Keep(ScriptableObject.CreateInstance<Catalog>());
            TestWorld.Fill(Catalog, "_opening", opening);
            TestWorld.FillList(Catalog, "_goods", Wood, Stone);
            TestWorld.FillList(Catalog, "_statistics", trade, land);
            _kind = Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            TestWorld.Fill(_kind, "_id", "chimere");
            Camp = Keep(ScriptableObject.CreateInstance<StructureDefinition>());
            TestWorld.Fill(Camp, "_id", "campement");
            TestWorld.Fill(Camp, "_nameKey", "construction.campement");
            TestWorld.Fill(Camp, "_widthMeters", 3f);
            TestWorld.Fill(Camp, "_depthMeters", 3f);
            TestWorld.Fill(Camp, "_unique", true);
            TestWorld.Fill(Camp, "_chimeraHome", true);
            TestWorld.Fill(Camp, "_builtWhenPlaced", true);
            TestWorld.Fill(Camp, "_storageCapacity", 100);
            TestWorld.FillList(Camp, "_workSlots", slot);

            World = new World(settings, map ?? new MapDescription(new Relief(40f, 40f, 2, 2, new float[4])), Catalog);
#endif
            if (withChimera)
                CreateTheChimera();
        }

        /// <summary>Crée la Chimère par son ordre, au centre de la carte.</summary>
        public void CreateTheChimera()
        {
            World.Submit(new CreateChimeraCommand(_kind, TestWorld.PlainAppearance(o => Keep(o))));
            World.Step();
        }

        public World World { get; }
        public Catalog Catalog { get; }
        public GoodDefinition Wood { get; }
        public GoodDefinition Stone { get; }
        public StructureDefinition Camp { get; }

        /// <summary>Le campement, une fois installé ; null avant.</summary>
        public Structure PlacedCamp => World.Structures.ChimeraHome;

        /// <summary>Installe le campement en (20 ; 14), porte au nord, par l'ordre « poser » : il reçoit le stock de départ.</summary>
        public void InstallTheCamp()
        {
            World.Submit(new PlaceStructureCommand(Camp, new Placement(new Vector2(20f, 14f), 0f)));
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
