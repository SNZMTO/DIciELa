using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// La file ouverte de la recherche des trajets (SAVOIR, 2.1) : un tas binaire, un arbre rangé dans un tableau où
    /// chaque parent passe avant ses deux enfants ; il rend toujours d'abord la maille la plus prometteuse, sans tout
    /// trier. Passe d'abord le plus petit total (coût payé plus coût restant estimé) ; à total égal, la maille la plus
    /// avancée (le coût payé le plus grand), si bien que sur l'herbe libre la recherche file droit au lieu de fouiller
    /// toute une bande. Le tableau grandit au besoin et se garde d'une recherche à l'autre.
    /// </summary>
    internal sealed class PathQueue
    {
        private Entry[] _entries = new Entry[64];
        private int _count;

        public int Count => _count;

        public void Clear()
        {
            _count = 0;
        }

        /// <summary>Ajoute une maille (sa place dans le maillage), son coût payé et son total.</summary>
        public void Push(int cell, int cost, int total)
        {
            if (_count == _entries.Length)
                Array.Resize(ref _entries, _entries.Length * 2);

            // L'entrée monte tant qu'elle passe avant son parent.
            var entry = new Entry(cell, cost, total);
            int child = _count++;
            while (child > 0)
            {
                int parent = (child - 1) / 2;
                if (!Before(entry, _entries[parent]))
                    break;
                _entries[child] = _entries[parent];
                child = parent;
            }
            _entries[child] = entry;
        }

        /// <summary>Retire la maille qui passe en premier, avec son coût payé.</summary>
        public void Pop(out int cell, out int cost)
        {
            if (_count == 0)
                throw new InvalidOperationException("La file ouverte est vide.");

            Entry first = _entries[0];
            Entry last = _entries[--_count];

            // La dernière entrée prend la place de la première, puis descend tant qu'un enfant passe avant elle.
            int parent = 0;
            while (2 * parent + 1 < _count)
            {
                int child = 2 * parent + 1;
                if (child + 1 < _count && Before(_entries[child + 1], _entries[child]))
                    child++;
                if (!Before(_entries[child], last))
                    break;
                _entries[parent] = _entries[child];
                parent = child;
            }
            _entries[parent] = last;

            cell = first.Cell;
            cost = first.Cost;
        }

        private static bool Before(Entry a, Entry b)
        {
            return a.Total < b.Total || (a.Total == b.Total && a.Cost > b.Cost);
        }

        private readonly struct Entry
        {
            public Entry(int cell, int cost, int total)
            {
                Cell = cell;
                Cost = cost;
                Total = total;
            }

            public int Cell { get; }
            public int Cost { get; }
            public int Total { get; }
        }
    }
}
