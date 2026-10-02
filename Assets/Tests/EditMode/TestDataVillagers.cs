using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La fabrique de fiches des tests, suite : les habitants ordinaires et les premiers habitants (GDD, sections 4 à 6),
    /// et les maisons qui les logent. Même règle : en mémoire, remplies par SerializedObject.
    /// </summary>
    internal static partial class TestData
    {
        /// <summary>Un personnage du pack, sans modèle : la simulation ne le lit pas.</summary>
        public static CharacterDefinition Character(string id)
        {
            CharacterDefinition character = ScriptableObject.CreateInstance<CharacterDefinition>();
            var serialized = new SerializedObject(character);
            Property(serialized, "_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return character;
        }

        /// <summary>Un premier habitant, fort dans cette sous-statistique, avec ce personnage ; nom : habitant.{id}.</summary>
        public static FirstVillagerDefinition FirstVillager(string id, SubStatisticDefinition strength,
            CharacterDefinition character)
        {
            FirstVillagerDefinition first = ScriptableObject.CreateInstance<FirstVillagerDefinition>();
            var serialized = new SerializedObject(first);
            Property(serialized, "_id").stringValue = id;
            Property(serialized, "_nameKey").stringValue = "habitant." + id;
            Property(serialized, "_strength").objectReferenceValue = strength;
            Property(serialized, "_character").objectReferenceValue = character;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return first;
        }

        /// <summary>Dit, dans ce déroulé du départ, le poste ouvert pour chaque premier habitant au campement.</summary>
        public static void FirstVillagersSlot(OpeningDefinition opening, WorkSlotDefinition slot)
        {
            var serialized = new SerializedObject(opening);
            Property(serialized, "_firstVillagersSlot").objectReferenceValue = slot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Donne à ce premier habitant son lieu de travail, dont il apporte l'idée en arrivant.</summary>
        public static void Workplace(FirstVillagerDefinition first, StructureDefinition workplace)
        {
            var serialized = new SerializedObject(first);
            Property(serialized, "_workplace").objectReferenceValue = workplace;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Dit, dans ce déroulé du départ, les premiers habitants, dans l'ordre de leur arrivée.</summary>
        public static void FirstVillagers(OpeningDefinition opening, params FirstVillagerDefinition[] firsts)
        {
            var serialized = new SerializedObject(opening);
            Fill(Property(serialized, "_firstVillagers"), firsts);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// La sorte des habitants ordinaires : de 5 à 20 en arrivant, ils rentrent dormir la nuit ; leur marche et leur
        /// flânerie, celles de la Chimère du banc.
        /// </summary>
        public static VillagerKindDefinition OrdinaryKind()
        {
            VillagerKindDefinition kind = VillagerKind("habitant");
            var serialized = new SerializedObject(kind);
            Property(serialized, "_startingStatisticMin").floatValue = 5f;
            Property(serialized, "_startingStatisticMax").floatValue = 20f;
            Property(serialized, "_sleepsAtNight").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return kind;
        }

        /// <summary>Donne à ce catalogue la sorte de ses habitants ordinaires.</summary>
        public static void Ordinary(Catalog catalog, VillagerKindDefinition kind)
        {
            var serialized = new SerializedObject(catalog);
            Property(serialized, "_villagerKind").objectReferenceValue = kind;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Une maison de size mètres de côté, porte de 1 m au nord, qui offre ces lits ; dressée dès la pose, pour que les
        /// tests n'aient pas à la bâtir.
        /// </summary>
        public static StructureDefinition House(string id, float size, int beds)
        {
            StructureDefinition house = Structure(id, size, size);
            var serialized = new SerializedObject(house);
            Property(serialized, "_builtWhenPlaced").boolValue = true;
            Property(serialized, "_beds").intValue = beds;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return house;
        }

        /// <summary>Donne à cette construction ces postes, une fois bâtie (aucun : elle n'en ouvre pas).</summary>
        public static void Posts(StructureDefinition structure, params WorkSlotDefinition[] posts)
        {
            var serialized = new SerializedObject(structure);
            Fill(Property(serialized, "_workSlots"), posts);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Donne à cette construction ces lits, une fois bâtie.</summary>
        public static void Beds(StructureDefinition structure, int beds)
        {
            var serialized = new SerializedObject(structure);
            Property(serialized, "_beds").intValue = beds;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
