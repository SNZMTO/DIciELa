using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Game;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Diciela.Tests.PlayMode
{
    /// <summary>
    /// Des arbres peints d'essai : un Terrain en mémoire de 40 × 40 m, plat, qui porte trois arbres. Deux sont d'un modèle
    /// rangé dans la sorte « arbre » (un cylindre du moteur, 2 m de haut) : l'instance 0 en (28 ; 22), large et haute de
    /// 1,5, tournée d'un quart de tour, et l'instance 2 en (10 ; 30). L'instance 1, en (10 ; 10), est d'un modèle rangé
    /// nulle part (un cube). Tout ce qu'il crée est détruit avec lui ; la vue détruite cesse de suivre le monde.
    /// </summary>
    internal sealed class TestPaintedNature : IDisposable
    {
        private readonly List<Object> _created = new List<Object>();

        public TestPaintedNature()
        {
            GameObject ranged = Keep(GameObject.CreatePrimitive(PrimitiveType.Cylinder));
            GameObject orphan = Keep(GameObject.CreatePrimitive(PrimitiveType.Cube));
            Data = Keep(new TerrainData { heightmapResolution = 33 });
            Data.size = new Vector3(40f, 10f, 40f);
            Data.treePrototypes = new[] { new TreePrototype { prefab = ranged }, new TreePrototype { prefab = orphan } };
            Data.SetTreeInstances(new[]
            {
                Instance(0, 28f, 22f, 1.5f, Mathf.PI / 2f), Instance(1, 10f, 10f, 1f, 0f), Instance(0, 10f, 30f, 1f, 0f)
            }, false);

            Tree = Keep(ScriptableObject.CreateInstance<NatureDefinition>());
#if UNITY_EDITOR
            TestWorld.Fill(Tree, "_id", "arbre");
            TestWorld.FillList(Tree, "_models", ranged);
            TestWorld.Fill(Tree, "_workHours", 0.5f);
#endif
        }

        public TerrainData Data { get; }
        public NatureDefinition Tree { get; }

        /// <summary>Lit la carte : sa description, et le numéro d'instance de chaque arbre dans instances.</summary>
        public MapDescription Read(List<int> instances)
        {
            return MapReader.Read(Data, Vector3.zero, null, new[] { Tree }, instances);
        }

        /// <summary>Le Terrain qui porte ces arbres, et leur vue, montrée sur cette nature.</summary>
        public NatureViews Show(Nature nature, IReadOnlyList<int> instances, Relief relief)
        {
            Terrain terrain = Keep(Terrain.CreateTerrainGameObject(Data)).GetComponent<Terrain>();
            var settings = Keep(ScriptableObject.CreateInstance<PlacementSettings>());
            var views = Keep(new GameObject("Nature")).AddComponent<NatureViews>();
#if UNITY_EDITOR
            TestWorld.Fill(settings, "_translucentMaterial",
                Keep(new Material(Shader.Find("Universal Render Pipeline/Lit"))));
            TestWorld.Fill(views, "_settings", settings);
#endif
            views.Show(nature, terrain, instances, relief);
            return views;
        }

        public void Dispose()
        {
            foreach (Object created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        private T Keep<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        private static TreeInstance Instance(int prototype, float x, float z, float scale, float rotation)
        {
            return new TreeInstance
            {
                prototypeIndex = prototype,
                position = new Vector3(x / 40f, 0f, z / 40f),
                widthScale = scale,
                heightScale = scale,
                rotation = rotation,
                color = Color.white,
                lightmapColor = Color.white
            };
        }
    }
}
