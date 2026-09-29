using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les réglages d'une pièce par porteur (SAVOIR, 2.11), sur le personnage de TestRig habillé comme en jeu : un
    /// décalage ne vaut que sur ses têtes, ou ses corps pour une pièce du corps ; il peut donner à la pièce un autre modèle
    /// une autre taille sur ses têtes (la barbe élargie sur la grenouille).
    /// </summary>
    public sealed class WearerAdjustmentTests
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

        [Test]
        public void AnAdjustmentMovesThePieceOnItsHeadOnly()
        {
            HeadDefinition cat = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            HeadDefinition frog = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            AttachmentDefinition piece = _rig.Piece("Pièce de test", AttachmentFrame.Ears, ContactAxis.None,
                Vector3.zero, 1f, null);
            Adjust(piece, new Object[] { frog }, new Vector3(0f, -0.08f, -0.05f));

            Assert.That(Vector3.Distance(new Vector3(0f, 1.8f, 0f), _rig.Worn(cat, piece).position), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(new Vector3(0f, 1.72f, -0.05f), _rig.Worn(frog, piece).position),
                Is.LessThan(1e-4f), "Sur la grenouille seulement, le réglage s'ajoute.");
        }

        [Test]
        public void APieceOfTheBodyIsAdjustedByBodyNotByHead()
        {
            HeadDefinition head = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            AttachmentDefinition cape = _rig.Piece("Cape de test", AttachmentFrame.Body, ContactAxis.None,
                new Vector3(0f, 1.2f, -0.2f), 1f, null);
            Adjust(cape, new Object[] { head }, new Vector3(0f, 0f, -0.1f));
            Assert.AreEqual(-0.2f, _rig.Worn(head, cape).position.z, 1e-4f, "Une pièce du corps ne se règle pas par tête.");

            Adjust(cape, new Object[] { _rig.Body }, new Vector3(0f, 0f, -0.1f));
            Assert.AreEqual(-0.3f, _rig.Worn(head, cape).position.z, 1e-4f, "Elle se règle par corps.");
        }

        [Test]
        public void AnAdjustmentCanGiveThePieceAnotherSizeOnItsHeads()
        {
            HeadDefinition cat = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            HeadDefinition frog = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            AttachmentDefinition beard = _rig.Piece("Barbe agrandie", AttachmentFrame.Face, ContactAxis.None, Vector3.zero,
                1f, null);
            Adjust(beard, new Object[] { frog }, Vector3.zero);
#if UNITY_EDITOR
            var serialized = new UnityEditor.SerializedObject(beard);
            serialized.FindProperty("_adjustments").GetArrayElementAtIndex(0).FindPropertyRelative("_scale").vector3Value =
                Vector3.one * 1.25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
#endif
            Assert.AreEqual(1f, _rig.Worn(cat, beard).lossyScale.x, 1e-4f, "Sur le chat, sa taille.");
            Assert.AreEqual(1.25f, _rig.Worn(frog, beard).lossyScale.x, 1e-4f, "Sur la grenouille, agrandie.");
        }

        // Un seul réglage sur cette pièce : ce décalage, sur ces têtes ou ces corps.
        private static void Adjust(AttachmentDefinition piece, Object[] wearers, Vector3 offset)
        {
#if UNITY_EDITOR
            var serialized = new UnityEditor.SerializedObject(piece);
            UnityEditor.SerializedProperty list = serialized.FindProperty("_adjustments");
            list.arraySize = 1;
            UnityEditor.SerializedProperty adjustment = list.GetArrayElementAtIndex(0);
            UnityEditor.SerializedProperty heads = adjustment.FindPropertyRelative("_heads");
            UnityEditor.SerializedProperty bodies = adjustment.FindPropertyRelative("_bodies");
            heads.arraySize = 0;
            bodies.arraySize = 0;
            foreach (Object wearer in wearers)
            {
                UnityEditor.SerializedProperty target = wearer is BodyDefinition ? bodies : heads;
                target.arraySize++;
                target.GetArrayElementAtIndex(target.arraySize - 1).objectReferenceValue = wearer;
            }
            adjustment.FindPropertyRelative("_offset").vector3Value = offset;
            serialized.ApplyModifiedPropertiesWithoutUndo();
#endif
        }
    }
}
