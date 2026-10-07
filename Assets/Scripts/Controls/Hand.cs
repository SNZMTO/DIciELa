using System;

namespace Diciela.Controls
{
    /// <summary>
    /// La main (GDD, section 16 ; décision 173) : un seul outil à la fois. Quand on en prend un, par n'importe quel chemin
    /// (la roue, une carte du panneau Construire, la touche d'un outil, un menu de l'album), elle lâche les autres. Le geste
    /// « toucher » y lit si un outil est en main : le clic gauche est alors à lui. Échap passe par elle : l'outil en main
    /// défait ce qu'il tient, ou se lâche (décision 215). Celui qui la crée la libère (Dispose).
    /// </summary>
    public sealed class Hand : IDisposable, IEscapable
    {
        private readonly IHeldTool[] _tools;
        private readonly Action[] _changed;

        public Hand(params IHeldTool[] tools)
        {
            if (tools == null || Array.IndexOf(tools, null) >= 0)
                throw new ArgumentException("La main a besoin de ses outils.");

            _tools = tools;
            _changed = new Action[tools.Length];
            for (int i = 0; i < tools.Length; i++)
            {
                IHeldTool tool = tools[i];
                _changed[i] = () => PutDownAllBut(tool);
                tool.HeldChanged += _changed[i];
            }
        }

        /// <summary>Vrai si un outil est en main.</summary>
        public bool IsHolding
        {
            get
            {
                foreach (IHeldTool tool in _tools)
                {
                    if (tool.IsActive)
                        return true;
                }
                return false;
            }
        }

        /// <summary>Échap : l'outil en main défait ce qu'il tient, ou se lâche ; faux si la main est vide.</summary>
        public bool Escape()
        {
            foreach (IHeldTool tool in _tools)
            {
                if (!tool.IsActive)
                    continue;
                tool.Cancel();
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            for (int i = 0; i < _tools.Length; i++)
                _tools[i].HeldChanged -= _changed[i];
        }

        // Cet outil vient de changer : s'il est en main, les autres se lâchent. Lâchés, ils changent à leur tour, mais ne
        // sont plus en main : rien ne repart.
        private void PutDownAllBut(IHeldTool taken)
        {
            if (!taken.IsActive)
                return;
            foreach (IHeldTool tool in _tools)
            {
                if (!ReferenceEquals(tool, taken) && tool.IsActive)
                    tool.PutDown();
            }
        }
    }
}
