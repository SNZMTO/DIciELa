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
    /// l'écran de création propose, une seule fois ; les modèles et les couleurs viennent du pack, les têtes et les corps
    /// de leur découpe (Assets/Art/Personnages). Chaque personnage d'un corps montre un seul corps, dans sa couleur
    /// d'origine, a l'os de chaque emplacement et regarde vers +z depuis ses pieds ; tous ont le même squelette. Chaque
    /// tête et chaque corps sont son modèle coupé au cou : la tête ne garde que des sommets menés par les os de la tête,
    /// le corps aucun, et les deux ensemble ont tous les triangles du modèle. Chargés à part, sans toucher la scène.
    /// </summary>
    public sealed class ChimeraContentTests
    {
        private const string Pack = "Assets/Synty/";
        private const string Cuts = "Assets/Art/Personnages/";
        private static readonly string[] HeadBones = { "head", "eyes", "eyebrows" };

        [Test]
        public void TheCatalogLeadsToTheChimeraAndToEverythingHerScreenOffersOnce()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            VillagerKindDefinition chimera = catalog.ChimeraKind;
            Assert.IsNotNull(chimera, "Le catalogue ne mène pas à la sorte de la Chimère.");
            Assert.IsFalse(string.IsNullOrEmpty(chimera.Id), "La sorte de la Chimère n'a pas d'identifiant.");
            Assert.Greater(chimera.WalkMetersPerSecond, 0f);
            Assert.IsNotEmpty(catalog.Heads, "Le catalogue ne propose aucune tête.");
            Assert.IsNotEmpty(catalog.Bodies, "Le catalogue ne propose aucun corps.");

            var faults = new List<string>();
            Check(catalog.Palettes, p => p.Id, p => From(p.CharacterMaterial, Pack) + From(p.ObjectMaterial, Pack), faults);
            Check(catalog.Heads, h => h.Id, h => From(h.Mesh, Cuts) + In(catalog.Palettes, h.Palette), faults);
            Check(catalog.Bodies, b => b.Id, b => From(b.Model, Pack) + From(b.Mesh, Cuts) +
                                                  In(catalog.Palettes, b.Palette) +
                                                  string.Concat(b.Worn.Select(w => In(catalog.Attachments, w))), faults);
            Check(catalog.AttachmentSlots, s => s.Id, s => string.IsNullOrEmpty(s.Bone) ? " sans os ;" : "", faults);
            Check(catalog.Attachments, a => a.Id, a => From(a.Model, Pack) + In(catalog.AttachmentSlots, a.Slot) +
                                                       In(catalog.Palettes, a.Palette) +
                                                       (a.Anchor == Vector3.zero ? " sans point d'accroche ;" : ""),
                faults);

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        [Test]
        public void EveryBodyShowsOneBodyInItsOwnColorOnTheSameSkeletonWithEverySlotBoneLookingNorth()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
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

        [Test]
        public void EveryHeadAndBodyIsTheirPackModelCutAtTheNeck()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            var faults = new List<string>();
            foreach (BodyDefinition body in catalog.Bodies)
            {
                HeadDefinition head = catalog.Heads.FirstOrDefault(h => h != null && body != null && h.Id == body.Id);
                if (head == null || head.Mesh == null || body.Mesh == null || body.Model == null)
                {
                    faults.Add((body != null ? body.Id : "?") + " : pas de tête du même animal, ou pas de découpe.");
                    continue;
                }
                GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(body.Model));
                try
                {
                    SkinnedMeshRenderer source = Shown(root)[0];
                    string[] bones = source.bones.Select(b => b.name).ToArray();
                    if (head.Mesh.triangles.Length + body.Mesh.triangles.Length != source.sharedMesh.triangles.Length)
                        faults.Add(body.Id + " : la tête et le corps n'ont pas ensemble les triangles du modèle.");
                    if (head.Mesh.boneWeights.Any(w => HeadShare(w, bones) < 0.5f))
                        faults.Add(body.Id + " : la tête garde des sommets menés par d'autres os que ceux de la tête.");
                    if (body.Mesh.boneWeights.Any(w => HeadShare(w, bones) >= 0.5f))
                        faults.Add(body.Id + " : le corps garde des sommets de la tête.");
                    if (head.Mesh.bindposes.Length != bones.Length || body.Mesh.bindposes.Length != bones.Length)
                        faults.Add(body.Id + " : la découpe n'a pas les poses d'origine des os.");
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

        private static float HeadShare(BoneWeight w, string[] bones)
        {
            return (HeadBones.Contains(bones[w.boneIndex0]) ? w.weight0 : 0f) +
                   (HeadBones.Contains(bones[w.boneIndex1]) ? w.weight1 : 0f) +
                   (HeadBones.Contains(bones[w.boneIndex2]) ? w.weight2 : 0f) +
                   (HeadBones.Contains(bones[w.boneIndex3]) ? w.weight3 : 0f);
        }

        private static Transform Bone(GameObject root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(bone => bone.name == name);
        }
    }
}
