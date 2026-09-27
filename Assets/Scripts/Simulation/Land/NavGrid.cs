using System;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le maillage : un quadrillage invisible, plus fin que le mètre, qui dit ce qui occupe chaque maille (SAVOIR, 2.1).
    /// Il est déduit des constructions, jamais sauvegardé : chaque maille garde l'identifiant de ce qui la couvre (0 :
    /// libre) et le nombre de portes qui en ont besoin pour rester dégagées.
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

        public int OccupantAt(int x, int z)
        {
            return _occupant[z * Width + x];
        }

        public bool IsDoorway(int x, int z)
        {
            return _doorCount[z * Width + x] > 0;
        }

        public Vector2 CellCenter(int x, int z)
        {
            return new Vector2(Metrics.CellCenter(x), Metrics.CellCenter(z));
        }

        /// <summary>Les mailles (bornes comprises, ramenées sur le maillage) dont le milieu peut tomber dans ce rectangle.</summary>
        public void CellRange(RotatedRect rect, out int minX, out int minZ, out int maxX, out int maxZ)
        {
            float lowX = float.MaxValue, lowZ = float.MaxValue, highX = float.MinValue, highZ = float.MinValue;
            for (int corner = 0; corner < 4; corner++)
            {
                Vector2 point = rect.Corner(corner);
                lowX = Mathf.Min(lowX, point.x);
                lowZ = Mathf.Min(lowZ, point.y);
                highX = Mathf.Max(highX, point.x);
                highZ = Mathf.Max(highZ, point.y);
            }
            minX = Math.Max(0, Metrics.CellOf(lowX));
            minZ = Math.Max(0, Metrics.CellOf(lowZ));
            maxX = Math.Min(Width - 1, Metrics.CellOf(highX));
            maxZ = Math.Min(Depth - 1, Metrics.CellOf(highZ));
        }

        /// <summary>Donne à chaque maille du rectangle cet occupant (0 pour la libérer).</summary>
        public void SetOccupant(RotatedRect rect, int occupant)
        {
            CellRange(rect, out int minX, out int minZ, out int maxX, out int maxZ);
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (rect.Contains(CellCenter(x, z)))
                        _occupant[z * Width + x] = occupant;
                }
            }
        }

        /// <summary>Compte une porte de plus devant chaque maille du rectangle.</summary>
        public void AddDoorway(RotatedRect rect)
        {
            CellRange(rect, out int minX, out int minZ, out int maxX, out int maxZ);
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int index = z * Width + x;
                    if (rect.Contains(CellCenter(x, z)) && _doorCount[index] < byte.MaxValue)
                        _doorCount[index]++;
                }
            }
        }
    }
}
