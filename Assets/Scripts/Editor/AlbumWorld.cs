using Diciela.Game;
using Diciela.Simulation;
using UnityEditor;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>Le monde de la partie en cours, pour les mises en scène de l'album : null hors du Play ou avant sa naissance.</summary>
    internal static class AlbumWorld
    {
        public static World Running()
        {
            var runner = Object.FindAnyObjectByType<GameRunner>();
            return EditorApplication.isPlaying && runner != null ? runner.World : null;
        }
    }
}
