using System.Collections;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Le chantier qui monte, vu (GDD, section 9 ; décision 166), sur le petit monde des tests (le sol à 10 m) : une
    /// cabane posée, dont le modèle est un cube d'un mètre centré sur son origine, est sa vraie matière (le shader du
    /// chantier) avec son fantôme rangé dessous ; tant que rien n'est fait, la ligne est au pied du cube, à 9,5 m. Les
    /// copies des matériaux partent avec la vue.
    /// </summary>
    public sealed class SiteViewsTests
    {
        private static readonly int FillHeightId = Shader.PropertyToID("_FillHeight");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private TestWorld _test;

        [SetUp]
        public void CreateWorld()
        {
            _test = new TestWorld();
            _test.World.Submit(new PlaceStructureCommand(_test.Cabin, new Placement(new Vector2(60f, 60f), 0f)));
            _test.World.Step();
        }

        [TearDown]
        public void DestroyWorld()
        {
            _test.Dispose();
        }

        [Test]
        public void APlannedCabinIsItsRealMatterWithItsGhost()
        {
            Transform view = _test.Views.transform.GetChild(0);
            var built = view.GetComponent<MeshRenderer>();
            Transform ghost = view.Find("Fantôme");
            Assert.IsNotNull(ghost, "Le fantôme est rangé sous la cabane.");
            var ghostRenderer = ghost.GetComponent<MeshRenderer>();

            Assert.AreSame(_test.Settings.SiteMaterial.shader, built.sharedMaterial.shader, "La vraie matière.");
            Assert.AreSame(_test.Settings.SiteGhostMaterial, ghostRenderer.sharedMaterial, "Le fantôme.");
            Assert.AreEqual(ShadowCastingMode.Off, ghostRenderer.shadowCastingMode, "Le fantôme ne fait pas d'ombre.");
            Color original = _test.Cabin.Model.GetComponent<MeshRenderer>().sharedMaterial.GetColor(BaseColorId);
            Assert.AreEqual(original, built.sharedMaterial.GetColor(BaseColorId), "La copie garde la couleur de l'original.");
        }

        [Test]
        public void NothingDoneLeavesTheLineAtTheFootOfTheModel()
        {
            Transform view = _test.Views.transform.GetChild(0);
            var block = new MaterialPropertyBlock();

            view.GetComponent<MeshRenderer>().GetPropertyBlock(block);
            Assert.AreEqual(9.5f, block.GetFloat(FillHeightId), 1e-4f, "La vraie matière.");
            view.Find("Fantôme").GetComponent<MeshRenderer>().GetPropertyBlock(block);
            Assert.AreEqual(9.5f, block.GetFloat(FillHeightId), 1e-4f, "Le fantôme.");
            Assert.AreEqual(_test.Settings.PlannedColor, block.GetColor(BaseColorId), "La couleur du fantôme.");
        }

        [UnityTest]
        public IEnumerator HidingTheViewsDestroysTheCopiedMaterials()
        {
            Material copy = _test.Views.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreNotSame(_test.Settings.SiteMaterial, copy, "Une copie, pas la fiche.");

            _test.Views.Hide();
            yield return null;

            Assert.IsTrue(copy == null, "La copie est détruite.");
        }
    }
}
