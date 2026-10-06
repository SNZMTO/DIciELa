using System;
using Diciela.Data;
using Diciela.Simulation.Society;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>Les apparences des tests, pour un monde que personne ne regarde.</summary>
    internal static class TestAppearance
    {
        /// <summary>
        /// Une apparence sans pièce ni modèle : une tête et un corps d'une couleur sans matériau ; ses fiches sont confiées
        /// à keep, pour être détruites avec le test.
        /// </summary>
        public static Appearance Plain(Action<Object> keep)
        {
            var palette = ScriptableObject.CreateInstance<PaletteDefinition>();
            var head = ScriptableObject.CreateInstance<HeadDefinition>();
            var body = ScriptableObject.CreateInstance<BodyDefinition>();
            keep(palette);
            keep(head);
            keep(body);
            return new Appearance(head, palette, body, palette, Array.Empty<WornAttachment>());
        }
    }
}
