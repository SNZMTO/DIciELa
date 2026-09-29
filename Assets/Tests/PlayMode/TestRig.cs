using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.View;
using Diciela.Simulation.Society;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Un personnage monté comme ceux du pack, pour les tests de l'habillage : un squelette (root, puis pelvis tourné de
    /// 23°, puis head d'un autre angle) ; son corps allumé et un corps caché, maillages à peau enfants directs ; une pièce
    /// d'office (un maillage sans peau sous le bassin) ; une petite cape à peau sous son propre objet, et une cape cachée.
    /// Sa tête et son corps découpés tiennent chacun un triangle, sur ce squelette. Deux couleurs : l'origine, que portent
    /// le corps allumé et la petite cape, et une autre. Tout ce qu'il crée est détruit avec lui.
    /// </summary>
    internal sealed class TestRig : IDisposable
    {
        private readonly List<Object> _created = new List<Object>();

        public TestRig()
        {
            Shader shader = Shader.Find("Hidden/InternalErrorShader");
            OriginMaterial = Keep(new Material(shader));
            ChosenMaterial = Keep(new Material(shader));
            Origin = Palette(OriginMaterial);
            Chosen = Palette(ChosenMaterial);

            Character = Keep(new GameObject("Personnage de test"));
            Transform root = Child(Character.transform, "root", Vector3.zero, Vector3.zero);
            Transform pelvis = Child(root, "pelvis", new Vector3(0f, 0.55f, 0f), new Vector3(0f, 0f, 23f));
            Transform head = Child(pelvis, "head", new Vector3(0f, 0.66f, 0f), new Vector3(0f, 90f, 316f));
            Bones = new[] { root, pelvis, head };
            Skin(Child(Character.transform, "Corps du pack", Vector3.zero, Vector3.zero), Triangle(1), true);
            Skin(Child(Character.transform, "Corps caché", Vector3.zero, Vector3.zero), Triangle(1), false);
            Transform tail = Child(pelvis, "Queue d'office", Vector3.zero, Vector3.zero);
            tail.gameObject.AddComponent<MeshFilter>();
            tail.gameObject.AddComponent<MeshRenderer>().sharedMaterial = OriginMaterial;
            Transform capes = Child(Character.transform, "Capes", Vector3.zero, Vector3.zero);
            Skin(Child(capes, "Petite cape", Vector3.zero, Vector3.zero), Triangle(1), true);
            Skin(Child(capes, "Cape cachée", Vector3.zero, Vector3.zero), Triangle(1), false);

            Head = Keep(ScriptableObject.CreateInstance<HeadDefinition>());
            Body = Keep(ScriptableObject.CreateInstance<BodyDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(Head, "_id", "tete");
            TestWorld.Fill(Head, "_mesh", Triangle(2));
            TestWorld.Fill(Head, "_palette", Origin);
            TestWorld.Fill(Body, "_id", "corps");
            TestWorld.Fill(Body, "_model", Character);
            TestWorld.Fill(Body, "_mesh", Triangle(1));
            TestWorld.Fill(Body, "_palette", Origin);
#endif
        }

        public Material OriginMaterial { get; }
        public Material ChosenMaterial { get; }
        public PaletteDefinition Origin { get; }
        public PaletteDefinition Chosen { get; }

        /// <summary>Le personnage du pack de test ; ses os, dans l'ordre de ses maillages à peau.</summary>
        public GameObject Character { get; }
        public Transform[] Bones { get; }
        public HeadDefinition Head { get; }
        public BodyDefinition Body { get; }

        /// <summary>Sa tête et son corps dans leur couleur d'origine, sans pièce.</summary>
        public Appearance Plain => new Appearance(Head, Origin, Body, Origin, Array.Empty<WornAttachment>());

        /// <summary>Une couleur de ce matériau de personnage, sans matériau d'objet, détruite avec le banc.</summary>
        public PaletteDefinition Palette(Material material)
        {
            var palette = Keep(ScriptableObject.CreateInstance<PaletteDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(palette, "_characterMaterial", material);
#endif
            return palette;
        }

        /// <summary>Une pièce de ce nom sur son propre emplacement de l'os head, dans la couleur d'origine du banc.</summary>
        public AttachmentDefinition Piece(string name, AttachmentFrame frame, ContactAxis contact, Vector3 offset,
            float scale, Material glass)
        {
            GameObject model = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            model.name = name;
            model.GetComponent<MeshRenderer>().sharedMaterials = glass != null
                ? new[] { OriginMaterial, glass }
                : new[] { OriginMaterial };
            var slot = Keep(ScriptableObject.CreateInstance<AttachmentSlotDefinition>());
            var piece = Keep(ScriptableObject.CreateInstance<AttachmentDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(slot, "_bone", "head");
            TestWorld.Fill(slot, "_frame", frame);
            TestWorld.Fill(slot, "_contact", contact);
            TestWorld.Fill(piece, "_id", name);
            TestWorld.Fill(piece, "_slot", slot);
            TestWorld.Fill(piece, "_model", model);
            TestWorld.Fill(piece, "_palette", Origin);
            TestWorld.Fill(piece, "_offset", offset);
            TestWorld.Fill(piece, "_scale", Vector3.one * scale);
#endif
            return piece;
        }

        /// <summary>Une tête du banc, sur le crâne du banc, dont l'oreille droite est à cette place.</summary>
        public HeadDefinition HeadWithEars(Vector3 earRight)
        {
            var head = Keep(ScriptableObject.CreateInstance<HeadDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(head, "_id", "tete");
            TestWorld.Fill(head, "_mesh", Head.Mesh);
            TestWorld.Fill(head, "_earRight", earRight);
#endif
            return head;
        }

        /// <summary>Le modèle de cette pièce, porté par le personnage du banc habillé de cette tête ; null s'il n'y est pas.</summary>
        public Transform Worn(HeadDefinition head, AttachmentDefinition piece)
        {
            GameObject dressed = Keep(Dresser.Dress(new Appearance(head, Origin, Body, Origin,
                new[] { new WornAttachment(piece, Origin) }), null));
            return dressed.transform.Find("root/pelvis/head/" + piece.Model.name + "(Clone)");
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

        private void Skin(Transform holder, Mesh mesh, bool shown)
        {
            var skin = holder.gameObject.AddComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = mesh;
            skin.bones = Bones;
            skin.rootBone = Bones[1];
            skin.sharedMaterial = OriginMaterial;
            holder.gameObject.SetActive(shown);
        }

        /// <summary>Un triangle mené tout entier par cet os, les poses d'origine des trois os à l'identité.</summary>
        private Mesh Triangle(int bone)
        {
            var mesh = Keep(new Mesh
            {
                vertices = new[] { Vector3.zero, Vector3.up, Vector3.right },
                triangles = new[] { 0, 1, 2 }
            });
            var weight = new BoneWeight { boneIndex0 = bone, weight0 = 1f };
            mesh.boneWeights = new[] { weight, weight, weight };
            mesh.bindposes = new[] { Matrix4x4.identity, Matrix4x4.identity, Matrix4x4.identity };
            return mesh;
        }

        private static Transform Child(Transform parent, string name, Vector3 position, Vector3 angles)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.SetLocalPositionAndRotation(position, Quaternion.Euler(angles));
            return child;
        }
    }
}
