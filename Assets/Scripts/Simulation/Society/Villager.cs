using System;
using Diciela.Data;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.Simulation.Society
{
    /// <summary>
    /// Un habitant (ARCHITECTURE, section 4) : sa sorte, son apparence, sa place et ce qu'il fait. Sa place est sa
    /// position, en mètres sur le plan XZ ; il garde aussi celle du pas d'avant, pour que la vue le montre entre les
    /// deux. Sans poste, il flâne ; envoyé dormir, il va à son lit, s'y couche et n'en bouge plus jusqu'à ce qu'on le
    /// réveille. Une construction posée sur lui ne l'arrête pas : il en sort à pied, tout droit, par la maille libre qui
    /// la borde du côté du plus grand îlot (décisions 73 et 88), puis reprend ce qu'il faisait. Chaque pas tire son
    /// hasard de son numéro et du numéro du pas : les mêmes ordres rejouent la même partie.
    /// </summary>
    public sealed class Villager
    {
        private readonly NavGrid _grid;
        private readonly Islands _islands;
        private readonly Clock _clock;
        private readonly Walk _walk;
        private readonly Wander _wander;
        private readonly Bedtime _bedtime;
        private readonly WayOutFinder _wayOuts;
        private readonly float _metersPerStep;
        private bool _leaving;
        private Vector2 _wayOut;
        private bool _enclosed;
        // La version du maillage où il a cherché sa sortie : enfermé, il attend qu'elle change ; en sortant, il revérifie.
        private int _wayOutVersion;

        internal Villager(int id, VillagerKindDefinition kind, Appearance appearance, Vector2 position, NavGrid grid,
            Islands islands, PathFinder finder, Clock clock)
        {
            if (ReferenceEquals(kind, null))
                throw new ArgumentNullException(nameof(kind));
            if (ReferenceEquals(appearance, null))
                throw new ArgumentNullException(nameof(appearance));
            if (ReferenceEquals(grid, null))
                throw new ArgumentNullException(nameof(grid));
            if (ReferenceEquals(islands, null))
                throw new ArgumentNullException(nameof(islands));
            if (ReferenceEquals(finder, null))
                throw new ArgumentNullException(nameof(finder));
            if (ReferenceEquals(clock, null))
                throw new ArgumentNullException(nameof(clock));

            Id = id;
            Kind = kind;
            Appearance = appearance;
            Position = position;
            PreviousPosition = position;
            _grid = grid;
            _islands = islands;
            _clock = clock;
            _metersPerStep = clock.MetersPerStep(kind.WalkMetersPerSecond);
            _wayOuts = new WayOutFinder(grid, islands);
            _walk = new Walk(grid, finder);
            _wander = new Wander(kind, grid, islands, clock, _walk, _metersPerStep);
            _bedtime = new Bedtime(_walk, _metersPerStep);
            HashRandom random = RandomOfTheStep();
            _wander.Begin(position, ref random);
        }

        /// <summary>Son numéro dans ce monde ; jamais 0.</summary>
        public int Id { get; }

        public VillagerKindDefinition Kind { get; }

        /// <summary>Son corps, sa couleur et ses pièces : la vue l'habille ainsi.</summary>
        public Appearance Appearance { get; }

        /// <summary>Sa position au dernier pas, en mètres sur le plan XZ.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Sa position au pas d'avant : la vue le montre entre les deux.</summary>
        public Vector2 PreviousPosition { get; private set; }

        /// <summary>
        /// Vrai quand il marche le long d'un trajet ; pas quand il sort d'une construction posée sur lui, ni quand il y
        /// est enfermé, immobile.
        /// </summary>
        public bool IsWalking => _walk.IsWalking && !_leaving && !_enclosed;

        /// <summary>Le trajet qu'il suit, dont la ligne tendue ; il ne vaut que pendant qu'il marche.</summary>
        public Path Path => _walk.Path;

        /// <summary>Vrai pendant qu'il marche vers son lit.</summary>
        public bool IsGoingToBed => _bedtime.IsGoing;

        /// <summary>Vrai quand il dort, couché dans son logis : on ne le voit plus.</summary>
        public bool IsAsleep => _bedtime.IsAsleep;

        /// <summary>
        /// Joue un pas : endormi, rien ; sinon, sortir d'une construction posée sur lui d'abord, puis aller dormir ou
        /// flâner.
        /// </summary>
        internal void Step()
        {
            PreviousPosition = Position;
            if (_bedtime.IsAsleep)
                return;

            Vector2 position = Position;
            HashRandom random = RandomOfTheStep();
            if (!LeaveCoveredGround(ref position, ref random))
            {
                if (!_bedtime.IsGoing)
                    _wander.Step(ref position, ref random);
                else if (!_bedtime.Step(ref position))
                    _wander.Begin(position, ref random);
            }
            Position = position;
        }

        /// <summary>
        /// L'envoie dormir dans ce lit, le milieu de cette maille. Faux s'il ne peut pas l'atteindre d'où il est (il est
        /// encore sur une construction posée sur lui, ou son lit est sur un autre îlot) : rien ne change alors. S'il
        /// finit de sortir d'une construction, il en sort d'abord, puis va dormir. Seuls les habitants l'appellent, qui
        /// ont vérifié qu'il est debout.
        /// </summary>
        internal bool TryGoToBed(Vector2Int bed)
        {
            // Une maille prise est sur l'îlot 0 : sur un sol couvert, rien ne se rejoint.
            int island = _islands.IslandOf(_grid.CellOf(Position));
            if (island == 0 || _islands.IslandOf(bed) != island)
                return false;
            return _bedtime.TryStart(Position, bed);
        }

        /// <summary>
        /// Le réveille, ou l'arrête s'il allait dormir : il se remet à flâner autour d'où il est. Seuls les habitants
        /// l'appellent.
        /// </summary>
        internal void WakeUp()
        {
            _bedtime.Stop();
            HashRandom random = RandomOfTheStep();
            _wander.Begin(Position, ref random);
        }

        private HashRandom RandomOfTheStep()
        {
            // Au-delà de deux milliards de pas (sept ans de jeu), le numéro fait le tour : le hasard reste le même.
            return new HashRandom(Id, unchecked((int)_clock.StepCount));
        }

        /// <summary>
        /// Vrai tant qu'il sort d'une construction posée sur lui, ou qu'il y est enfermé : il ne fait rien d'autre. Le
        /// chemin de sortie traverse la construction ; il ne suit pas le maillage.
        /// </summary>
        private bool LeaveCoveredGround(ref Vector2 position, ref HashRandom random)
        {
            // Le maillage a changé pendant qu'il sort : si sa maille de sortie a été prise, il en cherche une autre.
            if (_leaving && _wayOutVersion != _grid.Version)
            {
                _wayOutVersion = _grid.Version;
                _leaving = _grid.IsWalkable(_grid.CellOf(_wayOut));
            }
            if (!_leaving)
            {
                if (_grid.IsWalkable(_grid.CellOf(position)))
                {
                    _enclosed = false;
                    return false;
                }
                // Enfermé : il attend que le maillage change pour chercher de nouveau.
                if (_enclosed && _wayOutVersion == _grid.Version)
                    return true;
                _wayOutVersion = _grid.Version;
                _enclosed = !_wayOuts.TryFind(position, out Vector2Int free);
                if (_enclosed)
                    return true;
                _wayOut = _grid.CellCenter(free);
                _leaving = true;
            }

            if (Vector2.Distance(position, _wayOut) > _metersPerStep)
            {
                position = Vector2.MoveTowards(position, _wayOut, _metersPerStep);
                return true;
            }
            position = _wayOut;
            _leaving = false;
            if (!_bedtime.IsGoing)
                _wander.Resume(position, ref random);
            else if (!_bedtime.Resume(position))
                _wander.Begin(position, ref random);
            return true;
        }
    }
}
