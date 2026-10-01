using System;
using System.Collections.Generic;
using Diciela.Simulation;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// L'outil « Retirer » (GDD, sections 2 et 16 ; décision 161) : en main, un clic sur un arbre ou un rocher debout le
    /// marque pour être retiré, ou le démarque s'il l'était ; un cadre tiré à la souris marque tous ceux dont le pied est
    /// dedans, du plus proche de son premier coin au plus loin : on les retirera dans cet ordre. Il parle au monde par les
    /// ordres du joueur ; Échap le lâche. Les gestes et les tests l'appellent par les mêmes méthodes.
    /// </summary>
    public sealed class ClearingTool : IEscapable, IHeldTool
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly NatureViews _views;
        private readonly List<KeyValuePair<float, int>> _framed = new List<KeyValuePair<float, int>>();

        public ClearingTool(World world, Camera camera, NatureViews views)
        {
            if (ReferenceEquals(world, null))
                throw new ArgumentNullException(nameof(world));
            if (camera == null || views == null)
                throw new ArgumentException("L'outil « Retirer » a besoin d'une caméra et des arbres vus.");

            _world = world;
            _camera = camera;
            _views = views;
        }

        /// <summary>Levé quand on prend l'outil en main, ou qu'on le lâche.</summary>
        public event Action HeldChanged;

        /// <summary>Vrai tant qu'il est en main.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Le prend en main.</summary>
        public void Take()
        {
            if (IsActive)
                return;
            IsActive = true;
            HeldChanged?.Invoke();
        }

        /// <summary>Le lâche ; faux s'il n'était pas en main.</summary>
        public bool Escape()
        {
            if (!IsActive)
                return false;
            IsActive = false;
            HeldChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Touche l'arbre ou le rocher qu'on voit sous ce point de l'écran : le marque, ou le démarque ; vrai si un ordre
        /// part.
        /// </summary>
        public bool TouchAt(Vector2 screenPoint)
        {
            if (!IsActive || !_views.TryPick(_camera.ScreenPointToRay(screenPoint), out NatureItem item))
                return false;
            int[] ids = { item.Id };
            _world.Submit(item.IsMarked ? (Command)new UnmarkForClearingCommand(ids) : new MarkForClearingCommand(ids));
            return true;
        }

        /// <summary>
        /// Marque les arbres et les rochers debout dont le pied est dans ce cadre de l'écran (en pixels, origine en bas à
        /// gauche), du plus proche du premier coin au plus loin ; rend combien.
        /// </summary>
        public int MarkWithin(Vector2 corner, Vector2 opposite)
        {
            if (!IsActive)
                return 0;
            Rect frame = Rect.MinMaxRect(Mathf.Min(corner.x, opposite.x), Mathf.Min(corner.y, opposite.y),
                Mathf.Max(corner.x, opposite.x), Mathf.Max(corner.y, opposite.y));
            Relief relief = _world.Map.Relief;
            _framed.Clear();
            foreach (NatureItem item in _world.Nature.All)
            {
                if (item.IsRemoved || item.IsMarked)
                    continue;
                Vector2 at = item.Placement.Center;
                Vector3 onScreen = _camera.WorldToScreenPoint(new Vector3(at.x, relief.HeightAt(at.x, at.y), at.y));
                if (onScreen.z > 0f && frame.Contains(onScreen))
                    _framed.Add(new KeyValuePair<float, int>(Vector2.Distance(onScreen, corner), item.Id));
            }
            if (_framed.Count == 0)
                return 0;
            _framed.Sort((a, b) => a.Key.CompareTo(b.Key));
            var ids = new int[_framed.Count];
            for (int i = 0; i < ids.Length; i++)
                ids[i] = _framed[i].Value;
            _world.Submit(new MarkForClearingCommand(ids));
            return ids.Length;
        }
    }
}
