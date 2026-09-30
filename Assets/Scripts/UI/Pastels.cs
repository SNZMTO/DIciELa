using Diciela.Data;

namespace Diciela.UI
{
    /// <summary>
    /// La classe de la feuille de style de chaque pastel (Fusion.uss) : son nom en minuscules (Rose → rose). La version
    /// pâle d'un pastel, pour les cartes, ajoute « -pale ».
    /// </summary>
    public static class Pastels
    {
        private static readonly string[] Classes = Build();

        public static string ClassOf(Pastel pastel)
        {
            int index = (int)pastel;
            return index >= 0 && index < Classes.Length ? Classes[index] : Classes[0];
        }

        private static string[] Build()
        {
            var values = (Pastel[])System.Enum.GetValues(typeof(Pastel));
            var classes = new string[values.Length];
            foreach (Pastel pastel in values)
                classes[(int)pastel] = pastel.ToString().ToLowerInvariant();
            return classes;
        }
    }
}
