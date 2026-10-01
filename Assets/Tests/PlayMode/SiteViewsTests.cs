using System.Collections;
using Diciela.Data;
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
    /// copies des matériaux partent avec la vue. Un matériau qui détoure (des feuilles) détoure aussi sur le chantier et
    /// sur son fantôme, au même seuil, et dessine les mêmes faces : les vignes du pack gardent leurs feuilles.
    /// </summary>
    public sealed class SiteViewsTests
    {
        private static readonly int FillHeightId = Shader.PropertyToID("_FillHeight");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private const string VinesPath = "Assets/Synty/PolygonFantasyVillage/Materials/Env/Vines_01.mat";

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

        [Test]
        public void AClippingMaterialStaysClippedOnTheSiteAndOnItsGhost()
        {
            // Un Lit d'URP qui détoure au seuil de 0,4 et dessine ses deux faces, comme des feuilles.
            var texture = _test.Keep(new Texture2D(2, 2));
            var leaves = _test.Keep(new Material(Shader.Find("Universal Render Pipeline/Lit")));
            leaves.SetTexture("_BaseMap", texture);
            leaves.SetFloat("_AlphaClip", 1f);
            leaves.SetFloat("_Cutoff", 0.4f);
            leaves.SetFloat("_Cull", 0f);

            Transform view = PlaceASiteDressedIn(leaves);
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

        [Test]
        public void ThePackVinesKeepTheirLeavesOnTheSite()
        {
#if UNITY_EDITOR
            var vines = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(VinesPath);
            Assert.IsNotNull(vines, "Le pack doit avoir ses vignes (" + VinesPath + ").");

            Material built = PlaceASiteDressedIn(vines).GetComponent<MeshRenderer>().sharedMaterial;

            Assert.AreSame(vines.GetTexture("_Leaf_Texture"), built.GetTexture("_BaseMap"), "La texture des feuilles.");
            Assert.IsTrue(built.IsKeywordEnabled("_ALPHATEST_ON"), "Les feuilles sont détourées.");
            Assert.AreEqual(vines.GetFloat("_Alpha_Clip_Threshold"), built.GetFloat("_Cutoff"), 1e-5f);
            Assert.AreEqual(vines.GetFloat("_Cull"), built.GetFloat("_Cull"), "Les mêmes faces.");
#endif
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

        // Une haie de 2 × 2 m, dont le modèle est un quad vêtu de ce matériau, posée en (70 ; 70) : un chantier.
        private Transform PlaceASiteDressedIn(Material material)
        {
            GameObject model = _test.Keep(GameObject.CreatePrimitive(PrimitiveType.Quad));
            model.GetComponent<MeshRenderer>().sharedMaterial = material;
            var hedge = _test.Keep(ScriptableObject.CreateInstance<StructureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(hedge, "_id", "haie");
            TestWorld.Fill(hedge, "_widthMeters", 2f);
            TestWorld.Fill(hedge, "_depthMeters", 2f);
            TestWorld.Fill(hedge, "_model", model);
#endif
            _test.World.Submit(new PlaceStructureCommand(hedge, new Placement(new Vector2(70f, 70f), 0f)));
            _test.World.Step();
            return _test.Views.transform.GetChild(1);
        }
    }
}
