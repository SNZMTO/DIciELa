using Diciela.Data;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le portrait de la Chimère (GDD, section 16 ; décision 154) : une photo de sa tête, de face, prise à l'atelier sur
    /// son fond, comme dans la maquette. Une caméra le temps d'une image, placée devant sa face d'après les points mesurés
    /// de sa tête (le cou, le haut du crâne, les yeux), cadrée un peu plus large que la tête pour garder les oreilles.
    /// La texture rendue est à celui qui l'a demandée : il la détruit.
    /// </summary>
    public static class Portrait
    {
        private const float FieldOfView = 24f;
        // Le cadre fait la hauteur de la tête, fois ceci : les oreilles et le haut d'un chapeau y tiennent à peu près.
        private const float Framing = 1.35f;
        private const float NearMeters = 0.05f;
        private const float FarMeters = 20f;

        /// <summary>
        /// La photo carrée, de tant de pixels de côté, de la tête de ce personnage habillé ; null s'il n'y en a pas.
        /// </summary>
        public static Texture2D Take(GameObject character, HeadDefinition head, int pixels)
        {
            if (character == null || head == null || pixels < 1)
                return null;

            Transform body = character.transform;
            Vector3 neck = body.TransformPoint(head.PointOf(AttachmentFrame.Neck));
            Vector3 crown = body.TransformPoint(head.PointOf(AttachmentFrame.Crown));
            Vector3 face = body.TransformPoint(head.PointOf(AttachmentFrame.Face));
            // Le milieu de la tête, à la hauteur entre le cou et le crâne, à l'avant de la face.
            Vector3 target = new Vector3(face.x, (neck.y + crown.y) * 0.5f, face.z);
            float height = Mathf.Max(0.05f, (crown - neck).magnitude) * Framing;
            float distance = height * 0.5f / Mathf.Tan(FieldOfView * 0.5f * Mathf.Deg2Rad);

            var holder = new GameObject("Portrait de la Chimère");
            RenderTexture texture = RenderTexture.GetTemporary(pixels, pixels, 24, RenderTextureFormat.ARGB32);
            try
            {
                Camera camera = holder.AddComponent<Camera>();
                camera.enabled = false;
                camera.fieldOfView = FieldOfView;
                camera.nearClipPlane = NearMeters;
                camera.farClipPlane = FarMeters;
                camera.transform.position = target + body.forward * distance;
                camera.transform.LookAt(target);
                camera.targetTexture = texture;
                camera.Render();

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = texture;
                var photo = new Texture2D(pixels, pixels, TextureFormat.RGBA32, false);
                photo.ReadPixels(new Rect(0, 0, pixels, pixels), 0, 0);
                photo.Apply();
                RenderTexture.active = previous;
                return photo;
            }
            finally
            {
                RenderTexture.ReleaseTemporary(texture);
                Object.Destroy(holder);
            }
        }
    }
}
