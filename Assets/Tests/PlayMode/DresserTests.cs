using System.Collections;
using System.Collections.Generic;
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
    /// L'habillage (SAVOIR, 2.11), sur un petit personnage monté comme ceux du pack : un squelette (le bassin tourné de
    /// 23°, la tête d'un autre angle), un corps allumé dans sa couleur d'origine, un corps caché. Habillé d'une autre
    /// couleur et d'un chapeau, il perd son corps caché, prend la couleur choisie sur son matériau partagé, et le chapeau
    /// pend à l'os de la tête, droit, son pivot sur son point d'accroche, où que soit posé le personnage.
    /// </summary>
    public sealed class DresserTests
    {
        private static readonly Vector3 HeadTop = new Vector3(0f, 1.7f, -0.1f);

        private readonly List<Object> _created = new List<Object>();
        private Material _origin;
        private Material _chosen;
        private Appearance _appearance;

        [SetUp]
        public void CreateCharacter()
        {
            Shader shader = Shader.Find("Hidden/InternalErrorShader");
            _origin = Keep(new Material(shader));
            _chosen = Keep(new Material(shader));

            GameObject character = Keep(new GameObject("Personnage de test"));
            Transform pelvis = Child(character.transform, "pelvis", new Vector3(0f, 0.55f, 0f), new Vector3(0f, 0f, 23f));
            Child(pelvis, "head", new Vector3(0f, 0.66f, 0f), new Vector3(0f, 90f, 316f));
            Child(character.transform, "Corps", Vector3.zero, Vector3.zero).gameObject.AddComponent<SkinnedMeshRenderer>()
                .sharedMaterial = _origin;
            Child(character.transform, "Corps caché", Vector3.zero, Vector3.zero).gameObject
                .AddComponent<SkinnedMeshRenderer>().gameObject.SetActive(false);
            GameObject hat = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            hat.name = "Chapeau de test";

            var origin = Keep(ScriptableObject.CreateInstance<PaletteDefinition>());
            var chosen = Keep(ScriptableObject.CreateInstance<PaletteDefinition>());
            var body = Keep(ScriptableObject.CreateInstance<BodyDefinition>());
            var slot = Keep(ScriptableObject.CreateInstance<AttachmentSlotDefinition>());
            var piece = Keep(ScriptableObject.CreateInstance<AttachmentDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(origin, "_material", _origin);
            TestWorld.Fill(chosen, "_material", _chosen);
            TestWorld.Fill(body, "_model", character);
            TestWorld.Fill(body, "_palette", origin);
            TestWorld.Fill(slot, "_bone", "head");
            TestWorld.Fill(piece, "_slot", slot);
            TestWorld.Fill(piece, "_model", hat);
            TestWorld.Fill(piece, "_anchor", HeadTop);
#endif
            _appearance = new Appearance(body, chosen, new[] { piece });
        }

        [TearDown]
        public void DestroyCharacter()
        {
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        [UnityTest]
        public IEnumerator TheCharacterLosesItsHiddenBodyAndWearsTheChosenColor()
        {
            GameObject dressed = Keep(Dresser.Dress(_appearance, null));
            yield return null;

            SkinnedMeshRenderer[] skins = dressed.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.AreEqual(1, skins.Length, "Le corps caché est retiré.");
            Assert.AreSame(_chosen, skins[0].sharedMaterial, "La couleur choisie remplace celle d'origine.");
            Assert.AreSame(_origin, _appearance.Body.Model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial,
                "Le modèle du pack ne change pas.");
        }

        [Test]
        public void ThePieceHangsFromItsBoneUprightOnItsAnchorWhereverTheCharacterStands()
        {
            var place = Keep(new GameObject("Place de test")).transform;
            place.SetPositionAndRotation(new Vector3(10f, 2f, -5f), Quaternion.Euler(0f, 135f, 0f));

            GameObject dressed = Dresser.Dress(_appearance, place);

            Transform hat = dressed.transform.Find("pelvis/head/Chapeau de test(Clone)");
            Assert.IsNotNull(hat, "Le chapeau pend à l'os de la tête.");
            Assert.Less(Vector3.Distance(place.TransformPoint(HeadTop), hat.position), 1e-4f,
                "Son pivot est sur son point d'accroche, dans les axes du personnage.");
            Assert.Less(Quaternion.Angle(place.rotation, hat.rotation), 0.01f, "Il est droit, comme le personnage.");
        }

        private static Transform Child(Transform parent, string name, Vector3 position, Vector3 angles)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.SetLocalPositionAndRotation(position, Quaternion.Euler(angles));
            return child;
        }

        private T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }
    }
}
