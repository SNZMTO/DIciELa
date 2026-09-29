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
    /// corps et les pièces détachées de la tête ont ensemble tous les triangles du modèle (le crâne en a en plus pour
    /// refermer les trous d'oreilles cousues : leur centre n'est pas un sommet du modèle ; ces oreilles, un sous-maillage de
    /// plus pour refermer leur dessous ; une paire d'oreilles est en deux moitiés). Chaque tête porte d'office les
    /// pièces que son modèle a (des oreilles pour six animaux, même cousues au crâne chez le raton laveur, des touffes pour
    /// quatre, la huppe de l'oiseau ; chacune garde son museau). Chargés à part, sans toucher la scène.
    /// </summary>
    public sealed class HeadCutTests
    {
        private const string Cuts = "Assets/Art/Personnages/";
        private static readonly string[] HeadBones = { "head", "eyes", "eyebrows" };

        // Ce que la tête de chaque animal porte d'office, par emplacement : savoir du pack.
        private static readonly Dictionary<string, string[]> Expected = new Dictionary<string, string[]>
        {
            { "chat", new[] { "oreilles" } },
            { "cochon", new[] { "oreilles", "cheveux" } },
            { "grenouille", new string[0] },
            { "lapin", new[] { "oreilles" } },
            { "oiseau", new[] { "cheveux" } },
            { "ratonlaveur", new[] { "oreilles", "cheveux" } },
            { "renard", new[] { "oreilles", "cheveux" } },
            { "souris", new[] { "oreilles", "cheveux" } }
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
                        .SelectMany(w => w.Model.GetComponentsInChildren<MeshFilter>(true))
                        .Sum(filter => FromSource(filter.sharedMesh, source.sharedMesh.subMeshCount));
                    if (FromModel(head.Mesh, source.sharedMesh) + body.Mesh.triangles.Length + parts !=
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
        public void EveryHeadWearsThePiecesItsPackModelHas()
        {
            Catalog catalog = ContentTests.LoadSingle<Catalog>();
            var faults = new List<string>();
            foreach (HeadDefinition head in catalog.Heads.Where(h => h != null))
            {
                string animal = head.Id.Substring(0, head.Id.LastIndexOf('_'));
                if (!Expected.TryGetValue(animal, out string[] expected))
                {
                    faults.Add(head.Id + " : animal inconnu du test.");
                    continue;
                }
                string worn = string.Join(", ", head.Worn.Select(w => w.Slot.Id).OrderBy(id => id));
                if (worn != string.Join(", ", expected.OrderBy(id => id)))
                    faults.Add(head.Id + " porte d'office « " + worn + " ».");
            }

            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        // Les indices des triangles de ce maillage de pièce venus du modèle : ceux de ses premiers sous-maillages, autant que
        // le modèle en a (le couvercle d'un dessous refermé en fait un de plus).
        private static int FromSource(Mesh mesh, int subMeshes)
        {
            int count = 0;
            for (int sub = 0; sub < Mathf.Min(subMeshes, mesh.subMeshCount); sub++)
                count += mesh.GetTriangles(sub).Length;
            return count;
        }

        // Les indices des triangles de ce maillage dont les trois coins sont à la place d'un sommet du modèle.
        private static int FromModel(Mesh mesh, Mesh model)
        {
            var places = new HashSet<Vector3>(model.vertices);
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            int count = 0;
            for (int t = 0; t < triangles.Length; t += 3)
            {
                if (places.Contains(vertices[triangles[t]]) && places.Contains(vertices[triangles[t + 1]]) &&
                    places.Contains(vertices[triangles[t + 2]]))
                    count += 3;
            }
            return count;
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
