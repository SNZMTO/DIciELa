using UnityEngine;
using UnityEngine.UIElements;

namespace Diciela.Data
{
    /// <summary>
    /// Une marchandise (GDD, section 8) : le Bois, la Pierre… Son nom et son icône, que montrent les fanions du stock et
    /// les fiches ; son encombrement, les places qu'elle prend dans un stock. Ce qu'elle vaut viendra avec l'étal.
    /// </summary>
    [CreateAssetMenu(menuName = "DICIELA/Marchandise", fileName = "Marchandise")]
    public sealed class GoodDefinition : ScriptableObject
    {
        [Tooltip("Identifiant stable, cité par les sauvegardes ; jamais le nom du fichier.")]
        [SerializeField] private string _id = "";

        [Tooltip("La clé de son nom dans la table des textes (Assets/Localization/strings.csv).")]
        [SerializeField] private string _nameKey = "";

        [Tooltip("Son icône (Assets/UI/Icones).")]
        [SerializeField] private VectorImage _icon;

        [Tooltip("Son encombrement : les places qu'une unité prend dans un stock, entrepôt, charge ou charrette (GDD, " +
                 "section 8 ; décision 194). 1 pour le Bois et la Pierre ; plus pour ce qui est encombrant.")]
        [SerializeField, Min(1)] private int _space = 1;

        public string Id => _id;
        public string NameKey => _nameKey;
        public VectorImage Icon => _icon;
        public int Space => Mathf.Max(1, _space);
    }
}
