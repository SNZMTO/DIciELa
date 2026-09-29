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
    /// de sa couleur ; la petite cape prend celle du corps. Une pièce pend à l'os de son emplacement, droite, à sa taille,
    /// son pivot au point de son repère sur la tête choisie plus son décalage, où que soit posé le personnage ; sa couleur
    /// remplace la sienne, et son verre reste du verre. Une pièce à contact descend jusqu'à toucher le dessus de la tête ;
    /// une pièce qu'une autre cache ne se pose pas ; une cape remplace la petite cape du corps.
    /// </summary>
    public sealed class DresserTests
    {
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
        public void ThePieceHangsUprightFromItsBoneAtItsHeadPointPlusOffsetAtItsSizeInItsColorAndKeepsItsGlass()
        {
            var crown = new Vector3(0f, 1.7f, 0.05f);
            var offset = new Vector3(0.1f, 0.2f, -0.1f);
            Material glass = _rig.Keep(new Material(_rig.OriginMaterial));
            AttachmentDefinition piece = _rig.Piece("Chapeau de test", AttachmentFrame.Crown, ContactAxis.None, offset, 2f,
                glass);
#if UNITY_EDITOR
            TestWorld.Fill(_rig.Head, "_crown", crown);
#endif
            Transform place = _rig.Keep(new GameObject("Place de test")).transform;
            place.SetPositionAndRotation(new Vector3(10f, 2f, -5f), Quaternion.Euler(0f, 135f, 0f));

            GameObject dressed = Dresser.Dress(new Appearance(_rig.Head, _rig.Origin, _rig.Body, _rig.Origin,
                new[] { new WornAttachment(piece, _rig.Chosen) }), place);

            Transform worn = dressed.transform.Find("root/pelvis/head/Chapeau de test(Clone)");
            Assert.IsNotNull(worn, "Le chapeau pend à l'os de la tête.");
            Assert.Less(Vector3.Distance(place.TransformPoint(crown + offset), worn.position), 1e-4f,
                "Son pivot est au haut du crâne de la tête choisie, plus son décalage, dans les axes du personnage.");
            Assert.Less(Quaternion.Angle(place.rotation, worn.rotation), 0.01f, "Il est droit, comme le personnage.");
            Assert.Less(Vector3.Distance(Vector3.one * 2f, worn.lossyScale), 1e-4f, "Il est à sa taille, malgré les os.");
            Assert.AreEqual(new[] { _rig.ChosenMaterial, glass }, worn.GetComponent<MeshRenderer>().sharedMaterials,
                "Sa couleur remplace la sienne ; son verre reste du verre.");
        }

        [Test]
        public void APieceWithContactComesDownOnTheHeadTopUntilItTouches()
        {
            // Le dessus de la tête : un plateau à 1,8 m, sauf une bosse à 1,9 m sous x ≥ 0. Le dessous de la pièce : 10 cm
            // sous son pivot partout. À taille 2, elle touche la bosse : son pivot à 1,9 + 0,2, moins 2 cm d'enfoncement.
            var top = new SurfaceMap(new Vector2(-0.5f, -0.5f), 0.5f, 2, 2, new[] { 1.8f, 1.9f, 1.8f, 1.9f });
            var under = new SurfaceMap(new Vector2(-0.5f, -0.5f), 0.5f, 2, 2, new[] { -0.1f, -0.1f, -0.1f, -0.1f });
            AttachmentDefinition piece = _rig.Piece("Chapeau posé", AttachmentFrame.Crown, ContactAxis.Down,
                new Vector3(0f, -0.02f, 0f), 2f, null);
#if UNITY_EDITOR
            TestWorld.Fill(_rig.Head, "_top", top);
            TestWorld.Fill(piece, "_shape", under);
#endif
            GameObject dressed = _rig.Keep(Dresser.Dress(new Appearance(_rig.Head, _rig.Origin, _rig.Body, _rig.Origin,
                new[] { new WornAttachment(piece, _rig.Origin) }), null));

            Transform worn = dressed.transform.Find("root/pelvis/head/Chapeau posé(Clone)");
            Assert.AreEqual(1.9f + 0.2f - 0.02f, worn.position.y, 1e-3f, "Elle s'arrête sur le plus haut du crâne.");
        }

        [Test]
        public void APieceThatAnotherHidesIsNotWorn()
        {
            AttachmentDefinition ears = _rig.Piece("Oreilles de test", AttachmentFrame.Ears, ContactAxis.None, Vector3.zero,
                1f, null);
            AttachmentDefinition hat = _rig.Piece("Chapeau qui cache", AttachmentFrame.Crown, ContactAxis.None, Vector3.zero,
                1f, null);
#if UNITY_EDITOR
            TestWorld.FillList(hat, "_hides", ears.Slot);
#endif
            GameObject dressed = _rig.Keep(Dresser.Dress(new Appearance(_rig.Head, _rig.Origin, _rig.Body, _rig.Origin,
                new[] { new WornAttachment(ears, _rig.Origin), new WornAttachment(hat, _rig.Origin) }), null));

            Assert.IsNull(dressed.transform.Find("root/pelvis/head/Oreilles de test(Clone)"), "Le chapeau les cache.");
            Assert.IsNotNull(dressed.transform.Find("root/pelvis/head/Chapeau qui cache(Clone)"));
        }
    }
}
