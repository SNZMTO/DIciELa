using System;
using System.Collections.Generic;
using System.Linq;
using Diciela.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu de la Chimère, lu dans le projet sans rien écrire : le catalogue mène à sa sorte et à tout ce que
    /// l'écran de création propose, une seule fois ; les couleurs viennent du pack, les têtes et les corps de leur découpe
    /// (Assets/Art/Personnages), les pièces du pack ou de la découpe des têtes ; chaque pièce a sa taille, et celle d'un
    /// emplacement à contact, sa face qui touche ; chaque tête a ses points et ses surfaces mesurés. Chaque personnage d'un
    /// corps montre un seul corps, dans sa couleur d'origine, a l'os de chaque emplacement et regarde vers +z depuis ses
    /// pieds ; tous ont le même squelette. La découpe elle-même : HeadCutTests. Chargés à part, sans toucher la scène.
    /// </summary>
    public sealed class ChimeraContentTests
    {
        private const string Pack = "Assets/Synty/";
        private const string Cuts = "Assets/Art/Personnages/";

        /// <summary>Sans le pack ou sans sa découpe (une copie publique du dépôt), ces tests n'ont rien à lire : ils sont
        /// passés, pas échoués. Dans le dépôt de travail, les deux sont là et les tests tournent.</summary>
        [SetUp]
        public void RequireThePackAndItsCut()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Synty") || !AssetDatabase.IsValidFolder("Assets/Art/Personnages"))
            {
                Assert.Ignore("Le pack n'est pas importé, ou sa découpe n'est pas faite : ce test lit leurs fichiers.");
            }
        }

        [Test]
        public void TheCatalogLeadsToTheChimeraAndToEverythingHerScreenOffersOnce()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            VillagerKindDefinition chimera = catalog.ChimeraKind;
            Assert.IsNotNull(chimera, "Le catalogue ne mène pas à la sorte de la Chimère.");
            Assert.IsFalse(string.IsNullOrEmpty(chimera.Id), "La sorte de la Chimère n'a pas d'identifiant.");
            Assert.Greater(chimera.WalkMetersPerSecond, 0f);
            Assert.IsNotEmpty(catalog.Heads, "Le catalogue ne propose aucune tête.");
            Assert.IsNotEmpty(catalog.Bodies, "Le catalogue ne propose aucun corps.");

            var faults = new List<string>();
            Check(catalog.Palettes, p => p.Id, p => From(p.CharacterMaterial, Pack) + From(p.ObjectMaterial, Pack), faults);
            Check(catalog.Heads, h => h.Id, h => From(h.Mesh, Cuts) + In(catalog.Palettes, h.Palette) +
                                                 string.Concat(h.Worn.Select(w => In(catalog.Attachments, w))) +
                                                 Measured(h), faults);
            Check(catalog.Bodies, b => b.Id, b => From(b.Model, Pack) + From(b.Mesh, Cuts) +
                                                  In(catalog.Palettes, b.Palette) +
                                                  string.Concat(b.Worn.Select(w => In(catalog.Attachments, w))), faults);
            Check(catalog.AttachmentSlots, s => s.Id, s => string.IsNullOrEmpty(s.Bone) ? " sans os ;" : "", faults);
            Check(catalog.Attachments, a => a.Id, a => (From(a.Model, Pack) == "" ? "" : From(a.Model, Cuts)) +
                                                       In(catalog.AttachmentSlots, a.Slot) +
                                                       In(catalog.Palettes, a.Palette) +
                                                       (a.HasScale ? "" : " sans taille ;") +
                                                       (a.Slot == null || a.Slot.Contact == ContactAxis.None ||
                                                        !a.Shape.IsEmpty ? "" : " sans sa face qui touche ;"),
                faults);

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        [Test]
        public void EveryBodyShowsOneBodyInItsOwnColorOnTheSameSkeletonWithEverySlotBoneLookingNorth()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            var faults = new List<string>();
            string[] skeleton = null;
            foreach (BodyDefinition body in catalog.Bodies)
            {
                // Un corps sans personnage ou sans couleur est dit par le test du catalogue.
                if (body == null || body.Model == null || body.Palette == null)
                    continue;
                GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(body.Model));
                try
                {
                    SkinnedMeshRenderer[] shown = Shown(root);
                    if (shown.Length != 1 || shown[0].sharedMaterial != body.Palette.CharacterMaterial)
                        faults.Add(body.Id + " : " + shown.Length + " corps allumé(s), ou pas dans sa couleur d'origine.");
                    string[] bones = shown.Length > 0 ? shown[0].bones.Select(b => b.name).ToArray() : new string[0];
                    skeleton = skeleton ?? bones;
                    if (!bones.SequenceEqual(skeleton))
                        faults.Add(body.Id + " n'a pas le squelette des autres.");
                    foreach (AttachmentSlotDefinition slot in catalog.AttachmentSlots)
                    {
                        if (Bone(root, slot.Bone) == null)
                            faults.Add(body.Id + " n'a pas d'os « " + slot.Bone + " » (" + slot.Id + ").");
                    }
                    Transform head = Bone(root, "head"), eyes = Bone(root, "eyes");
                    Transform heel = Bone(root, "foot_l"), toes = Bone(root, "ball_l");
                    if (head == null || eyes == null || heel == null || toes == null ||
                        eyes.position.z - head.position.z <= 0.1f || toes.position.z <= heel.position.z ||
                        Mathf.Abs(toes.position.y) > 0.1f)
                        faults.Add(body.Id + " ne regarde pas vers +z depuis ses pieds.");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        /// <summary>
        /// Une liste du catalogue : aucune fiche vide, des identifiants non vides et uniques, rien à redire à chacune ; et
        /// chaque fiche de cette sorte dans le projet y est.
        /// </summary>
        private static void Check<T>(IReadOnlyList<T> listed, Func<T, string> id, Func<T, string> fault,
            List<string> faults) where T : ScriptableObject
        {
            var ids = new HashSet<string>();
            var reached = new HashSet<T>();
            foreach (T fiche in listed)
            {
                if (fiche == null || !reached.Add(fiche))
                {
                    faults.Add("Une liste de " + typeof(T).Name + " du catalogue a une fiche vide, ou en double.");
                    continue;
                }
                if (string.IsNullOrEmpty(id(fiche)) || !ids.Add(id(fiche)))
                    faults.Add(fiche.name + " : identifiant vide, ou déjà pris.");
                string more = fault(fiche);
                if (more.Length > 0)
                    faults.Add(fiche.name + " :" + more);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!reached.Contains(AssetDatabase.LoadAssetAtPath<T>(path)))
                    faults.Add(path + " n'est dans aucune liste du catalogue.");
            }
        }

        /// <summary>Ses points et ses surfaces : mesurés au-dessus des pieds, pas vides.</summary>
        private static string Measured(HeadDefinition head)
        {
            bool points = new[] { AttachmentFrame.Crown, AttachmentFrame.Face, AttachmentFrame.Ears, AttachmentFrame.Neck }
                .All(frame => head.PointOf(frame).y > 0.5f);
            bool surfaces = !head.SurfaceOf(ContactAxis.Down).IsEmpty && !head.SurfaceOf(ContactAxis.Back).IsEmpty;
            return points && surfaces ? "" : " sans ses points ou ses surfaces ;";
        }

        private static string From(Object asset, string folder)
        {
            return asset != null && AssetDatabase.GetAssetPath(asset).StartsWith(folder) ? "" : " pas de " + folder + " ;";
        }

        private static string In<T>(IReadOnlyList<T> list, T fiche) where T : Object
        {
            return fiche != null && list.Contains(fiche) ? "" : " une fiche hors du catalogue ;";
        }

        private static SkinnedMeshRenderer[] Shown(GameObject root)
        {
            return root.transform.Cast<Transform>().Select(child => child.GetComponent<SkinnedMeshRenderer>())
                .Where(skin => skin != null && skin.gameObject.activeSelf).ToArray();
        }

        private static Transform Bone(GameObject root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(bone => bone.name == name);
        }
    }
}
