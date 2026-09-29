using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La pose d'une pièce (SAVOIR, 2.11), sur le personnage de TestRig habillé comme en jeu : une boucle d'oreille suit
    /// l'oreille de son côté, chaque moitié d'une paire la sienne, d'après l'écart entre les oreilles de la tête choisie
    /// et celles de sa tête de référence ; ailleurs que sur celle-ci, une paire se pose par sa base ; une pièce tourne sur
    /// son pivot de sa rotation ; un réglage ne vaut que sur ses têtes, ou ses corps pour une pièce du corps.
    /// </summary>
    public sealed class PiecePlacementTests
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
        public void AnEarringFollowsTheEarOfItsSide()
        {
            HeadDefinition wide = Head(new Vector3(0.4f, 1.8f, 0f)), narrow = Head(new Vector3(0.3f, 1.7f, 0.1f));
            AttachmentDefinition earring = _rig.Piece("Boucle de test", AttachmentFrame.Ears, ContactAxis.None,
                new Vector3(0.4f, 0.1f, 0f), 1f, null);
#if UNITY_EDITOR
            TestWorld.Fill(earring, "_reference", wide);
#endif
            Assert.That(Vector3.Distance(new Vector3(0.4f, 1.9f, 0f), Worn(wide, earring).position), Is.LessThan(1e-4f),
                "Sur sa tête de référence, à sa place.");
            Transform onNarrow = Worn(narrow, earring);
            Assert.That(Vector3.Distance(new Vector3(0.3f, 1.8f, 0.1f), onNarrow.position), Is.LessThan(1e-4f),
                "Des oreilles 10 cm plus serrées : elle se rapproche d'autant du milieu, à la hauteur de ses oreilles.");
            Assert.AreEqual(1f, onNarrow.lossyScale.x, 1e-4f, "Elle garde sa taille.");
        }

        [Test]
        public void EachHalfOfAPairFollowsItsEar()
        {
            HeadDefinition wide = Head(new Vector3(0.4f, 1.8f, 0f)), narrow = Head(new Vector3(0.35f, 1.8f, 0f));
            AttachmentDefinition ears = Pair("Oreilles de test", ContactAxis.None, Vector3.zero);
#if UNITY_EDITOR
            TestWorld.Fill(ears, "_reference", wide);
#endif
            Transform onWide = Worn(wide, ears), onNarrow = Worn(narrow, ears);

            Assert.AreEqual(0.1f, onWide.Find("Droite").position.x, 1e-4f, "Sur sa tête de référence, à sa place.");
            Assert.AreEqual(0.05f, onNarrow.Find("Droite").position.x, 1e-4f, "Des oreilles 5 cm plus serrées.");
            Assert.AreEqual(-0.05f, onNarrow.Find("Gauche").position.x, 1e-4f, "De chaque côté.");
        }

        [Test]
        public void APairRestsByItsBaseOnAnotherHead()
        {
            // Une rangée de cases de 10 cm, de x = -0,5 à 0,5. La tête de référence est plate, à 1,8 m ; les autres ont un
            // plateau sous le dedans des oreilles (à moins de 30 cm du milieu), à 1,6 m, et plus bas au-dehors. Le dessous
            // de la paire : 10 cm sous son pivot, sous ses moitiés seulement (de 20 à 40 cm du milieu), enfoncé de 2 cm dans
            // sa tête de référence.
            var under = Row(x => Mathf.Abs(x) > 0.2f && Mathf.Abs(x) < 0.4f ? -0.1f : SurfaceMap.Empty);
            HeadDefinition reference = Head(new Vector3(0.4f, 1.8f, 0f)), gentle = Head(new Vector3(0.4f, 1.7f, 0f)),
                steep = Head(new Vector3(0.4f, 1.7f, 0f));
            AttachmentDefinition ears = Pair("Oreilles posées", ContactAxis.Down, new Vector3(0f, -0.02f, 0f));
#if UNITY_EDITOR
            TestWorld.Fill(reference, "_top", Row(x => 1.8f));
            TestWorld.Fill(gentle, "_top", Row(x => Mathf.Abs(x) < 0.3f ? 1.6f : 1.58f));
            TestWorld.Fill(steep, "_top", Row(x => Mathf.Abs(x) < 0.3f ? 1.6f : 1.5f));
            TestWorld.Fill(ears, "_shape", under);
            TestWorld.Fill(ears, "_reference", reference);
#endif
            Assert.AreEqual(1.8f + 0.1f - 0.02f, Worn(reference, ears).position.y, 1e-3f, "Sur sa tête, posée.");
            Assert.AreEqual(1.58f + 0.1f - 0.02f, Worn(gentle, ears).position.y, 1e-3f,
                "Ailleurs, par sa base : le dehors touche (enfoncé de même), le dedans s'enfonce ; rien ne flotte.");
            Transform onSteep = Worn(steep, ears);
            Assert.AreEqual(1.6f + 0.1f - 0.02f, onSteep.position.y, 1e-3f,
                "Sur un bord trop penché, elle glisse vers le milieu et porte à plat sur le plateau.");
            Assert.That(onSteep.Find("Droite").position.x, Is.InRange(0.035f, 0.065f), "De 5 cm environ, de chaque côté.");
        }

        [Test]
        public void APieceTurnsByItsRotationAroundItsPivot()
        {
            HeadDefinition head = Head(new Vector3(0.4f, 1.8f, 0f));
            AttachmentDefinition earring = _rig.Piece("Boucle de test", AttachmentFrame.Ears, ContactAxis.None,
                new Vector3(0.4f, 0f, 0f), 1f, null);
#if UNITY_EDITOR
            TestWorld.Fill(earring, "_rotation", new Vector3(0f, 0f, 90f));
#endif
            Transform worn = Worn(head, earring);

            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 0f, 90f), worn.rotation), Is.LessThan(0.01f));
            Assert.That(Vector3.Distance(new Vector3(0.4f, 1.8f, 0f), worn.position), Is.LessThan(1e-4f),
                "Elle tourne sur son pivot : il ne bouge pas.");
        }

        [Test]
        public void AnAdjustmentMovesThePieceOnItsHeadOnly()
        {
            HeadDefinition cat = Head(new Vector3(0.4f, 1.8f, 0f)), frog = Head(new Vector3(0.4f, 1.8f, 0f));
            AttachmentDefinition piece = _rig.Piece("Pièce de test", AttachmentFrame.Ears, ContactAxis.None,
                Vector3.zero, 1f, null);
            Adjust(piece, new Object[] { frog }, new Vector3(0f, -0.08f, -0.05f));

            Assert.That(Vector3.Distance(new Vector3(0f, 1.8f, 0f), Worn(cat, piece).position), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(new Vector3(0f, 1.72f, -0.05f), Worn(frog, piece).position),
                Is.LessThan(1e-4f), "Sur la grenouille seulement, le réglage s'ajoute.");
        }

        [Test]
        public void APieceOfTheBodyIsAdjustedByBodyNotByHead()
        {
            HeadDefinition head = Head(new Vector3(0.4f, 1.8f, 0f));
            AttachmentDefinition cape = _rig.Piece("Cape de test", AttachmentFrame.Body, ContactAxis.None,
                new Vector3(0f, 1.2f, -0.2f), 1f, null);
            Adjust(cape, new Object[] { head }, new Vector3(0f, 0f, -0.1f));
            Assert.AreEqual(-0.2f, Worn(head, cape).position.z, 1e-4f, "Une pièce du corps ne se règle pas par tête.");

            Adjust(cape, new Object[] { _rig.Body }, new Vector3(0f, 0f, -0.1f));
            Assert.AreEqual(-0.3f, Worn(head, cape).position.z, 1e-4f, "Elle se règle par corps.");
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

        // Une paire d'oreilles du banc : deux moitiés, à 10 cm de part et d'autre de son pivot.
        private AttachmentDefinition Pair(string name, ContactAxis contact, Vector3 offset)
        {
            AttachmentDefinition pair = _rig.Piece(name, AttachmentFrame.Ears, contact, offset, 1f, null);
#if UNITY_EDITOR
            TestWorld.Fill(pair.Slot, "_pairs", true);
            TestWorld.Fill(pair.Slot, "_seated", true);
#endif
            foreach ((string half, float side) in new[] { ("Droite", 1f), ("Gauche", -1f) })
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = half;
                cube.transform.SetParent(pair.Model.transform, false);
                cube.transform.localPosition = new Vector3(0.1f * side, 0f, 0f);
            }
            return pair;
        }

        // Une rangée de cases de 10 cm sur x, de -0,5 à 0,5, à z = 0 ; chaque case, de la hauteur donnée à son centre.
        private static SurfaceMap Row(System.Func<float, float> height)
        {
            var values = new float[10];
            for (int i = 0; i < values.Length; i++)
                values[i] = height(-0.45f + 0.1f * i);
            return new SurfaceMap(new Vector2(-0.5f, -0.05f), 0.1f, 10, 1, values);
        }

        // Une tête du banc dont l'oreille droite est à cette place.
        private HeadDefinition Head(Vector3 earRight)
        {
            var head = _rig.Keep(ScriptableObject.CreateInstance<HeadDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(head, "_id", "tete");
            TestWorld.Fill(head, "_mesh", _rig.Head.Mesh);
            TestWorld.Fill(head, "_earRight", earRight);
#endif
            return head;
        }

        // La pièce portée par le personnage habillé de cette tête.
        private Transform Worn(HeadDefinition head, AttachmentDefinition piece)
        {
            GameObject dressed = _rig.Keep(Dresser.Dress(new Appearance(head, _rig.Origin, _rig.Body, _rig.Origin,
                new[] { new WornAttachment(piece, _rig.Origin) }), null));
            return dressed.transform.Find("root/pelvis/head/" + piece.Model.name + "(Clone)");
        }
    }
}
