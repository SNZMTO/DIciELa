using System;
using System.Collections;
using Diciela.Simulation.Land;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Diciela.Game
{
    /// <summary>
    /// La scène de la carte d'une session (ARCHITECTURE, section 10) : chargée par-dessus la scène racine et rendue
    /// active (sa lumière s'applique), lue en description pure, et déchargée au départ, même en plein chargement.
    /// </summary>
    internal sealed class MapScene
    {
        private readonly string _path;
        private AsyncOperation _loading;
        private Scene _scene;

        public MapScene(string path)
        {
            _path = path;
        }

        /// <summary>La description de la carte, une fois lue ; null tant qu'elle ne l'est pas, ou si elle ne peut pas.</summary>
        public MapDescription Description { get; private set; }

        /// <summary>Charge la carte, en fait la scène active et la lit ; une erreur s'écrit dans la console.</summary>
        public IEnumerator Load(Object context)
        {
            // Une carte déjà ouverte (dans la Hiérarchie de l'éditeur, à côté de Jeu) serait chargée une seconde fois :
            // deux Terrains, deux soleils, et la simulation lirait peut-être la copie qui n'est pas enregistrée.
            if (SceneManager.GetSceneByPath(_path).isLoaded)
            {
                Debug.LogError("DICIELA : la carte « " + _path + " » est déjà ouverte ; la fermer avant le Play.", context);
                yield break;
            }

            _loading = SceneLoader.LoadAdditive(_path);
            if (_loading == null)
            {
                Debug.LogError("DICIELA : la carte « " + _path + " » ne se charge pas.", context);
                yield break;
            }
            yield return _loading;
            _loading = null;

            _scene = SceneManager.GetSceneByPath(_path);
            SceneManager.SetActiveScene(_scene);
            MapRoot root = MapRoot.FindIn(_scene);
            if (root == null)
            {
                Debug.LogError("DICIELA : la carte « " + _path + " » n'a pas de racine (MapRoot).", context);
                yield break;
            }

            try
            {
                Description = MapReader.Read(root.Terrain.terrainData, root.transform.position);
            }
            catch (ArgumentException exception)
            {
                Debug.LogException(exception, context);
            }
        }

        /// <summary>Décharge la carte ; si elle se charge encore, elle partira à la fin de son chargement.</summary>
        public void Unload()
        {
            if (_loading != null)
            {
                string path = _path;
                _loading.completed += _ => SceneManager.UnloadSceneAsync(path);
            }
            else if (_scene.IsValid() && _scene.isLoaded)
                SceneManager.UnloadSceneAsync(_scene);
            _loading = null;
            _scene = default;
            Description = null;
        }
    }
}
