using System.Collections;
using Diciela.Game;
using Diciela.UI;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'ouverture d'une partie dans la scène de test : elle s'ouvre sur l'écran de création, et le temps ne part qu'à la
    /// validation (GDD, section 6). Le test valide par le même appel que le bouton et qu'Entrée.
    /// </summary>
    internal static class TestOpening
    {
        /// <summary>Attend que le monde soit né et l'écran ouvert, puis valide la Chimère de départ.</summary>
        public static IEnumerator Confirm(int maxFrames)
        {
            // La borne compte les images, jamais les secondes : une machine lente attend plus longtemps.
            for (int frame = 0; frame < maxFrames; frame++)
            {
                var runner = Object.FindAnyObjectByType<GameRunner>();
                if (runner != null && runner.World != null)
                {
                    Object.FindAnyObjectByType<CreationScreen>().Confirm();
                    yield break;
                }
                yield return null;
            }
            Assert.Fail("L'écran de création de la scène de test ne s'est pas ouvert.");
        }
    }
}
