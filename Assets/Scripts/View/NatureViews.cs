using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Diciela.View
{
    /// <summary>
    /// Les arbres et les rochers vus (GDD, section 2 ; décision 157) : peints sur le Terrain de la carte, dont la partie
    /// joue une copie, chacun retrouvé par le numéro de son instance. Retiré, il disparaît : sa taille passe à 0, sans
    /// renuméroter les autres. Marqué, un fanion se dresse à son pied (remplaçant simple, du matériau translucide) ; tous
    /// les fanions se dessinent d'un seul appel instancié par image, refaits seulement quand les marques changent : des
    /// centaines ne coûtent presque rien. La vue suit les événements de la simulation, jamais une photographie.
    /// </summary>
    public sealed class NatureViews : MonoBehaviour
    {
        // Le plus grand lot d'un appel instancié.
        private const int BatchSize = 1023;
        // Le fanion se plante à côté du pied, pas dedans : à cette distance de son bord, en mètres.
        private const float MarkGap = 0.15f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Tooltip("La fiche des réglages de la pose : le matériau translucide, la couleur et la hauteur du fanion.")]
        [SerializeField] private PlacementSettings _settings;

        private readonly List<Matrix4x4> _marks = new List<Matrix4x4>();
        private Nature _nature;
        private Terrain _terrain;
        private IReadOnlyList<int> _instances;
        private Relief _relief;
        private Mesh _mesh;
        private Material _material;
        private RenderParams _params;
        private bool _marksChanged;

        /// <summary>
        /// Montre les arbres et les rochers de ce monde, peints sur ce Terrain (null : aucun n'est peint), chacun à
        /// l'instance que la lecture de la carte lui a donnée ; puis suit les marques et les retraits.
        /// </summary>
        public void Show(Nature nature, Terrain terrain, IReadOnlyList<int> instances, Relief relief)
        {
            if (ReferenceEquals(nature, null))
                throw new ArgumentNullException(nameof(nature));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));
            if (_settings == null || _settings.TranslucentMaterial == null)
                throw new InvalidOperationException("Les arbres vus n'ont pas leur fiche de réglages de la pose.");
            if (terrain != null && (instances == null || instances.Count != nature.All.Count))
                throw new ArgumentException("Chaque arbre ou rocher de la carte doit avoir son instance sur le Terrain.");

            Hide();
            _nature = nature;
            _terrain = terrain;
            _instances = instances;
            _relief = relief;
            _mesh = MarkMesh.Build(_settings.MarkHeightMeters);
            _material = new Material(_settings.TranslucentMaterial) { enableInstancing = true };
            _material.SetColor(BaseColorId, _settings.MarkedColor);
            var size = new Vector3(relief.SizeX, 2000f, relief.SizeZ);
            _params = new RenderParams(_material)
            {
                layer = gameObject.layer,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                worldBounds = new Bounds(size * 0.5f, size)
            };

            foreach (NatureItem item in nature.All)
            {
                if (item.IsRemoved)
                    HideTree(item);
            }
            _marksChanged = true;
            nature.ItemMarked += OnMarksChanged;
            nature.ItemUnmarked += OnMarksChanged;
            nature.ItemRemoved += OnItemRemoved;
        }

        /// <summary>Cesse de suivre le monde et ne dessine plus rien.</summary>
        public void Hide()
        {
            if (_nature != null)
            {
                _nature.ItemMarked -= OnMarksChanged;
                _nature.ItemUnmarked -= OnMarksChanged;
                _nature.ItemRemoved -= OnItemRemoved;
            }
            _nature = null;
            _terrain = null;
            _instances = null;
            _relief = null;
            _marks.Clear();
            if (_mesh != null)
                Object.Destroy(_mesh);
            if (_material != null)
                Object.Destroy(_material);
            _mesh = null;
            _material = null;
        }

        private void OnDestroy()
        {
            Hide();
        }

        private void LateUpdate()
        {
            if (_nature == null)
                return;
            if (_marksChanged)
                RebuildMarks();
            for (int start = 0; start < _marks.Count; start += BatchSize)
                Graphics.RenderMeshInstanced(_params, _mesh, 0, _marks, Math.Min(BatchSize, _marks.Count - start), start);
        }

        private void OnMarksChanged(NatureItem item)
        {
            _marksChanged = true;
        }

        private void OnItemRemoved(NatureItem item)
        {
            HideTree(item);
            _marksChanged = true;
        }

        /// <summary>L'instance de cet arbre ou rocher sur le Terrain prend la taille 0 : on ne le voit plus.</summary>
        private void HideTree(NatureItem item)
        {
            if (_terrain == null)
                return;
            int index = _instances[item.Id - 1];
            TerrainData data = _terrain.terrainData;
            TreeInstance tree = data.GetTreeInstance(index);
            tree.widthScale = 0f;
            tree.heightScale = 0f;
            data.SetTreeInstance(index, tree);
        }

        /// <summary>Un fanion par arbre ou rocher marqué, planté contre le bord est de son pied, sur le sol.</summary>
        private void RebuildMarks()
        {
            _marksChanged = false;
            _marks.Clear();
            IReadOnlyList<NatureItem> marked = _nature.Marked;
            for (int i = 0; i < marked.Count; i++)
            {
                NatureItem item = marked[i];
                float halfWidth = item.Definition.WidthMeters * item.Scale * 0.5f;
                Vector2 foot = item.Placement.Center + new Vector2(halfWidth + MarkGap, 0f);
                var ground = new Vector3(foot.x, _relief.HeightAt(foot.x, foot.y), foot.y);
                _marks.Add(Matrix4x4.Translate(ground));
            }
        }
    }
}
