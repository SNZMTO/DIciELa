using System;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Game
{
    /// <summary>
    /// Le journal des arbres et des rochers : combien la carte en a, et chaque retrait, écrits dans la console pour qu'un
    /// Play court prouve que le défrichage travaille (ARCHITECTURE, section 12). Pas une ligne par marque : un cadre sur
    /// une forêt en marque des milliers d'un coup, et la console figerait l'éditeur. Une part du journal de la simulation.
    /// </summary>
    internal sealed class NatureJournal
    {
        private const string Prefix = "DICIELA : ";

        private readonly Nature _nature;

        public NatureJournal(Nature nature)
        {
            _nature = nature ?? throw new ArgumentNullException(nameof(nature));
            _nature.ItemRemoved += OnItemRemoved;
            Debug.Log(Prefix + "la carte a " + _nature.All.Count + " arbres et rochers.");
        }

        public void Stop()
        {
            _nature.ItemRemoved -= OnItemRemoved;
        }

        private static void OnItemRemoved(NatureItem item)
        {
            Debug.Log(Prefix + Describe(item) + " est retiré.");
        }

        private static string Describe(NatureItem item)
        {
            return "l'arbre ou rocher n° " + item.Id + " (" + item.Definition.Id + ", en " + item.Placement.Center + ")";
        }
    }
}
