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
    /// marchandises et ouvre le poste « à tout faire » (porter, puis défricher ; bâtir entre les deux si le test le
    /// demande) ; le déroulé du départ, qui lui donne
    /// 30 Bois et 20 Pierre quand on l'installe, qui sait bâtir le campement et ce que le test y ajoute, et fait arriver
    /// un premier habitant, le bâtisseur, dès qu'une maison a un lit (aucune n'en a, sauf celle que le test demande).
    /// Les fiches sont remplies par SerializedObject, le chemin de l'Inspector, et détruites avec lui.
    /// </summary>
    internal sealed class TestCampTown : IDisposable
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly VillagerKindDefinition _kind;
        private readonly OpeningDefinition _opening;

        public TestCampTown(bool withChimera = true, MapDescription map = null, bool builds = false)
        {
            _opening = Keep(ScriptableObject.CreateInstance<OpeningDefinition>());
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
            var build = Keep(ScriptableObject.CreateInstance<TaskKindDefinition>());
            TestWorld.Fill(build, "_work", (int)TaskWork.Build);
            TestWorld.Fill(build, "_subStatistic", transport);
            if (builds)
                TestWorld.FillList(slot, "_tasks", carry, build, clear);
            else
                TestWorld.FillList(slot, "_tasks", carry, clear);
            FillGoods(_opening, "_startingGoods", new GoodAmount(Wood, 30), new GoodAmount(Stone, 20));
            Catalog = Keep(ScriptableObject.CreateInstance<Catalog>());
            TestWorld.Fill(Catalog, "_opening", _opening);
            TestWorld.FillList(Catalog, "_goods", Wood, Stone);
            TestWorld.FillList(Catalog, "_statistics", trade, land);
            _kind = Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            TestWorld.Fill(_kind, "_id", "chimere");
            TestWorld.Fill(_kind, "_nameKey", "habitant.chimere");
            var ordinary = Keep(ScriptableObject.CreateInstance<VillagerKindDefinition>());
            TestWorld.Fill(ordinary, "_nameKey", "habitant.ordinaire");
            TestWorld.Fill(Catalog, "_villagerKind", ordinary);
            Builder = Keep(ScriptableObject.CreateInstance<FirstVillagerDefinition>());
            FirstVillagerDefinition builder = Builder;
            TestWorld.Fill(builder, "_nameKey", "habitant.batisseur");
            TestWorld.Fill(builder, "_strength", transport);
            TestWorld.Fill(builder, "_character", Keep(ScriptableObject.CreateInstance<CharacterDefinition>()));
            TestWorld.FillList(_opening, "_firstVillagers", builder);
            TestWorld.Fill(_opening, "_firstVillagersSlot", slot);
            Camp = KeepKnown(ScriptableObject.CreateInstance<StructureDefinition>());
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

        /// <summary>La ville sait tracer ces sols dès le départ, d'après le déroulé du départ.</summary>
        public void KnowsFloors(params FloorDefinition[] floors)
        {
#if UNITY_EDITOR
            TestWorld.FillList(_opening, "_knownFloors", floors);
#endif
        }

        /// <summary>Une maison d'un lit, dressée dès la pose, que la ville sait bâtir : posée, le bâtisseur arrive.</summary>
        public StructureDefinition HouseWithABed()
        {
            StructureDefinition house = KeepKnown(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(house, "_builtWhenPlaced", true);
            TestWorld.Fill(house, "_beds", 1);
#endif
            return house;
        }

        /// <summary>Crée la Chimère par son ordre, au centre de la carte.</summary>
        public void CreateTheChimera()
        {
            World.Submit(new CreateChimeraCommand(_kind, TestAppearance.Plain(o => Keep(o))));
            World.Step();
        }

        public World World { get; }
        public Catalog Catalog { get; }

        /// <summary>Le bâtisseur, premier habitant ; le test peut lui donner un lieu de travail, son idée.</summary>
        public FirstVillagerDefinition Builder { get; }
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
            StructureDefinition cabin = KeepKnown(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            FillGoods(cabin, "_cost", cost);
#endif
            return cabin;
        }

        /// <summary>Garde cette construction, et la Chimère sait la bâtir en arrivant, d'après le déroulé du départ.</summary>
        public StructureDefinition KeepKnown(StructureDefinition structure)
        {
            Keep(structure);
#if UNITY_EDITOR
            var serialized = new UnityEditor.SerializedObject(_opening);
            UnityEditor.SerializedProperty known = serialized.FindProperty("_knownStructures");
            known.arraySize++;
            known.GetArrayElementAtIndex(known.arraySize - 1).objectReferenceValue = structure;
            serialized.ApplyModifiedPropertiesWithoutUndo();
#endif
            return structure;
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

        /// <summary>
        /// Remplit une liste de marchandises comptées d'une fiche (un coût, un stock), comme l'Inspector.
        /// </summary>
        internal static void FillGoods(Object target, string field, params GoodAmount[] amounts)
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
