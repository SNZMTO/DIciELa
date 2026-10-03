using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les arbres et les rochers du contenu, lus dans le projet sans rien écrire (GDD, section 2 ; décisions 157 à 160) :
    /// chaque sorte rend une marchandise du catalogue, que la Chimère porte en un voyage (selon son encombrement), et
    /// demande du travail ; le campement a, en plus de son stock de départ, la place de ce que rend chacune ; le poste « à tout faire » défriche,
    /// en dernier ; un modèle rangé ne l'est que dans une sorte, et chaque modèle peint sur le Terrain d'une carte est rangé dans une
    /// sorte : sinon, le jeu ne saurait pas ce qu'il est.
    /// </summary>
    public sealed class NatureContentTests
    {
        private const string TerrainFolder = "Assets/Terrain";

        [Test]
        public void EachSortYieldsACatalogGoodTheChimeraCarriesInOneTrip()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            Assert.IsNotEmpty(catalog.Nature, "Le catalogue n'a ni arbre ni rocher.");
            var faults = new List<string>();
            foreach (NatureDefinition nature in catalog.Nature)
            {
                GoodAmount yield = nature.Yield;
                if (!Contains(catalog.Goods, yield.Good))
                    faults.Add(nature.Id + " rend une marchandise hors du catalogue.");
                if (yield.Amount < 1 || (yield.Good != null && yield.Amount * yield.Good.Space > catalog.ChimeraKind.CarryCapacity))
                    faults.Add(nature.Id + " rend " + yield.Amount + " : la Chimère doit le porter en un voyage.");
                if (nature.WorkHours <= 0f)
                    faults.Add(nature.Id + " ne demande aucun travail.");
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        [Test]
        public void TheCampHasRoomForWhatEachSortYields()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            int starting = 0;
            foreach (GoodAmount goods in catalog.Opening.StartingGoods)
                starting += goods.Amount * goods.Good.Space;
            int room = ProjectContent.CampOf(catalog).StorageCapacity - starting;
            foreach (NatureDefinition nature in catalog.Nature)
                Assert.GreaterOrEqual(room, nature.Yield.Amount * nature.Yield.Good.Space, "Plein dès le départ, le campement n'aurait pas la place " +
                                                                   "de ce que rend " + nature.Id + " : rien ne se défricherait.");
        }

        [Test]
        public void TheOddJobsPostClearsLast()
        {
            StructureDefinition camp = ProjectContent.CampOf(ProjectContent.LoadSingle<Catalog>());
            IReadOnlyList<TaskKindDefinition> tasks = camp.WorkSlots[0].Tasks;
            Assert.AreEqual(TaskWork.Clear, tasks[tasks.Count - 1].Work, "Porter, bâtir, puis défricher (décision 139).");
        }

        [Test]
        public void EachPaintedModelBelongsToExactlyOneSort()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            var sortOf = new Dictionary<GameObject, NatureDefinition>();
            var faults = new List<string>();
            foreach (NatureDefinition nature in catalog.Nature)
            {
                foreach (GameObject model in nature.Models)
                {
                    if (model == null)
                        faults.Add(nature.Id + " a une case de modèle vide.");
                    else if (sortOf.TryGetValue(model, out NatureDefinition other) && other != nature)
                        faults.Add(model.name + " est rangé dans " + other.Id + " et dans " + nature.Id + ".");
                    else
                        sortOf[model] = nature;
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:TerrainData", new[] { TerrainFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
                foreach (TreePrototype prototype in data.treePrototypes)
                {
                    if (prototype.prefab == null)
                        faults.Add(path + " peint un arbre sans modèle.");
                    else if (!sortOf.ContainsKey(prototype.prefab))
                        faults.Add(prototype.prefab.name + ", peint sur " + path + ", n'est rangé dans aucune sorte : le " +
                                   "glisser dans la liste Modèles d'une fiche de Assets/Data/Nature (Arbre ou Rocher).");
                }
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
        }

        private static bool Contains(IReadOnlyList<GoodDefinition> goods, GoodDefinition good)
        {
            foreach (GoodDefinition each in goods)
            {
                if (each == good)
                    return true;
            }
            return false;
        }
    }
}
