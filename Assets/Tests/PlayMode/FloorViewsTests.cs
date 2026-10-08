using System.Collections;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Les sols vus (décisions 209, 213, 216 et 228), par le chemin du joueur, sur le banc des sols (TestFloorBench)
    /// peint à 64 texels de côté (62,5 cm chacun) : un chemin tracé se voit en fantôme, à la texture de son calque,
    /// posée en tuiles comme sur le Terrain ; bâti par la Chimère, il se peint du calque de sa fiche, la terre, ajouté
    /// à la copie ; des pavés bâtis ensuite ajoutent leur calque sans défaire la terre ; un chemin qu'on efface se
    /// voile, puis rend la peinture de la carte. En prévision, un chemin tracé se peint déjà, un chemin qu'on efface
    /// est déjà parti, et les fantômes se cachent ; elle éteinte, tout redevient tel quel. L'original n'est jamais
    /// touché.
    /// </summary>
    public sealed class FloorViewsTests
    {
        private TestFloorBench _bench;
        private FloorDefinition _dirt;
        private FloorDefinition _paving;

        [SetUp]
        public void Create()
        {
            _bench = new TestFloorBench(64);
            TerrainLayer dirt = _bench.Layer("terre");
            dirt.tileSize = new Vector2(2f, 4f);
            dirt.tileOffset = new Vector2(1f, 0f);
            _dirt = _bench.Floor("terre", dirt);
            _paving = _bench.Floor("paves", _bench.Layer("paves"));
            _bench.Town.KnowsFloors(_dirt, _paving);
        }

        [TearDown]
        public void Destroy()
        {
            _bench.Dispose();
        }

        [UnityTest]
        public IEnumerator AFloorIsAGhostAtItsTextureThenItsLayerThenTheMapAgain()
        {
            World world = _bench.World;
            _bench.Trace(30f, _dirt);
            yield return null;
            GameObject dirtGhost = _bench.Child("Fantôme du sol terre");
            Assert.IsTrue(dirtGhost.activeSelf, "Tracé, le chemin se voit en fantôme…");
            Material ghost = dirtGhost.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreSame(_dirt.Layer.diffuseTexture, ghost.GetTexture("_BaseMap"), "… à la texture de son calque…");
            Assert.AreEqual(new Vector2(0.5f, 0.25f), ghost.GetTextureScale("_BaseMap"), "… en tuiles de 2 × 4 m…");
            Assert.AreEqual(new Vector2(0.5f, 0f), ghost.GetTextureOffset("_BaseMap"), "… décalées comme sur le Terrain.");
            Assert.IsFalse(_bench.Child("Voile des sols").activeSelf, "Un chemin qu'on trace n'est pas voilé.");
            Assert.AreEqual(1, _bench.Copy.alphamapLayers, "Pas encore bâti : rien n'est peint.");

            _bench.BuildAll();
            yield return null;
            Assert.IsFalse(dirtGhost.activeSelf, "Bâti, il n'a plus de fantôme…");
            Assert.AreEqual(2, _bench.Copy.alphamapLayers, "… et le calque de la terre s'ajoute à la copie.");
            CollectionAssert.AreEqual(new[] { 0f, 1f }, _bench.WeightsAt(20f, 30f), "Le chemin, de terre.");
            CollectionAssert.AreEqual(new[] { 1f, 0f }, _bench.WeightsAt(20f, 20f), "À côté, l'herbe de la carte.");

            _bench.Trace(6f, _paving);
            _bench.BuildAll();
            yield return null;
            CollectionAssert.AreEqual(new[] { 0f, 0f, 1f }, _bench.WeightsAt(20f, 6f), "Les pavés, de leur calque.");
            CollectionAssert.AreEqual(new[] { 0f, 1f, 0f }, _bench.WeightsAt(20f, 30f), "La terre reste.");

            Erase(30f);
            yield return null;
            Assert.IsTrue(_bench.Child("Voile des sols").activeSelf, "Qu'on l'efface, il se voile…");
            CollectionAssert.AreEqual(new[] { 0f, 1f, 0f }, _bench.WeightsAt(20f, 30f), "… et sert jusqu'à la fin.");
            _bench.BuildAll();
            yield return null;
            CollectionAssert.AreEqual(new[] { 1f, 0f, 0f }, _bench.WeightsAt(20f, 30f), "Effacé : l'herbe.");
            Assert.AreEqual(1, _bench.Original.alphamapLayers, "L'original n'est jamais touché.");
        }

        [UnityTest]
        public IEnumerator InForesightATracedFloorIsPaintedAndAnErasedOneIsGone()
        {
            _bench.Trace(30f, _dirt);
            _bench.BuildAll();
            _bench.Trace(10f, _dirt);
            Erase(30f);
            yield return null;
            CollectionAssert.AreEqual(new[] { 1f, 0f }, _bench.WeightsAt(20f, 10f), "Tracé, il n'est pas peint.");
            CollectionAssert.AreEqual(new[] { 0f, 1f }, _bench.WeightsAt(20f, 30f), "Qu'on l'efface, il sert encore.");

            _bench.Views.ShowForesight(true);
            yield return null;
            CollectionAssert.AreEqual(new[] { 0f, 1f }, _bench.WeightsAt(20f, 10f), "En prévision, le tracé se peint…");
            CollectionAssert.AreEqual(new[] { 1f, 0f }, _bench.WeightsAt(20f, 30f), "… et l'effacé est déjà parti…");
            Assert.IsFalse(_bench.Child("Fantôme du sol terre").activeSelf, "… sans fantôme…");
            Assert.IsFalse(_bench.Child("Voile des sols").activeSelf, "… ni voile.");

            _bench.Views.ShowForesight(false);
            yield return null;
            CollectionAssert.AreEqual(new[] { 1f, 0f }, _bench.WeightsAt(20f, 10f), "Elle éteinte, tout est tel quel.");
            CollectionAssert.AreEqual(new[] { 0f, 1f }, _bench.WeightsAt(20f, 30f));
            Assert.IsTrue(_bench.Child("Fantôme du sol terre").activeSelf, "Le fantôme revient.");
            Assert.IsTrue(_bench.Child("Voile des sols").activeSelf, "Le voile aussi.");
        }

        // Efface, par l'ordre du joueur, le chemin de x = 10 à 30 à cette hauteur.
        private void Erase(float z)
        {
            var box = new GroundBox(new RotatedRect(new Vector2(20f, z), new Vector2(30f, 4f), 0f));
            _bench.World.Submit(new EraseFloorCommand(box, FloorScope.Anywhere));
            _bench.World.Step();
        }
    }
}
