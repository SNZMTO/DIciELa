using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La fabrique de fiches des tests, suite : la charrette de l'étal et la Place ; ce qu'une construction garde de
    /// charrettes, ce qu'elle tient, si elle est unique ou dressée dès la pose. Même règle : en mémoire, remplies par
    /// SerializedObject.
    /// </summary>
    internal static partial class TestData
    {
        /// <summary>
        /// La charrette de l'étal : tant de places, tirée à cette part du pas de qui la tire, au bout d'un timon de tant de
        /// mètres ; ses places de charrette, à la taille de sa fiche par défaut ; elle a ces modèles, s'il y en a.
        /// </summary>
        public static CartDefinition Cart(int capacity, float speedFactor, float pullMeters, params GameObject[] models)
        {
            CartDefinition cart = ScriptableObject.CreateInstance<CartDefinition>();
            var serialized = new SerializedObject(cart);
            Property(serialized, "_id").stringValue = "charrette";
            Property(serialized, "_capacity").intValue = capacity;
            Property(serialized, "_speedFactor").floatValue = speedFactor;
            Property(serialized, "_pullMeters").floatValue = pullMeters;
            Fill(Property(serialized, "_models"), models);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return cart;
        }

        /// <summary>Donne à cette construction, une fois bâtie, tant de charrettes à garder.</summary>
        public static void Carts(StructureDefinition structure, int carts)
        {
            var serialized = new SerializedObject(structure);
            Property(serialized, "_carts").intValue = carts;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// La Place (décision 204) : au départ, au plus tant de mètres carrés et tant de places de charrette ; ses niveaux,
        /// ceux de sa fiche par défaut.
        /// </summary>
        public static PlaceDefinition Place(float startAreaSquareMeters, int startCartSpots)
        {
            PlaceDefinition place = ScriptableObject.CreateInstance<PlaceDefinition>();
            var serialized = new SerializedObject(place);
            Property(serialized, "_startAreaSquareMeters").floatValue = startAreaSquareMeters;
            Property(serialized, "_startCartSpots").intValue = startCartSpots;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return place;
        }

        /// <summary>Donne à ce catalogue cette Place.</summary>
        public static void UsePlace(Catalog catalog, PlaceDefinition place)
        {
            var serialized = new SerializedObject(catalog);
            Property(serialized, "_place").objectReferenceValue = place;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Dresse cette construction dès sa pose, sans chantier : le test n'a pas à la bâtir.</summary>
        public static void BuiltWhenPlaced(StructureDefinition structure)
        {
            var serialized = new SerializedObject(structure);
            Property(serialized, "_builtWhenPlaced").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Donne à ce catalogue cette charrette.</summary>
        public static void UseCart(Catalog catalog, CartDefinition cart)
        {
            var serialized = new SerializedObject(catalog);
            Property(serialized, "_cart").objectReferenceValue = cart;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Fait de cette construction une construction unique : une seule dans la ville.</summary>
        public static void Unique(StructureDefinition structure)
        {
            var serialized = new SerializedObject(structure);
            Property(serialized, "_unique").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Fait de cette construction un entrepôt de tant de places.</summary>
        public static void Storage(StructureDefinition structure, int capacity)
        {
            var serialized = new SerializedObject(structure);
            Property(serialized, "_storageCapacity").intValue = capacity;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
