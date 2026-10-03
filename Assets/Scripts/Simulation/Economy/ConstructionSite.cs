using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;

namespace Diciela.Simulation.Economy
{
    /// <summary>
    /// Un chantier (GDD, section 9) : une construction posée en fantôme, ses matériaux (demandés, livrés, en route) et
    /// son travail (fait, demandé), compté en heures d'un débutant. On n'y bâtit qu'une fois tous ses matériaux livrés.
    /// Ou le petit chantier d'un bâtiment qu'on déplace (décision 199) : sans matériaux, du travail pour le défaire, puis
    /// pour le remonter. Ce qui est en route et les ouvriers présents sont des réservations, jamais sauvegardées.
    /// </summary>
    public sealed class ConstructionSite
    {
        private readonly List<GoodDefinition> _goods = new List<GoodDefinition>();
        private readonly List<int> _needed = new List<int>();
        private readonly List<int> _delivered = new List<int>();
        private readonly List<int> _promised = new List<int>();

        internal ConstructionSite(Structure structure)
        {
            if (ReferenceEquals(structure, null))
                throw new ArgumentNullException(nameof(structure));

            Structure = structure;
            WorkNeeded = structure.Definition.WorkHours;
            // Une marchandise citée deux fois dans le coût ne fait qu'une ligne.
            foreach (GoodAmount cost in structure.Definition.Cost)
            {
                int line = _goods.IndexOf(cost.Good);
                if (line < 0)
                {
                    _goods.Add(cost.Good);
                    _needed.Add(0);
                    _delivered.Add(0);
                    _promised.Add(0);
                    line = _goods.Count - 1;
                }
                _needed[line] += cost.Amount;
            }
        }

        private ConstructionSite(Structure structure, double takeDownHours, double putBackHours)
        {
            Structure = structure ?? throw new ArgumentNullException(nameof(structure));
            IsMove = true;
            TakeDownHours = takeDownHours;
            WorkNeeded = takeDownHours + putBackHours;
        }

        /// <summary>
        /// Le petit chantier d'un bâtiment qu'on déplace : sans matériaux, tant d'heures pour le défaire, tant pour le
        /// remonter.
        /// </summary>
        internal static ConstructionSite ForMove(Structure structure, double takeDownHours, double putBackHours)
        {
            return new ConstructionSite(structure, takeDownHours, putBackHours);
        }

        public Structure Structure { get; }

        /// <summary>Vrai pour le petit chantier d'un bâtiment qu'on déplace.</summary>
        public bool IsMove { get; }

        /// <summary>Pour un déplacement, les heures de travail qui défont le bâtiment ; les suivantes le remontent.</summary>
        public double TakeDownHours { get; }

        /// <summary>Vrai une fois le bâtiment défait, puis sauté à sa nouvelle place.</summary>
        public bool HasLanded { get; private set; }

        /// <summary>Le nombre de lignes : une par marchandise demandée.</summary>
        public int LineCount => _goods.Count;

        public GoodDefinition GoodAt(int line)
        {
            return _goods[line];
        }

        public int NeededAt(int line)
        {
            return _needed[line];
        }

        public int DeliveredAt(int line)
        {
            return _delivered[line];
        }

        /// <summary>Ce qu'il manque de cette ligne, sans compter ce qui est déjà en route.</summary>
        public int MissingAt(int line)
        {
            return _needed[line] - _delivered[line] - _promised[line];
        }

        /// <summary>Vrai quand tous ses matériaux sont livrés : on peut y bâtir.</summary>
        public bool IsSupplied
        {
            get
            {
                for (int i = 0; i < _goods.Count; i++)
                {
                    if (_delivered[i] < _needed[i])
                        return false;
                }
                return true;
            }
        }

        /// <summary>Le travail fait, en heures d'un débutant.</summary>
        public double WorkDone { get; private set; }

        /// <summary>Le travail demandé, en heures d'un débutant.</summary>
        public double WorkNeeded { get; }

        /// <summary>La part du travail faite, de 0 à 1 ; un chantier sans travail demandé est à 1.</summary>
        public double Progress => WorkNeeded > 0d ? System.Math.Min(1d, WorkDone / WorkNeeded) : 1d;

        /// <summary>Le nombre d'ouvriers qui y vont ou y travaillent.</summary>
        public int Builders { get; private set; }

        /// <summary>Vrai une fois bâti : il n'est plus dans la file.</summary>
        public bool IsFinished { get; private set; }

        internal void Promise(GoodDefinition good, int amount)
        {
            _promised[Line(good)] += amount;
        }

        internal void Unpromise(GoodDefinition good, int amount)
        {
            _promised[Line(good)] -= amount;
        }

        internal void Receive(GoodDefinition good, int amount)
        {
            int line = Line(good);
            _promised[line] -= amount;
            _delivered[line] += amount;
        }

        internal void AddBuilder()
        {
            Builders++;
        }

        internal void RemoveBuilder()
        {
            Builders--;
        }

        /// <summary>Ajoute ce travail ; vrai s'il atteint le travail demandé.</summary>
        internal bool AddWork(double hours)
        {
            WorkDone += hours;
            return WorkDone >= WorkNeeded;
        }

        internal void MarkFinished()
        {
            IsFinished = true;
        }

        internal void MarkLanded()
        {
            HasLanded = true;
        }

        private int Line(GoodDefinition good)
        {
            int line = _goods.IndexOf(good);
            if (line < 0)
                throw new InvalidOperationException("Le chantier n° " + Structure.Id + " ne demande pas " + good.Id + ".");
            return line;
        }
    }
}
