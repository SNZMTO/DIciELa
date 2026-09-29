using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Un maillage à facettes, en low poly comme le pack, construit triangle par triangle : chaque triangle a ses trois
    /// sommets à lui (pas de lissage : chaque face prend la lumière à sa façon), dans le sous-maillage de son matériau,
    /// et tourné vers le côté d'où on le voit (Unity ne montre qu'une face : celle dont les sommets tournent dans le sens
    /// des aiguilles d'une montre pour qui la regarde).
    /// </summary>
    internal sealed class FacetMesh
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int>[] _triangles;

        /// <summary>Un maillage vide, à tant de matériaux.</summary>
        public FacetMesh(int materials)
        {
            _triangles = new List<int>[materials];
            for (int i = 0; i < materials; i++)
                _triangles[i] = new List<int>();
        }

        /// <summary>Un triangle de ce matériau, vu du côté de <paramref name="facing"/>.</summary>
        public void Triangle(int material, Vector3 a, Vector3 b, Vector3 c, Vector3 facing)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), facing) < 0f)
                (b, c) = (c, b);
            int first = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            _triangles[material].Add(first);
            _triangles[material].Add(first + 1);
            _triangles[material].Add(first + 2);
        }

        /// <summary>Un quadrilatère de ce matériau (ses coins dans l'ordre du tour), en deux triangles.</summary>
        public void Quad(int material, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 facing)
        {
            Triangle(material, a, b, c, facing);
            Triangle(material, a, c, d, facing);
        }

        /// <summary>Écrit ce maillage dans celui-ci, qui garde son identité (et ce qui le référence).</summary>
        public void WriteTo(Mesh mesh)
        {
            mesh.Clear();
            mesh.SetVertices(_vertices);
            mesh.subMeshCount = _triangles.Length;
            for (int i = 0; i < _triangles.Length; i++)
                mesh.SetTriangles(_triangles[i], i);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
