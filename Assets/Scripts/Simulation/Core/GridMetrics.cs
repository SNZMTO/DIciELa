using System;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// La seule conversion entre mètres et mailles (ARCHITECTURE, section 3). La maille n de côté couvre les mètres de
    /// n / mailles-par-mètre (compris) à (n + 1) / mailles-par-mètre (exclu) ; l'origine est celle du monde.
    /// </summary>
    public readonly struct GridMetrics
    {
        public GridMetrics(int cellsPerMeter)
        {
            if (cellsPerMeter < 1)
                throw new ArgumentException("Il faut au moins une maille par mètre.", nameof(cellsPerMeter));

            CellsPerMeter = cellsPerMeter;
            CellSize = 1f / cellsPerMeter;
        }

        public int CellsPerMeter { get; }

        /// <summary>Le côté d'une maille, en mètres.</summary>
        public float CellSize { get; }

        /// <summary>La maille qui contient ce point, sur un axe.</summary>
        public int CellOf(float meters)
        {
            return (int)MathF.Floor(meters * CellsPerMeter);
        }

        /// <summary>Le milieu de cette maille, en mètres, sur un axe.</summary>
        public float CellCenter(int cell)
        {
            return (cell + 0.5f) * CellSize;
        }

        /// <summary>Le nombre de mailles qu'il faut pour couvrir une longueur partant de l'origine.</summary>
        public int CellCount(float meters)
        {
            return (int)MathF.Ceiling(meters * CellsPerMeter);
        }
    }
}
