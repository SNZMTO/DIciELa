using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La fabrique de fiches des tests, suite : la charrette de l'étal, et ce qu'une construction en garde ou en appelle.
    /// Même règle : en mémoire, remplies par SerializedObject.
    /// </summary>
    internal static partial class TestData
    {
        /// <summary>
        /// La charrette de l'étal : tant de places, tirée à cette part du pas de qui la tire, au bout d'un timon de tant de
        /// mètres ; elle se gare le long d'un cercle à 55 % du demi-côté de la Place, de travers de 10° au plus ; elle a
        /// ces modèles, s'il y en a.
        /// </summary>
        public static CartDefinition Cart(int capacity, float speedFactor, float pullMeters, params GameObject[] models)
        {
            CartDefinition cart = ScriptableObject.CreateInstance<CartDefinition>();
            var serialized = new SerializedObject(cart);
            Property(serialized, "_id").stringValue = "charrette";
            Property(serialized, "_capacity").intValue = capacity;
            Property(serialized, "_speedFactor").floatValue = speedFactor;
            Property(serialized, "_pullMeters").floatValue = pullMeters;
            Property(serialized, "_parkRadius").floatValue = 0.55f;
            Property(serialized, "_parkTurnDegrees").floatValue = 10f;
            Fill(Property(serialized, "_models"), models);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return cart;
        }

        /// <summary>Donne à cette construction, une fois bâtie, tant de charrettes à garder et de places de charrette.</summary>
        public static void Carts(StructureDefinition structure, int carts, int spots)
        {
            var serialized = new SerializedObject(structure);
            Property(serialized, "_carts").intValue = carts;
            Property(serialized, "_cartSpots").intValue = spots;
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

        /// <summary>Fait de cette construction un entrepôt de tant de places.</summary>
        public static void Storage(StructureDefinition structure, int capacity)
        {
            var serialized = new SerializedObject(structure);
            Property(serialized, "_storageCapacity").intValue = capacity;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
