using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Une sorte d'arbre ou de rocher de la carte (GDD, section 2) : les modèles du pack que le développeur peint sur le
    /// Terrain pour elle, la place que prend son pied au sol, ce qu'elle rend une fois retirée et le travail que cela
    /// demande. Tant qu'on ne l'a pas retirée, on ne la traverse pas et rien ne se pose dessus.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Arbre ou rocher", fileName = "Nature")]
    public sealed class NatureDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Les modèles du pack peints sur le Terrain de la carte pour cette sorte (Paint Trees) : chaque modèle " +
                 "peint doit être dans la liste d'une sorte, et d'une seule.")]
        [SerializeField] private List<GameObject> _models = new List<GameObject>();

        [Tooltip("Largeur de la place que prend son pied au sol, en mètres, à la taille 1 (le tronc d'un arbre).")]
        [SerializeField, Min(0.25f)] private float _widthMeters = 1f;

        [Tooltip("Profondeur de la place que prend son pied au sol, en mètres, à la taille 1.")]
        [SerializeField, Min(0.25f)] private float _depthMeters = 1f;

        [Tooltip("Ce qu'elle rend une fois retirée ; un habitant le porte en un voyage.")]
        [SerializeField] private GoodAmount _yield;

        [Tooltip("Les heures de travail qu'elle demande à un débutant ; un habitant plus fort va plus vite.")]
        [SerializeField, Min(0.01f)] private float _workHours = 0.5f;

        public string Id => _id;
        public string NameKey => _nameKey;
        public IReadOnlyList<GameObject> Models => _models;
        public float WidthMeters => _widthMeters;
        public float DepthMeters => _depthMeters;
        public GoodAmount Yield => _yield;
        public float WorkHours => _workHours;
    }
}
