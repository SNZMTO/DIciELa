using System.Collections;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le filtre de rêverie de la prévision (décision 228), sur la caméra des tests : un filtre sur tout l'écran, du
    /// profil de la rêverie, éteint tant que la prévision l'est ; il entre en fondu, jusqu'au bout, et sort de même, le
    /// temps que disent les réglages de la caméra (ici une demi-seconde).
    /// </summary>
    public sealed class ForesightFilterTests
    {
        [UnityTest]
        public IEnumerator TheReverieFadesInAndOut()
        {
            using (var camera = new TestCamera())
            {
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                try
                {
                    var filter = camera.Holder.AddComponent<ForesightFilter>();
#if UNITY_EDITOR
                    TestWorld.Fill(camera.Settings, "_foresightBlendSeconds", 0.5f);
                    TestWorld.Fill(filter, "_profile", profile);
                    TestWorld.Fill(filter, "_settings", camera.Settings);
#endif
                    var foresight = new Foresight();
                    filter.Show(foresight);
                    var volume = filter.GetComponentInChildren<Volume>();
                    Assert.IsTrue(volume.isGlobal, "Un filtre sur tout l'écran…");
                    Assert.AreSame(profile, volume.sharedProfile, "… du profil de la rêverie…");
                    Assert.AreEqual(0f, volume.weight, "… éteint tant que la prévision l'est.");

                    foresight.Set(true);
                    yield return null;
                    Assert.That(volume.weight, Is.GreaterThan(0f).And.LessThan(1f), "Il entre en fondu…");
                    yield return new WaitForSecondsRealtime(0.6f);
                    Assert.AreEqual(1f, volume.weight, "… jusqu'au bout…");
                    foresight.Set(false);
                    yield return new WaitForSecondsRealtime(0.6f);
                    Assert.AreEqual(0f, volume.weight, "… et sort de même.");
                }
                finally
                {
                    Object.Destroy(profile);
                }
            }
        }
    }
}
