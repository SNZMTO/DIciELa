using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Un chantier vu (décision 166) : son modèle deux fois, au même endroit. La vraie matière (le shader
    /// Diciela/Chantier) ne garde que ce qui est sous la ligne ; le fantôme (Diciela/ChantierFantome, de la couleur du
    /// fantôme, TranslucentModel) ne garde que ce qui est au-dessus, et porte la lueur et les étincelles. L'un et l'autre
    /// détourent ce que l'original détoure (des feuilles et leurs tiges, le bord d'un chaume), et dessinent les mêmes
    /// faces. La ligne monte avec l'avancement, de la base de la boîte du modèle (ses fondations comprises) à son sommet,
    /// et ondule. Sa hauteur est la valeur propre de chaque rendu (SiteLine) : aucun matériau n'est créé par chantier,
    /// aucun bloc de propriétés ne l'éloigne du SRP Batcher (décision 181). Ce qui n'est pas un maillage (une fumée du
    /// pack) attend la fin du chantier.
    /// </summary>
    internal sealed class SiteFill
    {
        private readonly List<Renderer> _built = new List<Renderer>();
        private readonly TranslucentModel _ghost;
        private readonly float _bottom;
        private readonly float _height;
        // La part montrée : la ligne ne change que quand elle change.
        private double _progress = -1d;

        /// <summary>Fait de ce modèle posé un chantier : sa vraie matière, et son fantôme, copie rangée sous lui.</summary>
        public SiteFill(GameObject view, GameObject model, SiteMaterials materials, SiteGhosts ghosts)
        {
            var box = new Bounds();
            bool any = false;
            foreach (Renderer renderer in view.GetComponentsInChildren<Renderer>(true))
            {
                if (!TranslucentModel.KeepMesh(renderer))
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
            _ghost = new TranslucentModel(ghost, ghosts);
            Show(0d);
        }

        /// <summary>Monte la ligne à cette part du travail faite, de 0 (rien de bâti) à 1.</summary>
        public void Show(double progress)
        {
            if (progress == _progress)
                return;
            _progress = progress;
            float fill = _bottom + Mathf.Clamp01((float)progress) * _height;
            foreach (Renderer renderer in _built)
                SiteLine.Raise(renderer, fill);
            _ghost.Raise(fill);
        }
    }
}
