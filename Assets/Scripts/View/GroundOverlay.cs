using UnityEngine;
using UnityEngine.Rendering;

namespace Diciela.View
{
    /// <summary>
    /// Un calque posé sur le sol (décision 206) : un maillage drapé sur le relief (DrapedMesh), d'une seule couleur, du
    /// matériau translucide de la pose, sans ombre. Son rang dit l'ordre des calques transparents qui se recouvrent : le
    /// voile, puis les lignes, puis l'aperçu. Celui qui le crée le détruit.
    /// </summary>
    internal sealed class GroundOverlay
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly GameObject _object;
        private readonly Mesh _mesh;
        private readonly MeshRenderer _renderer;
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private Color _color;
        private bool _colored;

        public GroundOverlay(string name, Transform parent, Material material, int rank)
        {
            _object = new GameObject(name);
            _object.transform.SetParent(parent, false);
            _mesh = new Mesh { name = name };
            _mesh.MarkDynamic();
            _object.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = _object.AddComponent<MeshRenderer>();
            _renderer.sharedMaterial = material;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.sortingOrder = rank;
            _object.SetActive(false);
        }

        /// <summary>Prend ce qui a été construit.</summary>
        public void Apply(DrapedMesh built)
        {
            built.ApplyTo(_mesh);
        }

        public void SetColor(Color color)
        {
            if (_colored && color == _color)
                return;
            _colored = true;
            _color = color;
            _block.SetColor(BaseColorId, color);
            _renderer.SetPropertyBlock(_block);
        }

        public void SetVisible(bool visible)
        {
            if (_object.activeSelf != visible)
                _object.SetActive(visible);
        }

        public void Destroy()
        {
            Object.Destroy(_object);
            Object.Destroy(_mesh);
        }
    }
}
