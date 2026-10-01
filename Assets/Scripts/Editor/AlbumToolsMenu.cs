using Diciela.UI;
using Diciela.View;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// La mise en scène de la roue des outils pour l'album (Docs/ALBUM.md), en Play, une fois la ville ouverte : la roue
    /// s'ouvre au milieu de l'écran par le même appel que sa touche tenue (le CLI ne sait pas tenir une touche), et le
    /// pointeur vise Construire, pour que la bulle du milieu parle. Échap la referme.
    /// </summary>
    public static class AlbumToolsMenu
    {
        // Où l'on vise : au-dessus du milieu, de cette part de la hauteur de l'écran.
        private const float AimShare = 0.2f;

        [MenuItem("DICIELA/Album/Ouvrir la roue des outils")]
        public static void OpenTheWheel()
        {
            var wheel = Object.FindAnyObjectByType<ToolWheel>();
            var rig = Object.FindAnyObjectByType<CameraRig>();
            if (!EditorApplication.isPlaying || wheel == null || rig == null || rig.Camera == null)
            {
                Debug.LogError("DICIELA : on ouvre la roue des outils en Play, une fois la ville ouverte.");
                return;
            }

            var middle = new Vector2(rig.Camera.pixelWidth / 2f, rig.Camera.pixelHeight / 2f);
            wheel.OpenAt(middle);
            wheel.PointAt(middle + Vector2.up * rig.Camera.pixelHeight * AimShare);
            Debug.Log("DICIELA : mise en scène de l'album : la roue des outils est ouverte, Construire visé.");
        }
    }
}
