using System.Collections.Generic;
using Diciela.Data;
using NUnit.Framework;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les charrettes de l'étal du projet (GDD, sections 7 et 11 ; décisions 194, 197 et 204), lues sans rien écrire : la
    /// charrette du catalogue, ses modèles du pack, qui tiennent chacun dans une place de charrette, la plus grande la
    /// remplissant ; chaque construction qui tient des marchandises garde la sienne, et la Place a des places de
    /// charrette ; dans chaque poste, conduire vient juste après porter.
    /// </summary>
    public sealed class CartContentTests
    {
        [Test]
        public void EachWarehouseKeepsItsCartAndThePlazaCallsSome()
        {
            Catalog catalog = ProjectContent.LoadSingle<Catalog>();
            Assert.IsNotNull(catalog.Cart, "Le catalogue n'a pas de charrette.");
            Assert.IsNotEmpty(catalog.Cart.Models, "La charrette n'a pas de modèle du pack : on ne la verrait pas.");
            CollectionAssert.AllItemsAreNotNull(catalog.Cart.Models, "Un modèle de la charrette est vide.");
            CollectionAssert.AllItemsAreUnique(catalog.Cart.Models, "Un modèle de la charrette y est deux fois.");

            var faults = new List<string>();
            foreach (StructureDefinition structure in catalog.AllStructures())
            {
                int expected = structure.StorageCapacity > 0 ? 1 : 0;
                if (structure.Carts != expected)
                    faults.Add(structure.Id + " garde " + structure.Carts + " charrette(s) ; chaque entrepôt, le campement " +
                               "compris, a la sienne, et rien d'autre.");
            }
            Assert.IsEmpty(faults, string.Join("\n", faults));
            Assert.IsNotNull(catalog.Plaza, "Le catalogue n'a pas de Place.");
            Assert.Greater(catalog.Plaza.CartSpotsAt(0), 0, "La Place n'a pas de place de charrette au départ.");
        }

        [Test]
        public void EachCartModelFitsInACartSpotAndTheLargestFillsIt()
        {
            // Sans le pack (une copie publique du dépôt), il n'y a pas de charrette à mesurer : le test est passé, pas
            // échoué. Dans le dépôt de travail, le pack est là et le test tourne.
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Synty"))
                Assert.Ignore("Le pack n'est pas importé : ce test mesure ses modèles de charrette.");
            // Le modèle tel que le pack le pose, l'avant vers +z : sa longueur en z, sa largeur en x.
            CartDefinition cart = ProjectContent.LoadSingle<Catalog>().Cart;
            float longest = 0f, widest = 0f;
            foreach (GameObject model in cart.Models)
            {
                GameObject shown = Object.Instantiate(model);
                try
                {
                    Renderer[] renderers = shown.GetComponentsInChildren<Renderer>();
                    Bounds box = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                        box.Encapsulate(renderers[i].bounds);
                    Assert.LessOrEqual(box.size.z, cart.SpotLengthMeters + 0.05f, model.name + " déborde de sa place.");
                    Assert.LessOrEqual(box.size.x, cart.SpotWidthMeters + 0.05f, model.name + " déborde de sa place.");
                    longest = Mathf.Max(longest, box.size.z);
                    widest = Mathf.Max(widest, box.size.x);
                }
                finally
                {
                    Object.DestroyImmediate(shown);
                }
            }
            Assert.Greater(longest, cart.SpotLengthMeters - 0.2f, "La place a la longueur de la plus grande charrette.");
            Assert.Greater(widest, cart.SpotWidthMeters - 0.2f, "La place a la largeur de la plus grande charrette.");
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
