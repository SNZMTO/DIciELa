using System;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le maillage : un quadrillage invisible, plus fin que le mètre, qui dit ce qui occupe chaque maille (SAVOIR, 2.1).
    /// Il est déduit des constructions et des arbres et rochers, jamais sauvegardé : chaque maille garde l'identifiant de
    /// ce qui la couvre (0 : libre ; négatif : un arbre ou un rocher) et le nombre de portes qui en ont besoin pour rester
    /// dégagées. Une maille se désigne par un Vector2Int : en x sa colonne, en y sa rangée sur l'axe z du monde.
    /// </summary>
    public sealed class NavGrid
    {
        private readonly int[] _occupant;
        private readonly byte[] _doorCount;

        public NavGrid(Relief relief, GridMetrics metrics)
        {
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Metrics = metrics;
            Width = metrics.CellCount(relief.SizeX);
            Depth = metrics.CellCount(relief.SizeZ);
            _occupant = new int[Width * Depth];
            _doorCount = new byte[Width * Depth];
        }

        public GridMetrics Metrics { get; }
        public int Width { get; }
        public int Depth { get; }

        /// <summary>
        /// Le numéro de version du maillage : il change chaque fois que des mailles sont prises ou libérées. Ce qui se
        /// déduit du maillage (les îlots) se garde avec la version d'où il vient.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>Vrai si cette maille est dans le maillage.</summary>
        public bool Contains(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Depth;
        }

        /// <summary>L'identifiant de ce qui occupe cette maille, ou 0 si elle est libre.</summary>
        public int OccupantAt(Vector2Int cell)
        {
            return _occupant[IndexOf(cell)];
        }

        /// <summary>Vrai si cette maille doit rester dégagée devant une porte.</summary>
        public bool IsDoorway(Vector2Int cell)
        {
            return _doorCount[IndexOf(cell)] > 0;
        }

        /// <summary>
        /// Vrai si un piéton peut entrer dans cette maille : elle est libre. Un fantôme la prend dès sa pose, comme un
        /// bâtiment ; le devant d'une porte reste libre (sauf la rangée que touche son propre mur), les règles de pose y
        /// refusent tout. La seule réponse à « se traverse-t-elle ? ».
        /// </summary>
        public bool IsWalkable(Vector2Int cell)
        {
            return IsWalkable(IndexOf(cell));
        }

        /// <summary>La même réponse, pour la maille à cette place dans les tableaux du maillage.</summary>
        internal bool IsWalkable(int index)
        {
            return _occupant[index] == 0;
        }

        /// <summary>
        /// Vrai si l'on passe en diagonale de cette maille à sa voisine dans la direction step (un pas en x et un en y) :
        /// les deux mailles de côté, qui partagent le coin traversé, sont libres. Jamais de coin coupé : la même règle
        /// pour un pas de la recherche et pour une ligne droite qui passe par un coin.
        /// </summary>
        internal bool IsCornerOpen(Vector2Int cell, Vector2Int step)
        {
            return IsWalkable(new Vector2Int(cell.x + step.x, cell.y)) &&
                   IsWalkable(new Vector2Int(cell.x, cell.y + step.y));
        }

        /// <summary>Vrai si l'une des quatre voisines de cette maille est couverte par cet occupant.</summary>
        public bool Borders(Vector2Int cell, int occupant)
        {
            return IsHeldBy(new Vector2Int(cell.x - 1, cell.y), occupant) ||
                   IsHeldBy(new Vector2Int(cell.x + 1, cell.y), occupant) ||
                   IsHeldBy(new Vector2Int(cell.x, cell.y - 1), occupant) ||
                   IsHeldBy(new Vector2Int(cell.x, cell.y + 1), occupant);
        }

        private bool IsHeldBy(Vector2Int cell, int occupant)
        {
            return Contains(cell) && _occupant[IndexOf(cell)] == occupant;
        }

        /// <summary>Le milieu de cette maille, en mètres sur le plan XZ.</summary>
        public Vector2 CellCenter(Vector2Int cell)
        {
            return new Vector2(Metrics.CellCenter(cell.x), Metrics.CellCenter(cell.y));
        }

        /// <summary>La maille qui contient ce point, en mètres sur le plan XZ ; elle peut être hors du maillage.</summary>
        public Vector2Int CellOf(Vector2 point)
        {
            return new Vector2Int(Metrics.CellOf(point.x), Metrics.CellOf(point.y));
        }

        /// <summary>Les mailles que couvre ce rectangle (toutes celles qu'il touche), à parcourir avec foreach.</summary>
        public CoveredCells CellsCoveredBy(RotatedRect rect)
        {
            return new CoveredCells(this, rect);
        }

        /// <summary>Donne à chaque maille du rectangle cet occupant (0 pour la libérer).</summary>
        internal void SetOccupant(RotatedRect rect, int occupant)
        {
            foreach (Vector2Int cell in CellsCoveredBy(rect))
                _occupant[IndexOf(cell)] = occupant;
            Version++;
        }

        /// <summary>
        /// Libère les mailles du rectangle que tient encore cet occupant, et elles seules : un arbre retiré ne libère pas
        /// la maille que son voisin, peint contre lui, a gardée.
        /// </summary>
        internal void ReleaseOccupant(RotatedRect rect, int occupant)
        {
            foreach (Vector2Int cell in CellsCoveredBy(rect))
            {
                int index = IndexOf(cell);
                if (_occupant[index] == occupant)
                    _occupant[index] = 0;
            }
            Version++;
        }

        /// <summary>Compte une porte de plus devant chaque maille du rectangle.</summary>
        internal void AddDoorway(RotatedRect rect)
        {
            foreach (Vector2Int cell in CellsCoveredBy(rect))
            {
                int index = IndexOf(cell);
                if (_doorCount[index] < byte.MaxValue)
                    _doorCount[index]++;
            }
        }

        /// <summary>
        /// La place de la maille dans les tableaux du maillage, et dans ceux qui le suivent (la recherche des trajets) ;
        /// hors du maillage, une erreur, jamais la rangée voisine.
        /// </summary>
        internal int IndexOf(Vector2Int cell)
        {
            if (!Contains(cell))
                throw new ArgumentOutOfRangeException(nameof(cell), "La maille " + cell + " est hors du maillage.");
            return cell.y * Width + cell.x;
        }

        /// <summary>La maille qui est à cette place dans les tableaux : l'inverse de <see cref="IndexOf"/>.</summary>
        internal Vector2Int CellAt(int index)
        {
            if (index < 0 || index >= Width * Depth)
                throw new ArgumentOutOfRangeException(nameof(index), "La place " + index + " est hors du maillage.");
            return new Vector2Int(index % Width, index / Width);
        }
    }
}
