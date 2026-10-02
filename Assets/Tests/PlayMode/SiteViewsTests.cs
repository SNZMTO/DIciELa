using System;
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
    /// chantier) avec son fantôme rangé dessous ; tant que rien n'est fait, la ligne est au pied du cube, à 9,5 m. Un
    /// matériau qui détoure (des feuilles) détoure aussi sur le chantier et sur son fantôme, au même seuil, et dessine les
    /// mêmes faces. La ligne est la valeur propre de chaque rendu, sans bloc de propriétés (le SRP Batcher les regroupe),
    /// et le fantôme porte sa teinte dans son matériau. Les copies des matériaux, vraie matière et fantôme, partent avec
    /// la vue. Les matériaux du pack ont leurs tests à part (SitePackMaterialsTests).
    /// </summary>
    public sealed class SiteViewsTests
    {
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
            Assert.AreSame(_test.Settings.SiteGhostMaterial.shader, ghostRenderer.sharedMaterial.shader, "Le fantôme.");
            Assert.AreEqual(ShadowCastingMode.Off, ghostRenderer.shadowCastingMode, "Le fantôme ne fait pas d'ombre.");
            Color original = _test.Cabin.Model.GetComponent<MeshRenderer>().sharedMaterial.GetColor(BaseColorId);
            Assert.AreEqual(original, built.sharedMaterial.GetColor(BaseColorId), "La copie garde la couleur de l'original.");
        }

        [Test]
        public void NothingDoneLeavesTheLineAtTheFootOfTheModel()
        {
            Transform view = _test.Views.transform.GetChild(0);
            var built = view.GetComponent<MeshRenderer>();
            var ghost = view.Find("Fantôme").GetComponent<MeshRenderer>();

            Assert.AreEqual(9.5f, LineOf(built), 1e-4f, "La vraie matière.");
            Assert.AreEqual(9.5f, LineOf(ghost), 1e-4f, "Le fantôme.");
            Assert.AreEqual(_test.Settings.PlannedColor, ghost.sharedMaterial.GetColor(BaseColorId), "Sa teinte.");
            Assert.IsFalse(built.HasPropertyBlock(), "Aucun bloc de propriétés sur la vraie matière…");
            Assert.IsFalse(ghost.HasPropertyBlock(), "…ni sur le fantôme : le SRP Batcher les regroupe.");
        }

        [Test]
        public void AClippingMaterialStaysClippedOnTheSiteAndOnItsGhost()
        {
            var texture = _test.Keep(new Texture2D(2, 2));
            Material leaves = Leaves(texture);

            Transform view = _test.PlaceASiteDressedIn(leaves);
            Material built = view.GetComponent<MeshRenderer>().sharedMaterial;
            Material ghost = view.Find("Fantôme").GetComponent<MeshRenderer>().sharedMaterial;

            Assert.IsTrue(built.IsKeywordEnabled("_ALPHATEST_ON"), "La vraie matière détoure.");
            Assert.AreEqual(0.4f, built.GetFloat("_Cutoff"), 1e-5f);
            Assert.AreEqual(0f, built.GetFloat("_Cull"), "Elle dessine ses deux faces.");
            Assert.AreSame(texture, built.GetTexture("_BaseMap"));
            Assert.AreNotSame(_test.Settings.SiteGhostMaterial, ghost, "Son fantôme a sa copie.");
            Assert.IsTrue(ghost.IsKeywordEnabled("_ALPHATEST_ON"), "Le fantôme détoure : il a la forme des feuilles.");
            Assert.AreEqual(0.4f, ghost.GetFloat("_Cutoff"), 1e-5f);
            Assert.AreEqual(0f, ghost.GetFloat("_Cull"));
            Assert.AreSame(texture, ghost.GetTexture("_BaseMap"));
            Material cube = _test.Views.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
            Assert.IsFalse(cube.IsKeywordEnabled("_ALPHATEST_ON"), "Le cube de la cabane ne détoure pas.");
        }

        [UnityTest]
        public IEnumerator HidingTheViewsDestroysTheCopiedMaterials()
        {
            Transform cabin = _test.Views.transform.GetChild(0);
            Material copy = cabin.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreNotSame(_test.Settings.SiteMaterial, copy, "Une copie, pas la fiche.");
            Material tinted = cabin.Find("Fantôme").GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreNotSame(_test.Settings.SiteGhostMaterial, tinted, "Le fantôme teinté, une copie lui aussi.");
            Transform hedge = _test.PlaceASiteDressedIn(Leaves(_test.Keep(new Texture2D(2, 2))));
            Material ghostCopy = hedge.Find("Fantôme").GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreNotSame(_test.Settings.SiteGhostMaterial, ghostCopy, "Une copie du fantôme.");

            _test.Views.Hide();
            yield return null;

            Assert.IsTrue(copy == null, "La copie de la vraie matière est détruite.");
            Assert.IsTrue(ghostCopy == null, "La copie du fantôme aussi.");
            Assert.IsTrue(tinted == null, "Et le fantôme teinté.");
        }

        // La hauteur de la ligne de ce rendu : la valeur propre que lisent les shaders du chantier, en bits de flottant.
        private static float LineOf(MeshRenderer renderer)
        {
            return BitConverter.Int32BitsToSingle(unchecked((int)renderer.GetShaderUserValue()));
        }

        // Un Lit d'URP qui détoure au seuil de 0,4 et dessine ses deux faces, comme des feuilles.
        private Material Leaves(Texture texture)
        {
            var leaves = _test.Keep(new Material(Shader.Find("Universal Render Pipeline/Lit")));
            leaves.SetTexture("_BaseMap", texture);
            leaves.SetFloat("_AlphaClip", 1f);
            leaves.SetFloat("_Cutoff", 0.4f);
            leaves.SetFloat("_Cull", 0f);
            return leaves;
        }
    }
}
