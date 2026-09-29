using System.Collections.Generic;
using Diciela.Data;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Une silhouette d'animal du pack, découpée et mesurée, chargée à part sans toucher la scène ouverte : son crâne et son
    /// corps enregistrés (BodyCutter), ses morceaux détachables en mémoire (HeadPartCutter), les points de sa tête, la
    /// surface de son dos, et ce qu'il faut pour relever celle de sa tête.
    /// </summary>
    internal sealed class AnimalCut
    {

        private AnimalCut(GameObject model)
        {
            Model = model;
        }

        public GameObject Model { get; }
        public Mesh Skull { get; private set; }
        public Mesh Body { get; private set; }
        public Material Material { get; private set; }
        public Dictionary<HeadShells.Kind, HeadPartCutter.Part> Parts { get; } =
            new Dictionary<HeadShells.Kind, HeadPartCutter.Part>();

        /// <summary>La place des sommets du modèle debout, et les triangles de son crâne (yeux et sourcils compris).</summary>
        public Vector3[] Positions { get; private set; }

        public List<int> SkullTriangles { get; private set; }
        public HeadMeasures Measures { get; private set; }
        public SurfaceMap Back { get; private set; }
        public SurfaceMap Front { get; private set; }

        /// <summary>Découpe ce personnage du pack ; file nomme ses maillages (Chat_A).</summary>
        public static AnimalCut Of(GameObject model, string file, Rect backArea, float cell)
        {
            var cut = new AnimalCut(model);
            GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(model));
            try
            {
                SkinnedMeshRenderer skin = PackSkin.VisibleBody(root);
                HeadShells shells = HeadShells.Of(skin);
                (cut.Skull, cut.Body) = BodyCutter.Cut(skin, shells, file);
                cut.Material = skin.sharedMaterial;
                foreach (HeadShells.Kind kind in new[] { HeadShells.Kind.Ears, HeadShells.Kind.Tufts, HeadShells.Kind.Snout })
                {
                    if (HeadPartCutter.TryCut(skin, shells, kind, out HeadPartCutter.Part part))
                        cut.Parts[kind] = part;
                }
                // Le crâne se mesure refermé, comme il est enregistré : des oreilles cousues détachées y laissent un trou.
                var positions = new List<Vector3>(shells.Positions);
                cut.SkullTriangles = SkullCap.Closed(positions, shells.AllTrianglesOf(HeadShells.Kind.Skull));
                cut.Positions = positions.ToArray();
                cut.Measures = HeadMeasures.Of(cut.Positions, cut.SkullTriangles, shells.Eyes, shells.BrowHeight);
                List<int> body = shells.AllTrianglesOf(HeadShells.Kind.Body);
                cut.Back = SurfaceBaker.Surface(cut.Positions, body, ContactAxis.Forward, backArea, cell);
                cut.Front = SurfaceBaker.Surface(cut.Positions, body, ContactAxis.Back, backArea, cell);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return cut;
        }

        /// <summary>Sa couleur d'origine : celle de son corps allumé.</summary>
        public PaletteDefinition Palette(List<PaletteDefinition> palettes)
        {
            foreach (PaletteDefinition palette in palettes)
            {
                if (Material == palette.CharacterMaterial || Material == palette.ObjectMaterial)
                    return palette;
            }
            Debug.LogError("DICIELA : " + Model.name + " n'a pas une couleur du pack.", Model);
            return null;
        }
    }
}
