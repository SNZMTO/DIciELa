using System.Collections.Generic;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les points d'une tête du pack, pris sur la surface de son crâne par des rayons (MeshRay), sur le personnage debout
    /// (SAVOIR, 2.11) : le haut du crâne, le milieu des yeux à leur avant, la place des oreilles sur le crâne, le bas de
    /// la tête sur la ligne du milieu, et la largeur du crâne à hauteur des sourcils, qui donne le rapport de taille entre
    /// deux têtes. Le haut du crâne et les oreilles se tiennent à mi-profondeur de la tête, à hauteur des yeux : la place
    /// du point le plus haut d'un crâne plat varie de plusieurs dizaines de centimètres d'une tête à l'autre. Les oreilles
    /// se prennent depuis le haut de la ligne du milieu, pas depuis les dômes des yeux de la grenouille. La même mesure
    /// sert aux têtes des animaux et à la tête humaine d'où viennent les pièces faites pour les humains.
    /// </summary>
    internal readonly struct HeadMeasures
    {
        // Le haut du crâne se cherche sur une bande de cette demi-largeur : les dômes des yeux de la grenouille, de part et
        // d'autre du milieu, sont le haut de sa tête.
        private const float CrownHalfBand = 0.35f;
        private const float Step = 0.01f;
        // Les rayons de la ligne du milieu passent à 5 mm d'elle : sur la couture même du modèle, un rayon peut filer entre
        // deux triangles sans en toucher aucun.
        private const float Midline = 0.005f;
        private const float Far = 5f;
        // La place d'une oreille : le crâne touché depuis son centre, à cet angle de la verticale, vers l'extérieur.
        private const float EarDegrees = 50f;

        private HeadMeasures(Vector3 crown, Vector3 face, Vector3 ears, Vector3 earRight, Vector3 neck, Vector3 nose,
            float browHeight, float browWidth, float browDepth)
        {
            BrowHeight = browHeight;
            BrowDepth = browDepth;
            Crown = crown;
            Face = face;
            Ears = ears;
            EarRight = earRight;
            Neck = neck;
            Nose = nose;
            BrowWidth = browWidth;
        }

        public Vector3 Crown { get; }
        public Vector3 Face { get; }
        public Vector3 Ears { get; }

        /// <summary>La place de l'oreille droite du personnage (x positif), sur son crâne.</summary>
        public Vector3 EarRight { get; }

        public Vector3 Neck { get; }

        /// <summary>Le bout du nez : le point le plus avancé du crâne sur la ligne du milieu, sous les yeux.</summary>
        public Vector3 Nose { get; }

        public float BrowWidth { get; }

        /// <summary>La hauteur de ses sourcils.</summary>
        public float BrowHeight { get; }

        /// <summary>La profondeur du crâne à hauteur des sourcils, sur la ligne du milieu.</summary>
        public float BrowDepth { get; }

        /// <summary>
        /// Les points d'une tête : les triangles de son crâne (sur ces positions), les sommets de ses yeux, la hauteur de
        /// ses sourcils.
        /// </summary>
        public static HeadMeasures Of(Vector3[] positions, List<int> skull, List<Vector3> eyes, float browHeight)
        {
            var ray = new MeshRay(positions, skull);
            var box = new Bounds(positions[skull[0]], Vector3.zero);
            foreach (int index in skull)
                box.Encapsulate(positions[index]);

            Vector3 face = FaceOf(eyes);
            float middle = MiddleOf(ray, box, face.y);
            var crown = new Vector3(0f, CrownOf(ray, box), middle);
            var center = new Vector3(0f, (MidlineTopOf(ray, box) + box.min.y) * 0.5f, middle);
            float angle = EarDegrees * Mathf.Deg2Rad;
            Vector3 right = Hit(ray, center, new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f));
            Vector3 left = Hit(ray, center, new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0f));
            var ears = new Vector3(0f, (right.y + left.y) * 0.5f, (right.z + left.z) * 0.5f);

            var brow = new Vector3(0f, browHeight, crown.z);
            float width = Hit(ray, brow + Vector3.right * Far, Vector3.left).x -
                          Hit(ray, brow + Vector3.left * Far, Vector3.right).x;
            Vector3 beside = brow + Vector3.right * Midline;
            float depth = Hit(ray, beside + Vector3.forward * Far, Vector3.back).z -
                          Hit(ray, beside + Vector3.back * Far, Vector3.forward).z;
            return new HeadMeasures(crown, face, ears, right, NeckOf(ray, box), NoseOf(ray, box, face.y), browHeight,
                width, depth);
        }

        // La hauteur du plus haut point du crâne sous une bande autour du milieu.
        private static float CrownOf(MeshRay ray, Bounds box)
        {
            float crown = float.MinValue;
            for (float x = -CrownHalfBand; x <= CrownHalfBand; x += Step * 2f)
                crown = Mathf.Max(crown, TopAt(ray, box, x));
            return crown;
        }

        // La hauteur du plus haut point du crâne sur la ligne du milieu.
        private static float MidlineTopOf(MeshRay ray, Bounds box)
        {
            return TopAt(ray, box, Midline);
        }

        private static float TopAt(MeshRay ray, Bounds box, float x)
        {
            float top = float.MinValue;
            for (float z = box.min.z; z <= box.max.z; z += Step)
            {
                var from = new Vector3(x, box.max.y + 1f, z);
                if (ray.TryHit(from, Vector3.down, out float distance))
                    top = Mathf.Max(top, from.y - distance);
            }
            return top;
        }

        // Le milieu de la tête d'avant en arrière, sur la ligne du milieu, à cette hauteur (celle des yeux).
        private static float MiddleOf(MeshRay ray, Bounds box, float height)
        {
            var from = new Vector3(Midline, height, 0f);
            float front = Hit(ray, from + Vector3.forward * (box.max.z + Far), Vector3.back).z;
            float back = Hit(ray, from + Vector3.back * (Far - box.min.z), Vector3.forward).z;
            return (front + back) * 0.5f;
        }

        // Le plus avancé des points du crâne sur la ligne du milieu, entre son bas et ses yeux.
        private static Vector3 NoseOf(MeshRay ray, Bounds box, float eyes)
        {
            var nose = new Vector3(0f, eyes, float.MinValue);
            for (float y = box.min.y; y <= eyes; y += Step)
            {
                var from = new Vector3(Midline, y, box.max.z + 1f);
                if (ray.TryHit(from, Vector3.back, out float distance) && from.z - distance > nose.z)
                    nose = new Vector3(0f, y, from.z - distance);
            }
            if (nose.z == float.MinValue)
                throw new System.InvalidOperationException("DICIELA : un rayon de mesure manque l'avant de la face.");
            return nose;
        }

        // Le plus bas point de la tête sur la ligne du milieu.
        private static Vector3 NeckOf(MeshRay ray, Bounds box)
        {
            var neck = new Vector3(0f, float.MaxValue, 0f);
            for (float z = box.min.z; z <= box.max.z; z += Step)
            {
                var from = new Vector3(Midline, box.min.y - 1f, z);
                if (ray.TryHit(from, Vector3.up, out float distance) && from.y + distance < neck.y)
                    neck = new Vector3(0f, from.y + distance, z);
            }
            if (neck.y == float.MaxValue)
                throw new System.InvalidOperationException("DICIELA : un rayon de mesure manque le bas de la tête.");
            return neck;
        }

        // Le milieu des centres des deux yeux, à l'avant du plus avancé.
        private static Vector3 FaceOf(List<Vector3> eyes)
        {
            Vector3 left = Vector3.zero, right = Vector3.zero;
            int lefts = 0, rights = 0;
            float front = float.MinValue;
            foreach (Vector3 eye in eyes)
            {
                front = Mathf.Max(front, eye.z);
                if (eye.x < 0f)
                {
                    left += eye;
                    lefts++;
                }
                else
                {
                    right += eye;
                    rights++;
                }
            }
            if (lefts == 0 || rights == 0)
                throw new System.InvalidOperationException("DICIELA : une tête sans ses deux yeux ne se mesure pas.");
            return new Vector3(0f, (left.y / lefts + right.y / rights) * 0.5f, front);
        }

        private static Vector3 Hit(MeshRay ray, Vector3 from, Vector3 direction)
        {
            direction.Normalize();
            if (!ray.TryHit(from, direction, out float distance))
                throw new System.InvalidOperationException("DICIELA : un rayon de mesure manque le crâne.");
            return from + direction * distance;
        }
    }
}
