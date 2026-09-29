using System.Collections.Generic;
using System.Linq;
using Diciela.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// La découpe des animaux du pack (SAVOIR, 2.11), lue dans le projet sans rien écrire : chaque silhouette est son
    /// modèle coupé au cou, le crâne ne garde que des sommets menés par les os de la tête, le corps aucun, et le crâne, le
    /// corps et les pièces détachées de la tête ont ensemble tous les triangles du modèle. Chaque tête porte d'office les
    /// pièces que son modèle a (des oreilles détachables pour cinq animaux, des touffes pour quatre, le museau du renard
    /// seul, la huppe de l'oiseau) ; le raton laveur tient ses oreilles soudées. Chargés à part, sans toucher la scène.
    /// </summary>
    public sealed class HeadCutTests
    {
        private const string Cuts = "Assets/Art/Personnages/";
        private static readonly string[] HeadBones = { "head", "eyes", "eyebrows" };

        // Ce que la tête de chaque animal porte d'office, par emplacement, et ce que son crâne tient soudé : savoir du pack.
        private static readonly Dictionary<string, (string[] Worn, string[] Fused)> Expected =
            new Dictionary<string, (string[], string[])>
            {
                { "chat", (new[] { "oreilles" }, new string[0]) },
                { "cochon", (new[] { "oreilles", "cheveux" }, new string[0]) },
                { "grenouille", (new string[0], new string[0]) },
                { "lapin", (new[] { "oreilles" }, new string[0]) },
                { "oiseau", (new[] { "cheveux" }, new string[0]) },
                { "ratonlaveur", (new[] { "cheveux" }, new[] { "oreilles" }) },
                { "renard", (new[] { "oreilles", "cheveux", "museau" }, new string[0]) },
                { "souris", (new[] { "oreilles", "cheveux" }, new string[0]) }
            };

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
                    SkinnedMeshRenderer source = root.transform.Cast<Transform>()
                        .Select(child => child.GetComponent<SkinnedMeshRenderer>())
                        .First(skin => skin != null && skin.gameObject.activeSelf);
                    string[] bones = source.bones.Select(b => b.name).ToArray();
                    int parts = head.Worn.Where(w => w != null && AssetDatabase.GetAssetPath(w.Model).StartsWith(Cuts))
                        .Sum(w => w.Model.GetComponent<MeshFilter>().sharedMesh.triangles.Length);
                    if (head.Mesh.triangles.Length + body.Mesh.triangles.Length + parts !=
                        source.sharedMesh.triangles.Length)
                        faults.Add(body.Id + " : le crâne, le corps et les pièces de la tête n'ont pas ensemble les " +
                                   "triangles du modèle.");
                    if (head.Mesh.boneWeights.Any(w => HeadShare(w, bones) < 0.5f))
                        faults.Add(body.Id + " : le crâne garde des sommets menés par d'autres os que ceux de la tête.");
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

        [Test]
        public void EveryHeadWearsThePiecesItsPackModelHasAndHoldsItsFusedEars()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            var faults = new List<string>();
            foreach (HeadDefinition head in catalog.Heads.Where(h => h != null))
            {
                string animal = head.Id.Substring(0, head.Id.LastIndexOf('_'));
                if (!Expected.TryGetValue(animal, out (string[] Worn, string[] Fused) expected))
                {
                    faults.Add(head.Id + " : animal inconnu du test.");
                    continue;
                }
                string worn = string.Join(", ", head.Worn.Select(w => w.Slot.Id).OrderBy(id => id));
                string fused = string.Join(", ", head.Fused.Select(s => s.Id).OrderBy(id => id));
                if (worn != string.Join(", ", expected.Worn.OrderBy(id => id)))
                    faults.Add(head.Id + " porte d'office « " + worn + " ».");
                if (fused != string.Join(", ", expected.Fused.OrderBy(id => id)))
                    faults.Add(head.Id + " tient soudés « " + fused + " ».");
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static float HeadShare(BoneWeight w, string[] bones)
        {
            return (HeadBones.Contains(bones[w.boneIndex0]) ? w.weight0 : 0f) +
                   (HeadBones.Contains(bones[w.boneIndex1]) ? w.weight1 : 0f) +
                   (HeadBones.Contains(bones[w.boneIndex2]) ? w.weight2 : 0f) +
                   (HeadBones.Contains(bones[w.boneIndex3]) ? w.weight3 : 0f);
        }
    }
}
