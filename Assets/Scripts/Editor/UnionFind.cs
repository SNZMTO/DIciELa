namespace Diciela.EditorTools
{
    /// <summary>
    /// Des groupes qui se rejoignent (union-find) : chaque élément part seul ; en joindre deux fond leurs groupes ; la
    /// racine d'un élément nomme son groupe. Sert à trouver les îlots d'un maillage.
    /// </summary>
    internal sealed class UnionFind
    {
        private readonly int[] _parent;

        public UnionFind(int count)
        {
            _parent = new int[count];
            for (int i = 0; i < count; i++)
                _parent[i] = i;
        }

        public void Join(int a, int b)
        {
            a = Root(a);
            b = Root(b);
            if (a != b)
                _parent[a] = b;
        }

        public int Root(int a)
        {
            while (_parent[a] != a)
            {
                _parent[a] = _parent[_parent[a]];
                a = _parent[a];
            }
            return a;
        }
    }
}
