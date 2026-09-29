using Diciela.Data;
using Diciela.Simulation.Society;
using Diciela.View;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// La pose d'une pièce (SAVOIR, 2.11), sur le personnage de TestRig habillé comme en jeu : chaque moitié d'une paire
    /// d'oreilles suit l'oreille de son côté, d'après l'écart entre les oreilles de la tête choisie
    /// et celles de sa tête de référence ; ailleurs que sur celle-ci, une paire se pose par sa base ; une pièce tourne sur
    /// son pivot de sa rotation.
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
        public void EachHalfOfAPairFollowsItsEar()
        {
            HeadDefinition wide = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            HeadDefinition narrow = _rig.HeadWithEars(new Vector3(0.35f, 1.8f, 0f));
            AttachmentDefinition ears = Pair("Oreilles de test", ContactAxis.None, Vector3.zero);
#if UNITY_EDITOR
            TestWorld.Fill(ears, "_reference", wide);
#endif
            Transform onWide = _rig.Worn(wide, ears), onNarrow = _rig.Worn(narrow, ears);

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
            HeadDefinition reference = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            HeadDefinition gentle = _rig.HeadWithEars(new Vector3(0.4f, 1.7f, 0f));
            HeadDefinition steep = _rig.HeadWithEars(new Vector3(0.4f, 1.7f, 0f));
            AttachmentDefinition ears = Pair("Oreilles posées", ContactAxis.Down, new Vector3(0f, -0.02f, 0f));
#if UNITY_EDITOR
            TestWorld.Fill(reference, "_top", Row(x => 1.8f));
            TestWorld.Fill(gentle, "_top", Row(x => Mathf.Abs(x) < 0.3f ? 1.6f : 1.58f));
            TestWorld.Fill(steep, "_top", Row(x => Mathf.Abs(x) < 0.3f ? 1.6f : 1.5f));
            TestWorld.Fill(ears, "_shape", under);
            TestWorld.Fill(ears, "_reference", reference);
#endif
            Assert.AreEqual(1.8f + 0.1f - 0.02f, _rig.Worn(reference, ears).position.y, 1e-3f, "Sur sa tête, posée.");
            Assert.AreEqual(1.58f + 0.1f - 0.02f, _rig.Worn(gentle, ears).position.y, 1e-3f,
                "Ailleurs, par sa base : le dehors touche (enfoncé de même), le dedans s'enfonce ; rien ne flotte.");
            Transform onSteep = _rig.Worn(steep, ears);
            Assert.AreEqual(1.6f + 0.1f - 0.02f, onSteep.position.y, 1e-3f,
                "Sur un bord trop penché, elle glisse vers le milieu et porte à plat sur le plateau.");
            Assert.That(onSteep.Find("Droite").position.x, Is.InRange(0.035f, 0.065f), "De 5 cm environ, de chaque côté.");
        }

        [Test]
        public void APieceTurnsByItsRotationAroundItsPivot()
        {
            HeadDefinition head = _rig.HeadWithEars(new Vector3(0.4f, 1.8f, 0f));
            AttachmentDefinition earring = _rig.Piece("Boucle de test", AttachmentFrame.Ears, ContactAxis.None,
                new Vector3(0.4f, 0f, 0f), 1f, null);
#if UNITY_EDITOR
            TestWorld.Fill(earring, "_rotation", new Vector3(0f, 0f, 90f));
#endif
            Transform worn = _rig.Worn(head, earring);

            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 0f, 90f), worn.rotation), Is.LessThan(0.01f));
            Assert.That(Vector3.Distance(new Vector3(0.4f, 1.8f, 0f), worn.position), Is.LessThan(1e-4f),
                "Elle tourne sur son pivot : il ne bouge pas.");
        }

        // Une paire d'oreilles du banc : deux moitiés, à 10 cm de part et d'autre de son pivot.
        private AttachmentDefinition Pair(string name, ContactAxis contact, Vector3 offset)
        {
            AttachmentDefinition pair = _rig.Piece(name, AttachmentFrame.Ears, contact, offset, 1f, null);
#if UNITY_EDITOR
            TestWorld.Fill(pair.Slot, "_pairs", true);
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
    }
}
