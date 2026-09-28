using System;
using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Le contenu de la Chimère, lu dans le projet sans rien écrire : le catalogue mène à sa sorte et à tout ce que
    /// l'écran de création propose, une seule fois, et tout vient du pack, tel quel. Chaque corps montre un seul corps
    /// allumé, dans sa couleur d'origine ; il a l'os de chaque emplacement ; il regarde vers +z, l'origine à ses pieds (la
    /// vue le tourne vers où il va en le supposant), vérifié sur ses os dans leur pose d'origine (les yeux devant la
    /// tête, les orteils devant les talons, les pieds au niveau de l'origine), chargé à part, sans toucher la scène
    /// ouverte.
    /// </summary>
    public sealed class ChimeraContentTests
    {
        private const string Pack = "Assets/Synty/";

        [Test]
        public void TheCatalogLeadsToTheChimeraAndToEverythingHerScreenOffersOnce()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            VillagerKindDefinition chimera = catalog.ChimeraKind;
            Assert.IsNotNull(chimera, "Le catalogue ne mène pas à la sorte de la Chimère.");
            Assert.IsFalse(string.IsNullOrEmpty(chimera.Id), "La sorte de la Chimère n'a pas d'identifiant.");
            Assert.Greater(chimera.WalkMetersPerSecond, 0f);
            Assert.LessOrEqual(chimera.WanderPauseMinHours, chimera.WanderPauseMaxHours);
            Assert.IsNotEmpty(catalog.Bodies, "Le catalogue ne propose aucun corps.");

            var faults = new List<string>();
            Check(catalog.Palettes, p => p.Id, p => p.Material, p => "", faults);
            Check(catalog.Bodies, b => b.Id, b => b.Model,
                b => Contains(catalog.Palettes, b.Palette) ? "" : " : sa couleur d'origine n'est pas au catalogue", faults);
            Check(catalog.AttachmentSlots, s => s.Id, null,
                s => string.IsNullOrEmpty(s.Bone) ? " : sans os" : "", faults);
            Check(catalog.Attachments, a => a.Id, a => a.Model,
                a => !Contains(catalog.AttachmentSlots, a.Slot) ? " : son emplacement n'est pas au catalogue"
                    : a.Anchor == Vector3.zero ? " : sans point d'accroche" : "", faults);

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        [Test]
        public void EveryBodyShowsOneBodyInItsOwnColorWithEverySlotBoneAndLooksNorthFromItsFeet()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            var faults = new List<string>();
            foreach (BodyDefinition body in catalog.Bodies)
            {
                // Un corps sans personnage ou sans couleur est dit par le test du catalogue.
                if (body == null || body.Model == null || body.Palette == null)
                    continue;
                GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(body.Model));
                try
                {
                    int shown = 0;
                    foreach (Transform child in root.transform)
                    {
                        var skin = child.GetComponent<SkinnedMeshRenderer>();
                        if (skin != null && child.gameObject.activeSelf && skin.sharedMaterial == body.Palette.Material)
                            shown++;
                    }
                    if (shown != 1)
                        faults.Add(body.Id + " : " + shown + " corps allumé(s) dans sa couleur d'origine, au lieu d'un.");

                    foreach (AttachmentSlotDefinition slot in catalog.AttachmentSlots)
                    {
                        if (Bone(root, slot.Bone) == null)
                            faults.Add(body.Id + " n'a pas d'os « " + slot.Bone + " » (" + slot.Id + ").");
                    }

                    Transform head = Bone(root, "head");
                    Transform eyes = Bone(root, "eyes");
                    Transform heel = Bone(root, "foot_l");
                    Transform toes = Bone(root, "ball_l");
                    if (head == null || eyes == null || heel == null || toes == null)
                        faults.Add(body.Id + " n'a pas les os de la tête, des yeux et du pied gauche.");
                    else if (eyes.position.z - head.position.z <= 0.1f || toes.position.z <= heel.position.z ||
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
        /// Une liste du catalogue : aucune fiche vide, des identifiants non vides et uniques, un modèle (ou un matériau)
        /// du pack, rien de plus à redire ; et chaque fiche de cette sorte dans le projet y est.
        /// </summary>
        private static void Check<T>(IReadOnlyList<T> listed, Func<T, string> id, Func<T, Object> packAsset,
            Func<T, string> fault, List<string> faults) where T : ScriptableObject
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
                if (packAsset != null && (packAsset(fiche) == null ||
                                          !AssetDatabase.GetAssetPath(packAsset(fiche)).StartsWith(Pack)))
                    faults.Add(fiche.name + " : son modèle ne vient pas du pack.");
                string more = fault(fiche);
                if (more.Length > 0)
                    faults.Add(fiche.name + more + ".");
            }

            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!reached.Contains(AssetDatabase.LoadAssetAtPath<T>(path)))
                    faults.Add(path + " n'est dans aucune liste du catalogue.");
            }
        }

        private static bool Contains<T>(IReadOnlyList<T> list, T fiche) where T : Object
        {
            foreach (T listed in list)
            {
                if (listed != null && listed == fiche)
                    return true;
            }
            return false;
        }

        private static Transform Bone(GameObject root, string name)
        {
            foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
            {
                if (bone.name == name)
                    return bone;
            }
            return null;
        }
    }
}
