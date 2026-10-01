using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Un chantier vu (décision 166) : son modèle deux fois, au même endroit. La vraie matière (le shader
    /// Diciela/Chantier) ne garde que ce qui est sous la ligne ; le fantôme (Diciela/ChantierFantome, de la couleur du
    /// fantôme) ne garde que ce qui est au-dessus, et porte la lueur et les étincelles. L'un et l'autre détourent ce que
    /// l'original détoure (des feuilles), et dessinent les mêmes faces. La ligne monte avec l'avancement,
    /// de la base de la boîte du modèle (ses fondations comprises) à son sommet, et ondule. Chaque couche a son bloc de
    /// propriétés : aucun matériau n'est créé par chantier. Ce qui n'est pas un maillage (une fumée du pack) attend la
    /// fin du chantier.
    /// </summary>
    internal sealed class SiteFill
    {
        private static readonly int FillHeightId = Shader.PropertyToID("_FillHeight");
        private static readonly int WaveAmplitudeId = Shader.PropertyToID("_WaveAmplitude");
        private static readonly int WaveLengthId = Shader.PropertyToID("_WaveLength");
        private static readonly int WaveSecondsId = Shader.PropertyToID("_WaveSeconds");
        private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly List<Renderer> _built = new List<Renderer>();
        private readonly List<Renderer> _ghost = new List<Renderer>();
        private readonly MaterialPropertyBlock _builtBlock = new MaterialPropertyBlock();
        private readonly MaterialPropertyBlock _ghostBlock = new MaterialPropertyBlock();
        private readonly PlacementSettings _settings;
        private readonly float _bottom;
        private readonly float _height;
        // La part montrée : les blocs ne changent que quand elle change.
        private double _progress = -1d;

        /// <summary>Fait de ce modèle posé un chantier : sa vraie matière, et son fantôme, copie rangée sous lui.</summary>
        public SiteFill(GameObject view, GameObject model, SiteMaterials materials, SiteGhosts ghosts,
            PlacementSettings settings)
        {
            _settings = settings;
            var box = new Bounds();
            bool any = false;
            foreach (Renderer renderer in view.GetComponentsInChildren<Renderer>(true))
            {
                if (!IsMesh(renderer))
                    continue;
                var shared = renderer.sharedMaterials;
                for (int i = 0; i < shared.Length; i++)
                    shared[i] = materials.For(shared[i]);
                renderer.sharedMaterials = shared;
                _built.Add(renderer);
                if (any)
                    box.Encapsulate(renderer.bounds);
                else
                    box = renderer.bounds;
                any = true;
            }
            _bottom = box.min.y;
            _height = box.size.y;

            GameObject ghost = ModelPose.Instantiate(model, view.transform, "Fantôme");
            ghost.transform.localPosition = Vector3.zero;
            ghost.transform.localRotation = Quaternion.identity;
            ghost.transform.localScale = Vector3.one;
            foreach (Renderer renderer in ghost.GetComponentsInChildren<Renderer>(true))
            {
                if (!IsMesh(renderer))
                    continue;
                var shared = renderer.sharedMaterials;
                for (int i = 0; i < shared.Length; i++)
                    shared[i] = ghosts.For(shared[i]);
                renderer.sharedMaterials = shared;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                _ghost.Add(renderer);
            }
            Show(0d);
        }

        /// <summary>Monte la ligne à cette part du travail faite, de 0 (rien de bâti) à 1.</summary>
        public void Show(double progress)
        {
            if (progress == _progress)
                return;
            _progress = progress;
            float fill = _bottom + Mathf.Clamp01((float)progress) * _height;
            Fill(_builtBlock, fill);
            Fill(_ghostBlock, fill);
            _ghostBlock.SetColor(BaseColorId, _settings.PlannedColor);
            foreach (Renderer renderer in _built)
                renderer.SetPropertyBlock(_builtBlock);
            foreach (Renderer renderer in _ghost)
                renderer.SetPropertyBlock(_ghostBlock);
        }

        private void Fill(MaterialPropertyBlock block, float fill)
        {
            block.SetFloat(FillHeightId, fill);
            block.SetFloat(WaveAmplitudeId, _settings.WaveAmplitude);
            block.SetFloat(WaveLengthId, _settings.WaveLength);
            block.SetFloat(WaveSecondsId, _settings.WaveSeconds);
            block.SetFloat(SoftnessId, _settings.SiteSoftness);
        }

        // Une fumée ou un effet du pack n'est pas dessiné tant que la construction n'est pas bâtie.
        private static bool IsMesh(Renderer renderer)
        {
            if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                return true;
            renderer.enabled = false;
            return false;
        }
    }
}
