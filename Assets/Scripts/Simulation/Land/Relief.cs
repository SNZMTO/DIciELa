using System;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le relief de la carte : une grille régulière de hauteurs en mètres, dans le repère du monde (plan XZ, origine au
    /// coin de la carte). Entre quatre points, la hauteur se lit par interpolation bilinéaire, et la pente est celle de
    /// cette même surface : une seule façon de lire le sol.
    /// </summary>
    public sealed class Relief
    {
        private const float DegreesPerRadian = 180f / MathF.PI;

        private readonly float[] _heights;
        private readonly float _stepX;
        private readonly float _stepZ;

        /// <param name="heightsMeters">Les hauteurs, rangée par rangée : l'indice est z × samplesX + x. Copiées.</param>
        public Relief(float sizeX, float sizeZ, int samplesX, int samplesZ, float[] heightsMeters)
        {
            if (heightsMeters == null)
                throw new ArgumentNullException(nameof(heightsMeters));
            if (!(sizeX > 0f) || !(sizeZ > 0f))
                throw new ArgumentException("Une carte a une taille plus grande que zéro.");
            if (samplesX < 2 || samplesZ < 2)
                throw new ArgumentException("Un relief a au moins deux points de côté.");
            if (heightsMeters.Length != samplesX * samplesZ)
                throw new ArgumentException("Il faut une hauteur par point du relief.", nameof(heightsMeters));

            SizeX = sizeX;
            SizeZ = sizeZ;
            SamplesX = samplesX;
            SamplesZ = samplesZ;
            _heights = (float[])heightsMeters.Clone();
            _stepX = sizeX / (samplesX - 1);
            _stepZ = sizeZ / (samplesZ - 1);
        }

        public float SizeX { get; }
        public float SizeZ { get; }
        public int SamplesX { get; }
        public int SamplesZ { get; }

        public bool Contains(float x, float z)
        {
            return x >= 0f && x <= SizeX && z >= 0f && z <= SizeZ;
        }

        /// <summary>La hauteur du sol en mètres ; hors de la carte, celle du bord le plus proche.</summary>
        public float HeightAt(float x, float z)
        {
            Locate(x, z, out int ix, out int iz, out float tx, out float tz);
            float h00 = Sample(ix, iz), h10 = Sample(ix + 1, iz), h01 = Sample(ix, iz + 1), h11 = Sample(ix + 1, iz + 1);
            return (1f - tz) * ((1f - tx) * h00 + tx * h10) + tz * ((1f - tx) * h01 + tx * h11);
        }

        /// <summary>La pente du sol en degrés : 0 à plat, 45 quand on monte d'un mètre par mètre.</summary>
        public float SlopeDegreesAt(float x, float z)
        {
            Locate(x, z, out int ix, out int iz, out float tx, out float tz);
            float h00 = Sample(ix, iz), h10 = Sample(ix + 1, iz), h01 = Sample(ix, iz + 1), h11 = Sample(ix + 1, iz + 1);
            float riseX = ((1f - tz) * (h10 - h00) + tz * (h11 - h01)) / _stepX;
            float riseZ = ((1f - tx) * (h01 - h00) + tx * (h11 - h10)) / _stepZ;
            return MathF.Atan(MathF.Sqrt(riseX * riseX + riseZ * riseZ)) * DegreesPerRadian;
        }

        private float Sample(int ix, int iz)
        {
            return _heights[iz * SamplesX + ix];
        }

        /// <summary>La maille qui contient le point (ramené sur la carte) et la position du point dans cette maille.</summary>
        private void Locate(float x, float z, out int ix, out int iz, out float tx, out float tz)
        {
            float fx = Math.Clamp(x, 0f, SizeX) / _stepX;
            float fz = Math.Clamp(z, 0f, SizeZ) / _stepZ;
            ix = Math.Min((int)fx, SamplesX - 2);
            iz = Math.Min((int)fz, SamplesZ - 2);
            tx = fx - ix;
            tz = fz - iz;
        }
    }
}
