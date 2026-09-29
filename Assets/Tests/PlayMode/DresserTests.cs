using System;
using System.Collections;
using System.Linq;
using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// L'habillage (SAVOIR, 2.11), sur le personnage de TestRig. Habillé, il perd ce qu'il portait (ses corps, sa pièce
    /// d'office, sa cape cachée), garde sa petite cape, et reçoit la tête et le corps choisis sur ses propres os, chacun
    /// de sa couleur ; la petite cape prend celle du corps. Une pièce pend à l'os de son emplacement, droite, son pivot sur
    /// son point d'accroche, où que soit posé le personnage ; sa couleur remplace la sienne, et son verre reste du verre.
    /// </summary>
    public sealed class DresserTests
    {
        private static readonly Vector3 HeadTop = new Vector3(0f, 1.7f, -0.1f);

        private TestRig _rig;

        [SetUp]
        public void CreateRig()
        {
            _rig = new TestRig();
        }

        [TearDown]
        public void DestroyRig()
        {
            _rig.Dispose();
        }

        [UnityTest]
        public IEnumerator TheCharacterWearsTheChosenHeadAndBodyOnItsOwnBonesAndNothingElse()
        {
            GameObject dressed = _rig.Keep(Dresser.Dress(
                new Appearance(_rig.Head, _rig.Chosen, _rig.Body, _rig.Origin, Array.Empty<WornAttachment>()), null));
            yield return null;

            SkinnedMeshRenderer[] parts = dressed.transform.Cast<Transform>()
                .Select(child => child.GetComponent<SkinnedMeshRenderer>()).Where(skin => skin != null).ToArray();
            Assert.AreEqual(new[] { "Corps", "Tête" }, parts.Select(skin => skin.name).ToArray(),
                "Ses corps du pack sont retirés ; la tête et le corps choisis les remplacent.");
            Transform pelvis = dressed.transform.Find("root/pelvis");
            foreach (SkinnedMeshRenderer part in parts)
            {
                Assert.AreSame(pelvis, part.rootBone);
                Assert.AreEqual(new[] { "root", "pelvis", "head" }, part.bones.Select(bone => bone.name).ToArray());
                Assert.IsTrue(part.bones.All(bone => bone.IsChildOf(dressed.transform)), "Sur ses propres os.");
            }
            Assert.AreSame(_rig.Body.Mesh, parts[0].sharedMesh);
            Assert.AreSame(_rig.OriginMaterial, parts[0].sharedMaterial);
            Assert.AreSame(_rig.Head.Mesh, parts[1].sharedMesh);
            Assert.AreSame(_rig.ChosenMaterial, parts[1].sharedMaterial);
            Assert.IsEmpty(dressed.GetComponentsInChildren<MeshRenderer>(true), "Sa pièce d'office est retirée.");
            Assert.IsNull(dressed.transform.Find("Capes/Cape cachée"), "Sa cape cachée est retirée.");
            Assert.IsNotNull(dressed.transform.Find("Capes/Petite cape"), "Sa petite cape reste, comme un habit.");
        }

        [UnityTest]
        public IEnumerator TheBodyColorDressesTheLittleCapeToo()
        {
            GameObject dressed = _rig.Keep(Dresser.Dress(
                new Appearance(_rig.Head, _rig.Origin, _rig.Body, _rig.Chosen, Array.Empty<WornAttachment>()), null));
            yield return null;

            Assert.AreSame(_rig.ChosenMaterial, dressed.transform.Find("Corps").GetComponent<Renderer>().sharedMaterial);
            Assert.AreSame(_rig.ChosenMaterial,
                dressed.transform.Find("Capes/Petite cape").GetComponent<Renderer>().sharedMaterial);
        }

        [Test]
        public void ThePieceHangsUprightFromItsBoneOnItsAnchorInItsColorAndKeepsItsGlass()
        {
            Material glass = _rig.Keep(new Material(_rig.OriginMaterial));
            GameObject hat = _rig.Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            hat.name = "Chapeau de test";
            hat.GetComponent<MeshRenderer>().sharedMaterials = new[] { _rig.OriginMaterial, glass };
            var slot = _rig.Keep(ScriptableObject.CreateInstance<AttachmentSlotDefinition>());
            var piece = _rig.Keep(ScriptableObject.CreateInstance<AttachmentDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(slot, "_bone", "head");
            TestWorld.Fill(piece, "_slot", slot);
            TestWorld.Fill(piece, "_model", hat);
            TestWorld.Fill(piece, "_palette", _rig.Origin);
            TestWorld.Fill(piece, "_anchor", HeadTop);
#endif
            Transform place = _rig.Keep(new GameObject("Place de test")).transform;
            place.SetPositionAndRotation(new Vector3(10f, 2f, -5f), Quaternion.Euler(0f, 135f, 0f));

            GameObject dressed = Dresser.Dress(new Appearance(_rig.Head, _rig.Origin, _rig.Body, _rig.Origin,
                new[] { new WornAttachment(piece, _rig.Chosen) }), place);

            Transform worn = dressed.transform.Find("root/pelvis/head/Chapeau de test(Clone)");
            Assert.IsNotNull(worn, "Le chapeau pend à l'os de la tête.");
            Assert.Less(Vector3.Distance(place.TransformPoint(HeadTop), worn.position), 1e-4f,
                "Son pivot est sur son point d'accroche, dans les axes du personnage.");
            Assert.Less(Quaternion.Angle(place.rotation, worn.rotation), 0.01f, "Il est droit, comme le personnage.");
            Assert.AreEqual(new[] { _rig.ChosenMaterial, glass }, worn.GetComponent<MeshRenderer>().sharedMaterials,
                "Sa couleur remplace la sienne ; son verre reste du verre.");
        }
    }
}
