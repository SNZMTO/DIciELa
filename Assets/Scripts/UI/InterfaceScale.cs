using Diciela.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// La taille de l'interface à l'écran (GDD, section 16 ; décision 144) : il donne au panneau son échelle chaque fois
    /// que l'écran (ou l'image où le panneau se rend) ou le réglage change. Dans l'éditeur, le panneau est un asset :
    /// son échelle d'origine lui est rendue à la sortie, pour que le Play ne le modifie pas.
    /// </summary>
    public sealed class InterfaceScale : MonoBehaviour
    {
        [SerializeField] private PanelSettings _panel;
        [SerializeField] private InterfaceSettings _settings;

        private float _original;
        private Vector2Int _screen;
        private float _size;

        /// <summary>
        /// L'échelle pour un écran de cette taille : la hauteur sur la hauteur de référence, fois le réglage ; jamais au
        /// point de laisser moins que la place minimale, et jamais sous 1, la taille de référence, qui l'emporte sur un
        /// écran trop petit.
        /// </summary>
        public static float Resolve(float width, float height, float referenceHeight, Vector2 minimumRoom, float size)
        {
            float wanted = height / referenceHeight * size;
            float roomiest = Mathf.Min(width / minimumRoom.x, height / minimumRoom.y);
            return Mathf.Max(1f, Mathf.Min(wanted, roomiest));
        }

        private void OnEnable()
        {
            if (_panel == null || _settings == null)
            {
                Debug.LogError("DICIELA : la taille de l'interface n'a pas son panneau ou ses réglages (lancer " +
                               "« DICIELA/Scènes/Monter les scènes ») ; l'interface garde sa taille.", this);
                enabled = false;
                return;
            }
            _original = _panel.scale;
            _screen = Vector2Int.zero;
        }

        private void Update()
        {
            // Un panneau qui se rend dans une image (hors de l'écran) prend la taille de cette image.
            RenderTexture target = _panel.targetTexture;
            Vector2Int screen = target != null
                ? new Vector2Int(target.width, target.height)
                : new Vector2Int(Screen.width, Screen.height);
            if (screen == _screen && Mathf.Approximately(_settings.Size, _size))
                return;
            _screen = screen;
            _size = _settings.Size;
            _panel.scale = Resolve(screen.x, screen.y, _settings.ReferenceHeight, _settings.MinimumRoom, _size);
        }

        private void OnDisable()
        {
            if (_panel != null && _settings != null)
                _panel.scale = _original;
        }
    }
}
