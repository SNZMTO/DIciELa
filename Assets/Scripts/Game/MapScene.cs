using System;
using System.Collections;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Diciela.Game
{
    /// <summary>
    /// La scène de la carte d'une session (ARCHITECTURE, section 10) : chargée par-dessus la scène racine et rendue
    /// active (sa lumière s'applique), lue en description pure, et déchargée au départ, même en plein chargement. La
    /// partie joue sur une copie en mémoire de son sol, donnée au Terrain et à son collider, jamais enregistrée : retirer
    /// un arbre ne touche pas la carte du développeur (SAVOIR, 2.5).
    /// </summary>
    internal sealed class MapScene
    {
        private readonly string _path;
        private readonly IReadOnlyList<NatureDefinition> _nature;
        private readonly List<int> _natureInstances = new List<int>();
        private AsyncOperation _loading;
        private Scene _scene;
        private TerrainData _ground;

        /// <param name="nature">Les sortes d'arbres et de rochers du catalogue, pour lire ceux qui sont peints.</param>
        public MapScene(string path, IReadOnlyList<NatureDefinition> nature)
        {
            _path = path;
            _nature = nature;
        }

        /// <summary>La description de la carte, une fois lue ; null tant qu'elle ne l'est pas, ou si elle ne peut pas.</summary>
        public MapDescription Description { get; private set; }

        /// <summary>Le Terrain de la carte, qui porte la copie du sol ; null tant que la carte n'est pas lue.</summary>
        public Terrain Terrain { get; private set; }

        /// <summary>Pour chaque arbre ou rocher de la description, dans son ordre, le numéro de son instance sur le Terrain.</summary>
        public IReadOnlyList<int> NatureInstances => _natureInstances;

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

            Terrain = root.Terrain;
            _ground = Object.Instantiate(Terrain.terrainData);
            Terrain.terrainData = _ground;
            var collider = root.GetComponent<TerrainCollider>();
            if (collider != null)
                collider.terrainData = _ground;

            ArrivalPoint arrival = ArrivalPoint.FindIn(_scene);
            try
            {
                Description = MapReader.Read(_ground, root.transform.position,
                    arrival != null ? arrival.transform.position : (Vector3?)null, _nature, _natureInstances);
            }
            catch (ArgumentException exception)
            {
                Debug.LogException(exception, context);
            }
        }

        /// <summary>
        /// Décharge la carte ; si elle se charge encore, elle partira à la fin de son chargement. La copie du sol est
        /// détruite une fois la scène partie.
        /// </summary>
        public void Unload()
        {
            TerrainData ground = _ground;
            if (_loading != null)
            {
                string path = _path;
                _loading.completed += _ => SceneManager.UnloadSceneAsync(path);
            }
            else if (_scene.IsValid() && _scene.isLoaded)
            {
                AsyncOperation unloading = SceneManager.UnloadSceneAsync(_scene);
                if (ground != null && unloading != null)
                    unloading.completed += _ => Object.Destroy(ground);
                else if (ground != null)
                    Object.Destroy(ground);
            }
            else if (ground != null)
                Object.Destroy(ground);
            _loading = null;
            _scene = default;
            _ground = null;
            _natureInstances.Clear();
            Terrain = null;
            Description = null;
        }
    }
}
