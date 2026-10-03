using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les charrettes de l'étal du projet (GDD, sections 7 et 11 ; décisions 194 et 197), lues sans rien écrire : la
    /// charrette du catalogue, ses modèles du pack ; chaque construction qui tient des marchandises garde la sienne, et la
    /// Place a des places de charrette ; dans chaque poste, conduire vient juste après porter.
    /// </summary>
    public sealed class CartContentTests
    {
        [Test]
        public void EachWarehouseKeepsItsCartAndThePlaceCallsSome()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            Assert.IsNotNull(catalog.Cart, "Le catalogue n'a pas de charrette.");
            Assert.IsNotEmpty(catalog.Cart.Models, "La charrette n'a pas de modèle du pack : on ne la verrait pas.");
            CollectionAssert.AllItemsAreNotNull(catalog.Cart.Models, "Un modèle de la charrette est vide.");
            CollectionAssert.AllItemsAreUnique(catalog.Cart.Models, "Un modèle de la charrette y est deux fois.");

            var faults = new List<string>();
            int spots = 0;
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                int expected = structure.StorageCapacity > 0 ? 1 : 0;
                if (structure.Carts != expected)
                    faults.Add(structure.Id + " garde " + structure.Carts + " charrette(s) ; chaque entrepôt, le campement " +
                               "compris, a la sienne, et rien d'autre.");
                spots += structure.CartSpots;
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
            Assert.Greater(spots, 0, "Aucune construction n'a de place de charrette : la Place doit en avoir.");
        }

        [Test]
        public void InEachPostDrivingComesRightAfterCarrying()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            var faults = new List<string>();
            var posts = new HashSet<WorkSlotDefinition>();
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                foreach (WorkSlotDefinition post in structure.WorkSlots)
                    posts.Add(post);
            }
            foreach (WorkSlotDefinition post in posts)
            {
                IReadOnlyList<TaskKindDefinition> tasks = post.Tasks;
                for (int i = 0; i < tasks.Count; i++)
                {
                    bool carries = tasks[i].Work == TaskWork.Carry;
                    bool drivesNext = i + 1 < tasks.Count && tasks[i + 1].Work == TaskWork.Drive;
                    if (carries != drivesNext)
                        faults.Add(post.Id + " : conduire doit suivre porter, et seulement lui.");
                    if (tasks[i].Work == TaskWork.Drive && (i == 0 || tasks[i - 1].Work != TaskWork.Carry))
                        faults.Add(post.Id + " : conduire sans porter juste avant.");
                }
            }
            Assert.IsNotEmpty(posts, "Le catalogue n'a aucun poste.");
            Assert.IsEmpty(faults, string.Join("\n", faults));
        }
    }
}
