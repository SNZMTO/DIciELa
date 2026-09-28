using Diciela.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu de la Chimère, lu dans le projet sans rien écrire : le catalogue mène à sa sorte, et son personnage
    /// vient du pack, tel quel. La vue la tourne vers où elle va en supposant que ce personnage regarde vers +z, l'origine
    /// à ses pieds : on le vérifie sur ses os, dans leur pose d'origine (les yeux devant la tête, les orteils devant les
    /// talons, les pieds au niveau de l'origine), chargés à part, sans toucher la scène ouverte.
    /// </summary>
    public sealed class ChimeraContentTests
    {
        [Test]
        public void TheCatalogLeadsToTheChimeraAndHerPackCharacter()
        {
            VillagerKindDefinition chimera = ContentTests.LoadSingle<Catalog>().ChimeraKind;

            Assert.IsNotNull(chimera, "Le catalogue ne mène pas à la sorte de la Chimère.");
            Assert.IsFalse(string.IsNullOrEmpty(chimera.Id), "La sorte de la Chimère n'a pas d'identifiant.");
            Assert.Greater(chimera.WalkMetersPerSecond, 0f);
            Assert.LessOrEqual(chimera.WanderPauseMinHours, chimera.WanderPauseMaxHours);
            Assert.IsNotNull(chimera.Model, "La Chimère n'a pas de personnage.");
            StringAssert.StartsWith("Assets/Synty/", AssetDatabase.GetAssetPath(chimera.Model));
        }

        [Test]
        public void HerCharacterLooksNorthFromItsFeet()
        {
            string path = AssetDatabase.GetAssetPath(ContentTests.LoadSingle<Catalog>().ChimeraKind.Model);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform head = Bone(root, "head");
                Transform eyes = Bone(root, "eyes");
                Transform heel = Bone(root, "foot_l");
                Transform toes = Bone(root, "ball_l");

                Assert.Greater(eyes.position.z - head.position.z, 0.1f, "Les yeux ne sont pas devant la tête.");
                Assert.Greater(toes.position.z, heel.position.z, "Les orteils ne sont pas devant le talon.");
                Assert.AreEqual(0f, toes.position.y, 0.1f, "Les pieds ne sont pas au niveau de l'origine.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform Bone(GameObject root, string name)
        {
            foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
            {
                if (bone.name == name)
                    return bone;
            }
            Assert.Fail("Le personnage n'a pas d'os « " + name + " ».");
            return null;
        }
    }
}
