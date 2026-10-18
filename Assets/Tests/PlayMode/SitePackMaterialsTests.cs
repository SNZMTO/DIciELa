using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les matériaux du pack sur un chantier et son fantôme (décisions 175 et 177), sur le petit monde des tests : les
    /// vignes gardent leurs feuilles, à leur seuil ; un seuil du pack autre que 0,5 passe à la vraie matière et au
    /// fantôme ; le lierre garde ses tiges, que la couleur de ses sommets distingue des feuilles ; le bord d'un chaume,
    /// transparent par mélange, se détoure au seuil des réglages, sur le chantier comme sur son fantôme ; un shader du pack
    /// qu'on ne connaît pas (le triplanaire) est dit dans la console. Les matériaux du pack sont lus dans le projet, sans
    /// rien y écrire : une copie en mémoire porte un seuil changé.
    /// </summary>
    public sealed class SitePackMaterialsTests
    {
        private const string Materials = "Assets/Synty/PolygonFantasyVillage/Materials/";
        private const string VinesPath = Materials + "Env/Vines_01.mat";
        private const string IvyPath = Materials + "Env/Vines_Creeping_01.mat";
        private const string ThatchEndsPath = Materials + "Buildings/Thatch_Ends_01.mat";
        private const string TriplanarRockPath = Materials + "Env/Rock_Grass_Triplanar_01.mat";

        private TestWorld _test;

        [SetUp]
        public void CreateWorld()
        {
            _test = new TestWorld();
        }

        [TearDown]
        public void DestroyWorld()
        {
            _test.Dispose();
        }

        [Test]
        public void ThePackVinesKeepTheirLeavesOnTheSite()
        {
            Material vines = Pack(VinesPath);

            Material built = Built(_test.PlaceASiteDressedIn(vines));

            Assert.AreSame(vines.GetTexture("_Leaf_Texture"), built.GetTexture("_BaseMap"), "La texture des feuilles.");
            Assert.IsTrue(built.IsKeywordEnabled("_ALPHATEST_ON"), "Les feuilles sont détourées.");
            Assert.AreEqual(vines.GetFloat("_Alpha_Clip_Threshold"), built.GetFloat("_Cutoff"), 1e-5f);
            Assert.AreEqual(vines.GetFloat("_Cull"), built.GetFloat("_Cull"), "Les mêmes faces.");
            Assert.IsFalse(built.IsKeywordEnabled("_TRUNKMAP"), "Ces vignes n'ont pas de tiges à lire.");
        }

        [Test]
        public void APackThresholdPassesToTheSiteAndItsGhost()
        {
            var thinner = _test.Keep(new Material(Pack(VinesPath)));
            thinner.SetFloat("_Alpha_Clip_Threshold", 0.3f);

            Transform view = _test.PlaceASiteDressedIn(thinner);

            Assert.AreEqual(0.3f, Built(view).GetFloat("_Cutoff"), 1e-5f, "La vraie matière détoure à son seuil.");
            Assert.AreEqual(0.3f, Ghost(view).GetFloat("_Cutoff"), 1e-5f, "Son fantôme aussi.");
        }

        [Test]
        public void ThePackIvyKeepsItsStemsOnTheSiteAndItsGhost()
        {
            Material ivy = Pack(IvyPath);
            Texture stems = ivy.GetTexture("_Trunk_Texture");
            Assert.IsNotNull(stems, "Le lierre du pack a la texture de ses tiges.");

            Transform view = _test.PlaceASiteDressedIn(ivy);

            Assert.AreSame(stems, Built(view).GetTexture("_TrunkMap"), "La vraie matière a la texture des tiges…");
            Assert.IsTrue(Built(view).IsKeywordEnabled("_TRUNKMAP"), "…et la lit là où le sommet dit une tige.");
            Assert.AreSame(stems, Ghost(view).GetTexture("_TrunkMap"), "Le fantôme a la forme des tiges.");
            Assert.IsTrue(Ghost(view).IsKeywordEnabled("_TRUNKMAP"));
        }

        [Test]
        public void AThatchEdgeBlendedInThePackIsClippedOnTheSite()
        {
            // Un autre seuil que celui des réglages par défaut, pour que la copie ne le trouve que dans la fiche ; les
            // vues se remontent pour le lire.
#if UNITY_EDITOR
            TestWorld.Fill(_test.Settings, "_blendedCutoff", 0.4f);
#endif
            _test.Views.Show(_test.World.Structures, _test.World.ConstructionSites, _test.World.Map.Relief);

            Transform view = _test.PlaceASiteDressedIn(Pack(ThatchEndsPath));

            Assert.IsTrue(Built(view).IsKeywordEnabled("_ALPHATEST_ON"), "La frange du chaume se détoure…");
            Assert.AreEqual(0.4f, Built(view).GetFloat("_Cutoff"), 1e-5f, "…au seuil des réglages.");
            Assert.AreNotSame(_test.Settings.SiteGhostMaterial, Ghost(view), "Son fantôme a sa forme.");
            Assert.IsTrue(Ghost(view).IsKeywordEnabled("_ALPHATEST_ON"));
            Assert.AreEqual(0.4f, Ghost(view).GetFloat("_Cutoff"), 1e-5f);
        }

        [Test]
        public void AnUnknownPackShaderIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Rock_Grass_Triplanar_01.*n'est pas reconnu sur un chantier"));

            _test.PlaceASiteDressedIn(Pack(TriplanarRockPath));
        }

        private static Material Pack(string path)
        {
#if UNITY_EDITOR
            // Sans le pack (une copie publique du dépôt), le test est passé, pas échoué ; avec lui, un matériau manquant échoue.
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Synty"))
            {
                Assert.Ignore("Le pack n'est pas importé : ce test lit ses matériaux.");
            }
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            Assert.IsNotNull(material, "Le pack doit avoir ce matériau (" + path + ").");
            return material;
#else
            throw new System.NotSupportedException("Les matériaux du pack se lisent dans l'éditeur.");
#endif
        }

        private static Material Built(Transform view)
        {
            return view.GetComponent<MeshRenderer>().sharedMaterial;
        }

        private static Material Ghost(Transform view)
        {
            return view.Find("Fantôme").GetComponent<MeshRenderer>().sharedMaterial;
        }
    }
}
